public static class UsuarioFactory
{
    public static UsuarioBase Crear(string rol, string nombreUsuario, string contrasenaHash,
                                string nombre,string apellido,string dni ,string? email, string? telefono,
                                DateTime fechaRegistro, bool activo = true, int id = 0)
    {
        return rol switch
        {
            "Admin" => new Admin(nombreUsuario, contrasenaHash, nombre,apellido,dni,
                                         email, telefono, fechaRegistro, activo, id),
            "Recepcionista" => new Recepcionista(nombreUsuario, contrasenaHash, nombre,apellido,dni,
                                                 email, telefono, fechaRegistro,activo, id),
            _ => throw new ArgumentException($"Rol desconocido: '{rol}'.")
        };
    }
}