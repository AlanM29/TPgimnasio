using System;
using System.Globalization;
using System.Data.SQLite;

//  DAO: UsuarioDAO 
//  Es la ÚNICA clase que consulta la tabla Usuario.

public class UsuarioDAO : IUsuarioDAO
{
    // Busca un usuario con ese nombre Y esa contraseña .
    // Une Usuario con Empleados , para traer sus datos personales y para no dejar entrar a Empleados dados de baja.
    public UsuarioBase? ObtenerPorCredenciales(string nombreUsuario, string contrasenaHash)
    {
        try
        {
            using (SQLiteConnection conexion = ConexionBd.ObtenerConexion())
            {
                // @nombre y @clave son parámetros
                string sql = "SELECT u.id_usuario, u.nombre_usuario, u.contrasenahash, u.rol, " +
                             "       e.dni, e.nombre, e.apellido, e.email, e.telefono, e.fecha_ingreso " +
                             "FROM Usuario u " +
                             "INNER JOIN Empleados e ON e.id_empleado = u.id_empleado " +
                             "WHERE u.nombre_usuario = @nombre " +
                             "  AND LOWER(u.contrasenahash) = @clave " +
                             "  AND e.estado = 'Activo'";

                using (SQLiteCommand comando = new SQLiteCommand(sql, conexion))
                {
                    comando.Parameters.AddWithValue("@nombre", nombreUsuario);
                    comando.Parameters.AddWithValue("@clave", contrasenaHash.ToLower());

                    using (SQLiteDataReader lector = comando.ExecuteReader())
                    {
                        if (lector.Read())
                        {
                            return CrearDesdeFila(lector);
                        }
                    }
                }
            }
        }
        catch (SQLiteException ex)
        {
            Console.WriteLine("Error de base de datos: " + ex.Message);
        }
        catch (ArgumentException ex)
        {
            // Las validaciones de UsuarioBase (o la fábrica) rechazaron un dato guardado en la base
            Console.WriteLine("Los datos del usuario en la base no son válidos: " + ex.Message);
        }

        return null;
    }

    // Convierte la fila en el objeto del tipo correcto usando la UsuarioFactory
   
    private UsuarioBase CrearDesdeFila(SQLiteDataReader lector)
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
        DateTime fecha = LeerFecha(lector.GetString(9));

        return UsuarioFactory.Crear(TraducirRol(rolEnBase), nombreUsuario, hash, nombre, apellido,
                                    dni, email, telefono, fecha, true, id);
    }


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

    // Las fechas están guardadas como texto: "2026-10-30 22:22:57"
    private DateTime LeerFecha(string texto)
    {
        DateTime fecha;
        if (DateTime.TryParse(texto, CultureInfo.InvariantCulture, DateTimeStyles.None, out fecha))
        {
            return fecha;
        }
        return DateTime.Now;
    }
}
