using System.Data.SQLite;
using System.Globalization;
public class SocioDAO : ISocioDAO
{
    private const string FormatoFecha = "yyyy-MM-dd HH:mm:ss";
    // Texto fijo escrito en este archivo (nunca viene del usuario), por eso se puede concatenar
    private const string Columnas = "id_socio, dni, nombre, apellido, email, telefono, estado, fecha_alta";

    public Socio? ObtenerPorId(int id)
    {
        return ObtenerUno("WHERE id_socio = @valor", id);// objeto anónimo { valor = id }
    }
    public Socio? ObtenerPorDni(string dni)
    {
        return ObtenerUno("WHERE dni = @valor", dni);//hace lo mismo que por id, pero con el dni
    }
    public List<Socio> ObtenerTodos(bool SoloActivos)
    {
        string condicion = SoloActivos ? "WHERE estado = 'Activo' " : "";
        return ObtenerLista("SELECT " + Columnas + " FROM Socios " + condicion +
                            "ORDER BY nombre, apellido", null);
    }
    public List<Socio> BuscarPorApellido(string texto)//busca por apellido, devuelve lista vacia si no encuentra
    {
        return ObtenerLista("SELECT " + Columnas + " FROM Socios " +
                            "WHERE apellido LIKE @valor ORDER BY apellido, nombre",// el parametro @valor se reemplaza por el valor del texto con comodines
                            "%" + texto + "%");
    }
    public int Insertar(Socio socio)
    {
        try
        {
            using (SQLiteConnection conexion = ConexionBd.ObtenerConexion())
            {
                string sql = "INSERT INTO Socios (dni, nombre, apellido, email, telefono, estado, fecha_alta) " +
                             "VALUES (@dni, @nombre, @apellido, @email, @telefono, @estado, @fecha)";

                using (SQLiteCommand comando = new SQLiteCommand(sql, conexion))
                {
                    comando.Parameters.AddWithValue("@dni", socio.Dni);
                    comando.Parameters.AddWithValue("@nombre", socio.Nombre);
                    comando.Parameters.AddWithValue("@apellido", socio.Apellido);
                    comando.Parameters.AddWithValue("@email", (object?)socio.Email ?? DBNull.Value);// si es null,se guarda DBNull.Value
                    comando.Parameters.AddWithValue("@telefono", (object?)socio.Telefono ?? DBNull.Value);// si es null,se guarda DBNull.Value
                    comando.Parameters.AddWithValue("@estado", socio.Activo ? "Activo" : "Inactivo");
                    comando.Parameters.AddWithValue("@fecha",
                    socio.FechaRegistro.ToString(FormatoFecha, CultureInfo.InvariantCulture));
                    comando.ExecuteNonQuery();// Ejecuta la inserción y genera el id automáticamente
                }
                int id = (int)conexion.LastInsertRowId;
                socio.AsignarId(id);
                return id;
            }
        }
        catch (SQLiteException ex) when (ex.Message.Contains("UNIQUE constraint faile"))
        {
            if (ex.Message.Contains("Socios.dni"))
            {
                throw new InvalidOperationException("Ya existe un socio con ese DNI.", ex);
            }
            if (ex.Message.Contains("Socios.email"))
            {
                throw new InvalidOperationException("Ya existe un socio con ese email.", ex);
            }
            throw new InvalidOperationException("Ya existe un socio con esos datos.", ex);
        }
    }
    public bool Actualizar(Socio socio)
    {
        try
        {
            using (SQLiteConnection conexion = ConexionBd.ObtenerConexion())
            {
                string sql = "UPDATE Socios SET nombre = @nombre, apellido = @apellido, " +
                             "email = @email, telefono = @telefono WHERE id_socio = @id";

                using (SQLiteCommand comando = new SQLiteCommand(sql, conexion))
                {
                    comando.Parameters.AddWithValue("@nombre", socio.Nombre);
                    comando.Parameters.AddWithValue("@apellido", socio.Apellido);
                    comando.Parameters.AddWithValue("@email", (object?)socio.Email ?? DBNull.Value);// si es null,se guarda DBNull.Value
                    comando.Parameters.AddWithValue("@telefono", (object?)socio.Telefono ?? DBNull.Value);// si es null,se guarda DBNull.Value
                    comando.Parameters.AddWithValue("@id", socio.Id);
                    return comando.ExecuteNonQuery() > 0;
                }
            }
        }
        catch (SQLiteException ex) when (ex.Message.Contains("UNIQUE constraint failed"))
        {
            throw new InvalidOperationException("Ya existe otro socio con ese email.", ex);
        }
    }
    public bool DarDeBaja(int id)
    {
        return CambiarEstado(id, "Inactivo");
    }
    public bool Reactivar(int id)
    {
        return CambiarEstado(id, "Activo");
    }

    // estos son los auxiliares que reciben la condicion SQL y el valor del parametro
    private Socio? ObtenerUno(string condicion, object valor)//obtiene un socio segun la condicion sql, devuelve null si no encuentra
    {
        List<Socio> lista = ObtenerLista("SELECT " + Columnas + " FROM Socios " + condicion, valor);
        return lista.Count > 0 ? lista[0] : null;
    }
    private bool CambiarEstado(int id, string estado)
    {
        using (SQLiteConnection conexion = ConexionBd.ObtenerConexion())
        {
            using (SQLiteCommand comando = new SQLiteCommand(
                "UPDATE Socios SET estado = @estado WHERE id_socio = @id", conexion))
            {
                comando.Parameters.AddWithValue("@estado", estado);
                comando.Parameters.AddWithValue("@id", id);
                return comando.ExecuteNonQuery() > 0;
            }
        }
    }
    private List<Socio> ObtenerLista(string sql, object? valor)//recibe una consulta y un valor, devuelve una lista de socios(vacia si no encuentra)
    {

        List<Socio> lista = new List<Socio>();

        using (SQLiteConnection conexion = ConexionBd.ObtenerConexion())
        {
            using (SQLiteCommand comando = new SQLiteCommand(sql, conexion))
            {
                if (valor != null)
                {
                    comando.Parameters.AddWithValue("@valor", valor);
                }

                using (SQLiteDataReader lector = comando.ExecuteReader())
                {
                    while (lector.Read())
                    {
                        lista.Add(Mapear(lector));
                    }
                }
            }
        }
        return lista;
    }
    private Socio Mapear(SQLiteDataReader lector)//convierte la fila en un objeto socio,usando el su constructor y validaciones
    {
        return new Socio(
            dni: lector.GetString(1),
            nombre: lector.GetString(2),
            apellido: lector.GetString(3),
            email: lector.IsDBNull(4) ? null : lector.GetString(4),
            telefono: lector.IsDBNull(5) ? null : lector.GetString(5),
            fechaRegistro: LeerFecha(lector.GetString(7)),
            activo: lector.GetString(6) == "Activo",
            id: lector.GetInt32(0));
    }//GetString() hace que si el valor es null, lanza una excepción, por eso se usa IsDBNull() para verificar antes de leer

    private DateTime LeerFecha(string texto)//trata de convertir el texto en una fecha, sino puede devuelve la fecha actual
    {
        DateTime fecha;
        if (DateTime.TryParse(texto, CultureInfo.InvariantCulture, DateTimeStyles.None, out fecha))
        {
            return fecha;
        }
        return DateTime.Now;
    }

}