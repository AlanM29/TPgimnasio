using System;
using System.IO;
using Microsoft.Data.Sqlite;

//  DAO: ConexionBd
//  Su único trabajo es ABRIR la conexión a la base de datos.

public static class ConexionBd
{
    private const string NombreArchivo = "gimnasio.sqlite3";

    // El archivo se busca junto al .exe (carpeta bin/Debug/...)
    public static string RutaArchivo()
    {
        return Path.Combine(AppContext.BaseDirectory, NombreArchivo);
    }

    // Abre y devuelve una conexión. Quien la pide la cierra con "using".
    public static SqliteConnection ObtenerConexion()
    {
        SqliteConnectionStringBuilder constructor = new SqliteConnectionStringBuilder();
        constructor.DataSource = RutaArchivo();

        // ReadWriteCreate: si el archivo no existe, lo crea (después InicializadorBd le arma las tablas)
        constructor.Mode = SqliteOpenMode.ReadWriteCreate;

        // Hace que SQLite respete las claves foráneas (por ejemplo ON DELETE RESTRICT)
        constructor.ForeignKeys = true;

        SqliteConnection conexion = new SqliteConnection(constructor.ToString());
        conexion.Open();
        return conexion;
    }

    public static bool ExisteBaseDeDatos()
    {
        return File.Exists(RutaArchivo());
    }

    // Prueba que se pueda abrir la base. Devuelve true/false y explica qué pasó en "mensaje".
    public static bool ProbarConexion(out string mensaje)
    {
        try
        {
            using (SqliteConnection conexion = ObtenerConexion())
            {
                mensaje = "Conexión correcta.";
                return true;
            }
        }
        catch (SqliteException ex)
        {
            mensaje = "No se pudo conectar a la base de datos: " + ex.Message;
            return false;
        }
    }
}
