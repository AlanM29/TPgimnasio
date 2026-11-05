using System;
using System.Collections.Generic;
using System.Data.SQLite;

//  UsuarioDAO
public class UsuarioDAO : DaoBase, IUsuarioDAO
{
    // Une Usuario con Empleados, para traer sus datos personales y su estado.
    private const string Seleccion =
        "SELECT u.id_usuario, u.nombre_usuario, u.contrasenahash, u.rol, " +
        "       e.dni, e.nombre, e.apellido, e.email, e.telefono, e.fecha_ingreso, e.estado " +
        "FROM Usuario u " +
        "INNER JOIN Empleados e ON e.id_empleado = u.id_empleado ";

    //--- CONSULTA ---

    // Solo deja entrar a empleados Activo
    public UsuarioBase? ObtenerPorCredenciales(string nombreUsuario, string contrasenaHash)
    {
        return ConsultarUno<UsuarioBase>(
            Seleccion + "WHERE u.nombre_usuario = @nombre " +
                        "  AND LOWER(u.contrasenahash) = @clave " +
                        "  AND e.estado = 'Activo'",
            Mapear,
            ("@nombre", nombreUsuario),
            ("@clave", contrasenaHash.ToLower()));
    }

    public UsuarioBase? ObtenerPorId(int id)
    {
        return ConsultarUno<UsuarioBase>(Seleccion + "WHERE u.id_usuario = @id", Mapear, ("@id", id));
    }

    public UsuarioBase? ObtenerPorNombreUsuario(string nombreUsuario)
    {
        return ConsultarUno<UsuarioBase>(Seleccion + "WHERE u.nombre_usuario = @nombre",
                                         Mapear, ("@nombre", nombreUsuario));
    }

    public List<UsuarioBase> ObtenerTodos(bool soloActivos)
    {
        string condicion = soloActivos ? "WHERE e.estado = 'Activo' " : "";
        return ConsultarLista<UsuarioBase>(Seleccion + condicion + "ORDER BY u.nombre_usuario", Mapear);
    }

    // --- ALTA ---
    public int Insertar(UsuarioBase usuario)
    {
        try
        {
            using (SQLiteConnection conexion = ConexionBd.ObtenerConexion())
            {
                using (SQLiteTransaction transaccion = (SQLiteTransaction)conexion.BeginTransaction())
                {
                    string sqlEmpleado =
                        "INSERT INTO Empleados (dni, nombre, apellido, email, telefono, puesto, fecha_ingreso, estado) " +
                        "VALUES (@dni, @nombre, @apellido, @email, @telefono, @puesto, @fecha, @estado)";

                    using (SQLiteCommand comando = new SQLiteCommand(sqlEmpleado, conexion, transaccion))
                    {
                        AgregarParametros(comando,
                            ("@dni", usuario.Dni),
                            ("@nombre", usuario.Nombre),
                            ("@apellido", usuario.Apellido),
                            ("@email", usuario.Email),
                            ("@telefono", usuario.Telefono),
                            ("@puesto", PuestoDe(usuario.Rol)),
                            ("@fecha", FechaATexto(usuario.FechaRegistro)),
                            ("@estado", usuario.Activo ? "Activo" : "Inactivo"));
                        comando.ExecuteNonQuery();
                    }
                    long idEmpleado = conexion.LastInsertRowId;

                    string sqlUsuario =
                        "INSERT INTO Usuario (id_empleado, nombre_usuario, contrasenahash, rol) " +
                        "VALUES (@empleado, @usuario, @hash, @rol)";

                    using (SQLiteCommand comando = new SQLiteCommand(sqlUsuario, conexion, transaccion))
                    {
                        AgregarParametros(comando,
                            ("@empleado", idEmpleado),
                            ("@usuario", usuario.Nombre_Usuario),
                            ("@hash", usuario.Contrasenahash),
                            ("@rol", RolEnBase(usuario.Rol)));
                        comando.ExecuteNonQuery();
                    }
                    int idUsuario = (int)conexion.LastInsertRowId;

                    transaccion.Commit();
                    usuario.AsignarId(idUsuario);
                    return idUsuario;
                }
            }
        }
        catch (SQLiteException ex) when (EsDuplicado(ex))
        {
            if (ex.Message.Contains("Usuario.nombre_usuario"))
            {
                throw new InvalidOperationException("Ya existe un usuario con ese nombre de usuario.", ex);
            }
            if (ex.Message.Contains("Empleados.dni"))
            {
                throw new InvalidOperationException("Ya existe un empleado con ese DNI.", ex);
            }
            if (ex.Message.Contains("Empleados.email"))
            {
                throw new InvalidOperationException("Ya existe un empleado con ese email.", ex);
            }
            throw new InvalidOperationException("Ya existe un usuario con esos datos.", ex);
        }
    }

