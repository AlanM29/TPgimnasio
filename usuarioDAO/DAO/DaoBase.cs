using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Globalization;

//  DAO: DaoBase
//  Clase base de todos los DAO (SocioDAO, UsuarioDAO, PagoDAO).

public abstract class DaoBase
{
    // Las fechas se guardan como texto
    protected const string FormatoFecha = "yyyy-MM-dd HH:mm:ss";

    protected static string FechaATexto(DateTime fecha)
    {
        return fecha.ToString(FormatoFecha, CultureInfo.InvariantCulture);
    }

    // Si el texto no se puede convertir, devuelve la fecha actual
    protected static DateTime TextoAFecha(string texto)
    {
        DateTime fecha;
        if (DateTime.TryParse(texto, CultureInfo.InvariantCulture, DateTimeStyles.None, out fecha))
        {
            return fecha;
        }
        return DateTime.Now;
    }

    // Carga los parámetros en un comando
    protected static void AgregarParametros(SQLiteCommand comando, params (string Nombre, object? Valor)[] parametros)
    {
        foreach ((string Nombre, object? Valor) parametro in parametros)
        {
            comando.Parameters.AddWithValue(parametro.Nombre, parametro.Valor ?? DBNull.Value);
        }
    }

    // INSERT / UPDATE / DELETE. Devuelve la cantidad de filas afectadas.
    protected int EjecutarNoQuery(string sql, params (string Nombre, object? Valor)[] parametros)
    {
        using (SQLiteConnection conexion = ConexionBd.ObtenerConexion())
        {
            using (SQLiteCommand comando = new SQLiteCommand(sql, conexion))
            {
                AgregarParametros(comando, parametros);
                return comando.ExecuteNonQuery();
            }
        }
    }

    // INSERT que devuelve el id que generó la base
    protected long EjecutarInsert(string sql, params (string Nombre, object? Valor)[] parametros)
    {
        using (SQLiteConnection conexion = ConexionBd.ObtenerConexion())
        {
            using (SQLiteCommand comando = new SQLiteCommand(sql, conexion))
            {
                AgregarParametros(comando, parametros);
                comando.ExecuteNonQuery();
            }
            return conexion.LastInsertRowId;
        }
    }

    // Consulta que devuelve varias filas. "mapear" convierte cada fila en un objeto.
    protected List<T> ConsultarLista<T>(string sql, Func<SQLiteDataReader, T> mapear,params (string Nombre, object? Valor)[] parametros)
    {
        List<T> lista = new List<T>();

        using (SQLiteConnection conexion = ConexionBd.ObtenerConexion())
        {
            using (SQLiteCommand comando = new SQLiteCommand(sql, conexion))
            {
                AgregarParametros(comando, parametros);

                using (SQLiteDataReader lector = comando.ExecuteReader())
                {
                    while (lector.Read())
                    {
                        lista.Add(mapear(lector));
                    }
                }
            }
        }
        return lista;
    }

    // Consulta que devuelve a lo sumo una fila (o null si no hay)
    protected T? ConsultarUno<T>(string sql, Func<SQLiteDataReader, T> mapear,params (string Nombre, object? Valor)[] parametros) where T : class
    {
        List<T> lista = ConsultarLista<T>(sql, mapear, parametros);
        if (lista.Count > 0)
        {
            return lista[0];
        }
        return null;
    }

    // true si la consulta devuelve al menos una fila 
    protected bool Existe(string sql, params (string Nombre, object? Valor)[] parametros)
    {
        using (SQLiteConnection conexion = ConexionBd.ObtenerConexion())
        {
            using (SQLiteCommand comando = new SQLiteCommand(sql, conexion))
            {
                AgregarParametros(comando, parametros);
                return comando.ExecuteScalar() != null;
            }
        }
    }

    // true si el error de SQLite es por un dato repetido (columna UNIQUE)
    protected static bool EsDuplicado(SQLiteException ex)
    {
        return ex.Message.Contains("UNIQUE constraint failed");
    }
}
