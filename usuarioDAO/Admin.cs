public class Admin : UsuarioBase
{
    public Admin(string nombreUsuario, string contrasenaHash, string nombre, string apellido,
        string? email, string? telefono, DateTime fecha_creacion, bool activo = true, int id = 0) :
        base(nombreUsuario, contrasenaHash, nombre, apellido, email, telefono,
               fecha_creacion, activo, id)
    { }
    public override string Rol => "Admin";
    public override bool PuedeDarDeBajaSocios => true;
    public override bool PuedeModificarPrecios => true;
    public override bool PuedeGestionarUsuarios => true;
    public override bool PuedeExportar => true;
}