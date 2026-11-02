using System;
using System.IO;
using System.Data.SQLite;

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
    public static SQLiteConnection ObtenerConexion()
    {
        string ruta = RutaArchivo();

        // Si el archivo no existe, se crea vacío (después InicializadorBd arma las tablas)
        if (!File.Exists(ruta))
        {
            SQLiteConnection.CreateFile(ruta);
        }

        SQLiteConnectionStringBuilder constructor = new SQLiteConnectionStringBuilder();
        constructor.DataSource = ruta;
        constructor.ForeignKeys = true;

        SQLiteConnection conexion = new SQLiteConnection(constructor.ToString());
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
            using (SQLiteConnection conexion = ObtenerConexion())
            {
                mensaje = "Conexión correcta.";
                return true;
            }
        }
        catch (SQLiteException ex)
        {
            mensaje = "No se pudo conectar a la base de datos: " + ex.Message;
            return false;
        }
    }
}
