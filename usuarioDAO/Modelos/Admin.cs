public class Admin : UsuarioBase
{
    public Admin(string nombreUsuario, string contrasenaHash, string nombre, string apellido, string dni,
        string? email, string? telefono, DateTime fechaRegistro, bool activo = true, int id = 0) :
        base(nombreUsuario, contrasenaHash, dni ,nombre, apellido, email, telefono,
               fechaRegistro, activo, id)
    { }
    public override string Rol => "Admin";
    public override bool PuedeDarDeBajaSocios => true;
    public override bool PuedeModificarPrecios => true;
    public override bool PuedeGestionarUsuarios => true;
    public override bool PuedeExportar => true;
}