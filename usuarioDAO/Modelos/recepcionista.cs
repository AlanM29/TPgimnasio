public class Recepcionista: UsuarioBase
{
    public Recepcionista(string nombreUsuario, string contrasenaHash, string nombre, string apellido,string dni,
        string? email, string? telefono, DateTime fechaRegistro
        , bool activo = true, int id = 0) :
        base(nombreUsuario, contrasenaHash, dni ,nombre, apellido, email, telefono, fechaRegistro, activo, id)
    { }
    public override string Rol => "Recepcionista";
    public override bool PuedeDarDeBajaSocios => false;
    public override bool PuedeExportar => false;
    public override bool PuedeModificarPrecios => false;
    public override bool PuedeGestionarUsuarios => false;
    public override bool PuedeAnularPagos => false;
}