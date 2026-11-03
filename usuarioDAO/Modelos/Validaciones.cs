using System.Text.RegularExpressions;
public static class Validaciones
{
    public static string ValidarObligatorio(string? valor, string campo)
    {
        if (string.IsNullOrWhiteSpace(valor))
        {
            throw new ArgumentException($"{campo} no puede estar vacio");
        }
        return valor.Trim();
    }
    public static string? ValidarEmail(string? valor)
    {
        if (string.IsNullOrWhiteSpace(valor))
        {
            return null;//es opcional asi que puede ser nulo
        }
        string email = valor.Trim();
        if (!Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$")) //se usa para especificar el patron especifico
            throw new ArgumentException("El email tiene un formato invalido");
        return email;
    }
    public static string? ValidarTelefono(string? valor)
    {
        if (string.IsNullOrWhiteSpace(valor))
        {
            return null;//es opciona asi que puede ser nulo
        }
        string telefono = valor.Trim();
        if (!Regex.IsMatch(telefono, @"^[0-9+\- ]{6,20}$"))//se usa para especificar el patron especifico
            throw new ArgumentException("El teléfono solo admite números, +, - y espacios (6 a 20 caracteres).");
        return telefono;
    }
    public static string ValidarDni(string? valor)
    {
        string dni = ValidarObligatorio(valor, "El DNI");
        if (!Regex.IsMatch(dni, @"^\d{7,8}$"))
        {
            throw new ArgumentException("El DNI debe tener 7 u 8 números, sin puntos ni espacios.");
        }
        return dni;
    }
}