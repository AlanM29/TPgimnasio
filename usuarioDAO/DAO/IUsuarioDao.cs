public interface IUsuarioDao
{
    // Devuelve el usuario Admin o Recepcionista si el nombre y el hash coinciden, o null si no
    UsuarioBase? ObtenerPorCredenciales(string nombreUsuario, string contrasenaHash);
}
