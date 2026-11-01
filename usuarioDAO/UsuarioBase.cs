using System.Text.RegularExpressions;

public abstract class UsuarioBase
{
    public int Id { get; private set; }
    public string Nombre_Usuario { get; private set; }
    public string Contrasenahash { get; private set; }
    public string Nombre { get; private set; }
    public string Apellido { get; private set; }
    public string? Email { get; private set; }
    public string? Telefono { get; private set; }
    public DateTime Fecha_Creacion { get; private set; }
    public bool Activo { get; private set; }

    //cada subclase define sus permisos y su rol 
    public abstract string Rol { get; }
    public abstract bool PuedeDarDeBajaSocios { get; }
    public abstract bool PuedeModificarPrecios { get; }
    public abstract bool PuedeGestionarUsuarios { get; }
    public abstract bool PuedeExportar { get; }
    
    //Constructor
    public UsuarioBase(string nombreUsuario, string contrasenaHash,string nombre, string apellido,
        string? email, string? telefono, DateTime fecha_creacion, bool activo = true,int id = 0) 
    {
        Nombre_Usuario = ValidarNombreUsuario(nombreUsuario);
        Contrasenahash = ValidarObligatorio(contrasenaHash, "La contrasena");
        Nombre = ValidarObligatorio(nombre, "El nombre");
        Apellido = ValidarObligatorio(apellido, "El apellido");
        Email = ValidarEmail(email);
        Telefono = ValidarTelefono(telefono);
        Id = id;
        Activo = activo;
    }
    public void AsignarId(int id) => Id = id;
    public void CambiarContrasenahash(string contrasenianueva) => Contrasenahash = ValidarObligatorio(contrasenianueva, "La contrasena");
    public void ActualizarContacto(string nombre, string apellido, string? email, string? telefono)
    {
        Nombre = ValidarObligatorio(nombre, "El nombre");
        Apellido = ValidarObligatorio(apellido, "El apellido");
        Email = ValidarEmail(email);
        Telefono = ValidarTelefono(telefono);
    }
    //Aca se cambia su estado
    public void DarDeBaja() => Activo = false;
    public void Reactivar() => Activo = true;
    //Ahora van todas las validaciones 
    private static string ValidarObligatorio(string? valor, string campo) {
        if (string.IsNullOrWhiteSpace(valor))
        {
            throw new ArgumentException($"{campo} no puede estar vacio");
        }
        return valor.Trim();
    }
    private static string ValidarNombreUsuario(string valor)
    {
        string nombre = ValidarObligatorio(valor, "El nombre de usuario");
        if(nombre.Length < 4)
        {
            throw new ArgumentException("El nombre de usuario debe tener al menos 4 caracteres");
        }
        if(nombre.Contains(' '))
        {
            throw new ArgumentException("El nombre de usuario no puede contener espacios");
        }
        return nombre;
    }
    private static string ValidarEmail(string? valor)
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
    private static string ValidarTelefono(string? valor)
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
}
