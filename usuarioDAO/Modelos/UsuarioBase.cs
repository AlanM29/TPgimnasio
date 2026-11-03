using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

public abstract class UsuarioBase: Persona
{
    public string Nombre_Usuario { get; private set; }
    public string Contrasenahash { get; private set; }

    //cada subclase define sus permisos y su rol 
    public abstract string Rol { get; }
    public abstract bool PuedeDarDeBajaSocios { get; }
    public abstract bool PuedeModificarPrecios { get; }
    public abstract bool PuedeGestionarUsuarios { get; }
    public abstract bool PuedeExportar { get; }
    
    //Constructor
    public UsuarioBase(string nombreUsuario, string contrasenaHash, string dni,string nombre, string apellido,
        string? email, string? telefono, DateTime fechaRegistro, bool activo = true,int id = 0):
        base(dni, nombre, apellido,email, telefono, fechaRegistro, activo, id)
    { 
    
       Nombre_Usuario = ValidarNombreUsuario(nombreUsuario);
        Contrasenahash = Validaciones.ValidarObligatorio(contrasenaHash, "La contraseña");
    }
     public void CambiarContrasenahash(string contrasenianueva)
         => Contrasenahash = Validaciones.ValidarObligatorio(contrasenianueva, "La contrasena");


private static string ValidarNombreUsuario(string valor)
    {
        string nombre = Validaciones.ValidarObligatorio(valor, "El nombre de usuario");
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
   
}
