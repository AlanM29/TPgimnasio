using System;
using System.Data.SQLite;
using System.Globalization;
using System.IO;

//  INTERFAZ: EntradaConsola
//  Funciones comunes para leer datos por consola y mostrar errores.
//  Las usan MenuUsuarios y MenuPagos (y MenuLogin para la contraseña).

public static class EntradaConsola
{
    // Pide un número entero entre minimo y maximo (repite hasta que sea válido)
    public static int LeerEntero(string mensaje, int minimo, int maximo)
    {
        int numero;
        Console.Write(mensaje);
        while (!int.TryParse(Console.ReadLine(), out numero) || numero < minimo || numero > maximo)
        {
            Console.Write("Ingresá un número entre " + minimo + " y " + maximo + ": ");
        }
        return numero;
    }

    // Pide un monto. Acepta 25000, 25000.50 y 25000,50
    public static decimal LeerMonto(string mensaje)
    {
        decimal monto;
        Console.Write(mensaje);
        while (!decimal.TryParse((Console.ReadLine() ?? "").Trim().Replace(',', '.'),
                                 NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out monto))
        {
            Console.Write("Ingresá un monto válido (ej: 25000 o 25000,50): ");
        }
        return monto;
    }

    // Pide un texto que no puede quedar vacío
    public static string LeerTextoObligatorio(string mensaje)
    {
        Console.Write(mensaje);
        string texto = (Console.ReadLine() ?? "").Trim();
        while (texto.Length == 0)
        {
            Console.Write("No puede quedar vacío. " + mensaje);
            texto = (Console.ReadLine() ?? "").Trim();
        }
        return texto;
    }

    // Pide un texto opcional: si se deja vacío devuelve null
    public static string? LeerTextoOpcional(string mensaje)
    {
        Console.Write(mensaje);
        string texto = (Console.ReadLine() ?? "").Trim();
        return texto.Length == 0 ? null : texto;
    }

    // Muestra el valor actual; si se aprieta Enter sin escribir nada, se conserva
    public static string? LeerConValorActual(string campo, string? actual)
    {
        Console.Write(campo + " [" + (actual ?? "sin dato") + "] (Enter para no cambiar): ");
        string texto = (Console.ReadLine() ?? "").Trim();
        return texto.Length == 0 ? actual : texto;
    }

    // Pide una fecha con formato dd/MM/yyyy
    public static DateTime LeerFecha(string mensaje)
    {
        string[] formatos = { "dd/MM/yyyy", "d/M/yyyy" };
        DateTime fecha;
        Console.Write(mensaje);
        while (!DateTime.TryParseExact((Console.ReadLine() ?? "").Trim(), formatos,
                                       CultureInfo.InvariantCulture, DateTimeStyles.None, out fecha))
        {
            Console.Write("Fecha inválida. Usá el formato dd/MM/aaaa: ");
        }
        return fecha;
    }

    // Pregunta s/n
    public static bool Confirmar(string mensaje)
    {
        Console.Write(mensaje + " (s/n): ");
        string respuesta = (Console.ReadLine() ?? "").Trim().ToLower();
        while (respuesta != "s" && respuesta != "n")
        {
            Console.Write("Respondé s o n: ");
            respuesta = (Console.ReadLine() ?? "").Trim().ToLower();
        }
        return respuesta == "s";
    }

    // Lee la contraseña mostrando asteriscos
    public static string LeerContrasenaOculta()
    {
        string clave = "";
        ConsoleKeyInfo tecla;

        do
        {
            tecla = Console.ReadKey(true);

            if (tecla.Key == ConsoleKey.Backspace)
            {
                if (clave.Length > 0)
                {
                    clave = clave.Substring(0, clave.Length - 1);
                    Console.Write("\b \b");
                }
            }
            else if (tecla.Key != ConsoleKey.Enter && !char.IsControl(tecla.KeyChar))
            {
                clave += tecla.KeyChar;
                Console.Write("*");
            }
        } while (tecla.Key != ConsoleKey.Enter);

        Console.WriteLine();
        return clave;
    }

    // Ejecuta una acción y, si falla, muestra un mensaje claro en vez de cerrar el programa.
    // Es el único lugar donde se imprimen los errores de los servicios y los DAO.
    public static void Intentar(Action accion)
    {
        try
        {
            accion();
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine("Dato inválido: " + ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            Console.WriteLine("No se pudo completar: " + ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            Console.WriteLine(ex.Message);
        }
        catch (SQLiteException ex)
        {
            Console.WriteLine("Error de base de datos: " + ex.Message);
        }
        catch (IOException ex)
        {
            Console.WriteLine("Error de archivo: " + ex.Message);
        }
    }
}
