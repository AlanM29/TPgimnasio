using System;
using System.Collections.Generic;
using System.IO;

//  LogService
public static class LogService
{
    private const string NombreArchivo = "log.txt";

    private static string Ruta()
    {
        return Path.Combine(AppContext.BaseDirectory, NombreArchivo);
    }

    // Agrega una línea al final del archivo. Si falla, avisa 
    public static void Registrar(string usuario, string accion, string detalle)
    {
        try
        {
            string linea = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") +
                           " | " + usuario + " | " + accion + " | " + detalle;

            File.AppendAllText(Ruta(), linea + Environment.NewLine);
        }
        catch (IOException ex)
        {
            Console.WriteLine("Aviso: no se pudo escribir en el registro (" + ex.Message + ")");
        }
        catch (UnauthorizedAccessException ex)
        {
            Console.WriteLine("Aviso: sin permiso para escribir el registro (" + ex.Message + ")");
        }
    }

    // Devuelve las últimas "cantidad" líneas del registro
    public static List<string> ObtenerUltimos(int cantidad)
    {
        List<string> resultado = new List<string>();

        try
        {
            if (!File.Exists(Ruta()))
            {
                return resultado;
            }

            string[] lineas = File.ReadAllLines(Ruta());
            int inicio = lineas.Length - cantidad;
            if (inicio < 0)
            {
                inicio = 0;
            }

            for (int i = inicio; i < lineas.Length; i++)
            {
                resultado.Add(lineas[i]);
            }
        }
        catch (IOException ex)
        {
            Console.WriteLine("Aviso: no se pudo leer el registro (" + ex.Message + ")");
        }

        return resultado;
    }
}
