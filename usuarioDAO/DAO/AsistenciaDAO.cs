using System.Data.SQLite;
using System.Globalization;

//  DAO: AsistenciaDao
//  Única clase que consulta la tabla Asistencias. No imprime nada.

public class AsistenciaDAO : IAsistenciaDAO
{
    private const string FormatoFecha = "yyyy-MM-dd HH:mm:ss";
    private const string Columnas = "id_asis, id_socio, tiempo";

    public int Insertar(Asistencia asistencia)
    {
        try
        {
            using (SQLiteConnection conexion = ConexionBd.ObtenerConexion())
            {
                string sql = "INSERT INTO Asistencias (id_socio, tiempo) VALUES (@idsocio, @tiempo)";

                using (SQLiteCommand comando = new SQLiteCommand(sql, conexion))
                {
                    comando.Parameters.AddWithValue("@idsocio", asistencia.Id_Socio);//agrega id_socio al comando sql
                    comando.Parameters.AddWithValue("@tiempo",asistencia.FechaHora.ToString(FormatoFecha, CultureInfo.InvariantCulture));
                    comando.ExecuteNonQuery();
                }

                int id = (int)conexion.LastInsertRowId;
                asistencia.AsignarId(id);
                return id;
            }
        }
        catch (SQLiteException ex) when (ex.Message.Contains("FOREIGN KEY constraint failed"))//so no existe el id lanza una excepcion
        {
            throw new InvalidOperationException("El socio indicado no existe.", ex);
        }
    }

    public List<Asistencia> ObtenerPorSocio(int idSocio)
    {
        return ObtenerLista("SELECT " + Columnas + " FROM Asistencias " +
                            "WHERE id_socio = @valor ORDER BY tiempo DESC", idSocio);
    }

    public List<Asistencia> ObtenerPorFecha(DateTime fecha)
    {
        // El texto empieza con la fecha, así que LIKE 'yyyy-MM-dd%' trae todo ese día
        string dia = fecha.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        return ObtenerLista("SELECT " + Columnas + " FROM Asistencias " +
                            "WHERE tiempo LIKE @valor ORDER BY tiempo", dia + "%");
    }

    public bool ExisteAsistenciaEnFecha(int idSocio, DateTime fecha)//devuelve true si ya existe una asistencia para ese socio en esa fecha
    {
        string dia = fecha.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

        using (SQLiteConnection conexion = ConexionBd.ObtenerConexion())
        {
            string sql = "SELECT COUNT(*) FROM Asistencias WHERE id_socio = @idsocio AND tiempo LIKE @dia";
            using (SQLiteCommand comando = new SQLiteCommand(sql, conexion))
            {
                comando.Parameters.AddWithValue("@idsocio", idSocio);
                comando.Parameters.AddWithValue("@dia", dia + "%");
                return Convert.ToInt32(comando.ExecuteScalar()) > 0;
            }
        }
    }

    // ---------- Auxiliares ----------

    private List<Asistencia> ObtenerLista(string sql, object valor)
    {
        List<Asistencia> lista = new List<Asistencia>();

        using (SQLiteConnection conexion = ConexionBd.ObtenerConexion())
        {
            using (SQLiteCommand comando = new SQLiteCommand(sql, conexion))
            {
                comando.Parameters.AddWithValue("@valor", valor);

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

    private Asistencia Mapear(SQLiteDataReader lector)//mapea los datos de la base de datos a un objeto Asistencia
    {
        DateTime fecha;
        if (!DateTime.TryParse(lector.GetString(2), CultureInfo.InvariantCulture, DateTimeStyles.None, out fecha))
        {
            fecha = DateTime.Now;
        }
        return new Asistencia(lector.GetInt32(1), fecha, lector.GetInt32(0) );//pasa los datos al constructor
    }
}