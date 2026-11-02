public class Recepcionista: UsuarioBase
{
    public Recepcionista(string nombreUsuario, string contrasenaHash, string nombre, string apellido,
        string? email, string? telefono, DateTime fecha_creacion, bool activo = true, int id = 0) :
        base(nombreUsuario, contrasenaHash, nombre, apellido, email, telefono, fecha_creacion, activo, id)
    { }
    public override string Rol => "Recepcionista";
    public override bool PuedeDarDeBajaSocios => false;
    public override bool PuedeExportar => false;
    public override bool PuedeModificarPrecios => false;
    public override bool PuedeGestionarUsuarios => false;
}