    // --- MODIFICACIÓN ---

    public bool Actualizar(UsuarioBase usuario)
    {
        try
        {
            using (SQLiteConnection conexion = ConexionBd.ObtenerConexion())
            {
                using (SQLiteTransaction transaccion = (SQLiteTransaction)conexion.BeginTransaction())
                {
                    // Los datos personales están en Empleados.
                    string sqlEmpleado =
                        "UPDATE Empleados SET nombre = @nombre, apellido = @apellido, " +
                        "email = @email, telefono = @telefono " +
                        "WHERE id_empleado = (SELECT id_empleado FROM Usuario WHERE id_usuario = @id)";

                    int filas;
                    using (SQLiteCommand comando = new SQLiteCommand(sqlEmpleado, conexion, transaccion))
                    {
                        AgregarParametros(comando,
                            ("@nombre", usuario.Nombre),
                            ("@apellido", usuario.Apellido),
                            ("@email", usuario.Email),
                            ("@telefono", usuario.Telefono),
                            ("@id", usuario.Id));
                        filas = comando.ExecuteNonQuery();
                    }

                    using (SQLiteCommand comando = new SQLiteCommand(
                        "UPDATE Usuario SET contrasenahash = @hash WHERE id_usuario = @id", conexion, transaccion))
                    {
                        AgregarParametros(comando,
                            ("@hash", usuario.Contrasenahash),
                            ("@id", usuario.Id));
                        comando.ExecuteNonQuery();
                    }

                    transaccion.Commit();
                    return filas > 0;
                }
            }
        }
        catch (SQLiteException ex) when (EsDuplicado(ex))
        {
            throw new InvalidOperationException("Ya existe otro empleado con ese email.", ex);
        }
    }

    // ---------- BAJA ----------

    public bool DarDeBaja(int id)
    {
        return CambiarEstado(id, "Inactivo");
    }

    public bool Reactivar(int id)
    {
        return CambiarEstado(id, "Activo");
    }

    private bool CambiarEstado(int id, string estado)
    {
        string sql = "UPDATE Empleados SET estado = @estado " +
                     "WHERE id_empleado = (SELECT id_empleado FROM Usuario WHERE id_usuario = @id)";
        return EjecutarNoQuery(sql, ("@estado", estado), ("@id", id)) > 0;
    }

    // --- CONVERSIONES ---
    private UsuarioBase Mapear(SQLiteDataReader lector)
    {
        int id = lector.GetInt32(0);
        string nombreUsuario = lector.GetString(1);
        string hash = lector.GetString(2);
        string rolEnBase = lector.GetString(3);
        string dni = lector.GetString(4);
        string nombre = lector.GetString(5);
        string apellido = lector.GetString(6);
        string? email = lector.IsDBNull(7) ? null : lector.GetString(7);
        string? telefono = lector.IsDBNull(8) ? null : lector.GetString(8);
        DateTime fecha = TextoAFecha(lector.GetString(9));
        bool activo = lector.GetString(10) == "Activo";

        return UsuarioFactory.Crear(TraducirRol(rolEnBase), nombreUsuario, hash, nombre, apellido, dni, email, telefono, fecha, activo, id);
    }

    // Base -> modelo
    private string TraducirRol(string rolEnBase)
    {
        if (rolEnBase == "admin")
        {
            return "Admin";
        }
        if (rolEnBase == "usuario")
        {
            return "Recepcionista";
        }
        throw new ArgumentException("Rol desconocido en la base de datos: '" + rolEnBase + "'.");
    }

    // Modelo -> base
    private string RolEnBase(string rol)
    {
        if (rol == "Admin")
        {
            return "admin";
        }
        if (rol == "Recepcionista")
        {
            return "usuario";
        }
        throw new ArgumentException("Rol desconocido: '" + rol + "'.");
    }

    // Texto para la columna "puesto" de Empleados
    private string PuestoDe(string rol)
    {
        return rol == "Admin" ? "Administrador" : "Recepcionista";
    }
}
