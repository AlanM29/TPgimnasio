using System;
using System.Security.Cryptography;
using System.Text;

//  Clase estática "caja de herramientas" para la seguridad.
//  Las contraseñas NUNCA se guardan en texto plano: se guarda su hash.
public static class Seguridad
{
    public static string CalcularHash(string texto)
    {
        byte[] bytes = SHA256.HashData(Encoding.UTF8.GetBytes(texto));
        return Convert.ToHexString(bytes).ToLower();
    }
}
