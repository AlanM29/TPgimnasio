using System;
using System.Globalization;
using System.IO;


//  SERVICIO: LicenciaService
//  Formato del código:  GYM-<PLAN>-<FIRMA>

public class LicenciaService
{
    private const string NombreArchivo = "licencia.txt";
    private const string Sal = "|GIMNASIO"; // dato "secreto" que se suma al plan para firmar

    // Lee el archivo y devuelve la Licencia (válida o no). Nunca tira excepción.
    public Licencia Cargar()
    {
        string ruta = Path.Combine(AppContext.BaseDirectory, NombreArchivo);

        try
        {
            if (!File.Exists(ruta))
            {
                return Invalida("No se encontró " + NombreArchivo + " en: " + AppContext.BaseDirectory);
            }

            string codigo = "";
            string textoVencimiento = "";

            foreach (string linea in File.ReadAllLines(ruta))
            {
                string limpia = linea.Trim();

                if (limpia.StartsWith("CODIGO="))
                {
                    codigo = limpia.Substring("CODIGO=".Length).Trim();
                }
                else if (limpia.StartsWith("VENCIMIENTO="))
                {
                    textoVencimiento = limpia.Substring("VENCIMIENTO=".Length).Trim();
                }
            }

            return Validar(codigo, textoVencimiento);
        }
        catch (IOException ex)
        {
            return Invalida("No se pudo leer el archivo de licencia: " + ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Invalida("Sin permiso para leer el archivo de licencia: " + ex.Message);
        }
    }

    public Licencia Validar(string codigo, string textoVencimiento)
    {
        if (codigo == "")
        {
            return Invalida("El archivo no contiene una línea CODIGO=...");
        }

        string[] partes = codigo.Split('-');
        if (partes.Length != 3 || partes[0].ToUpper() != "GYM")
        {
            return Invalida("El código tiene un formato incorrecto.");
        }

        string plan = partes[1].ToUpper();
        string firma = partes[2].ToUpper();

        if (plan != "COMPLETA" && plan != "BASICA")
        {
            return Invalida("El plan indicado en el código no existe.");
        }

        // Cheoqueo de codigo para saber si fue modificado o no
        if (firma != CalcularFirma(plan))
        {
            return Invalida("El código no coincide: fue modificado o no es válido.");
        }

        string vencimientoMostrar = "sin vencimiento";

        if (textoVencimiento != "")
        {
            DateTime fecha;
            if (!DateTime.TryParseExact(textoVencimiento, "yyyy-MM-dd",
                                        CultureInfo.InvariantCulture, DateTimeStyles.None, out fecha))
            {
                return Invalida("La fecha de vencimiento debe tener el formato aaaa-mm-dd.");
            }

            if (fecha.Date < DateTime.Today)
            {
                return Invalida("La licencia venció el " + textoVencimiento + ".");
            }

            vencimientoMostrar = textoVencimiento;
        }

        return new Licencia(plan, true, "Licencia válida.", vencimientoMostrar);
    }

    private Licencia Invalida(string motivo)
    {
        return new Licencia("NINGUNA", false, motivo, "");
    }

    // La firma son los primeros 8 caracteres del hash de (plan + sal)
    private static string CalcularFirma(string plan)
    {
        return Seguridad.CalcularHash(plan + Sal).Substring(0, 8).ToUpper();
    }

    // Sirve para generar códigos nuevos (por ejemplo, para probar)
    public static string GenerarCodigo(string plan)
    {
        plan = plan.ToUpper();
        return "GYM-" + plan + "-" + CalcularFirma(plan);
    }
}
