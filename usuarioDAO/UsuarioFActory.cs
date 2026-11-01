public static class UsuarioFactory
{
    public static UsuarioBase Crear(string rol, string nombreUsuario, string contrasenaHash,
                                string nombre,string apellido, string? email, string? telefono,
                                DateTime fecha_creacion, bool activo = true, int id = 0)
    {
        return rol switch
        {
            "Admin" => new Admin(nombreUsuario, contrasenaHash, nombre,apellido,
                                         email, telefono, fecha_creacion, activo, id),
            "Recepcionista" => new Recepcionista(nombreUsuario, contrasenaHash, nombre,apellido,
                                                 email, telefono, fecha_creacion,activo, id),
            _ => throw new ArgumentException($"Rol desconocido: '{rol}'.")
        };
    }
}