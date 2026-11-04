public interface IUsuarioDAO
{
    // Devuelve el usuario Admin o Recepcionista si el nombre y el hash coinciden, o null si no. Si falla la base de datos o un dato guardado es inválido.
    UsuarioBase? ObtenerPorCredenciales(string nombreUsuario, string contrasenaHash);

    // ---- Consulta ----
    UsuarioBase? ObtenerPorId(int id);
    UsuarioBase? ObtenerPorNombreUsuario(string nombreUsuario);
    List<UsuarioBase> ObtenerTodos(bool soloActivos);

    // ---- Alta ----
    // Crea el empleado y el usuario. Devuelve el id generado . Lanza InvalidOperationException si el nombre de usuario, el DNI o el email ya existen.
    int Insertar(UsuarioBase usuario);

    // ---- Modificación ----
    // Guarda nombre, apellido, email, teléfono y contraseña (hash).
    bool Actualizar(UsuarioBase usuario);

    // ---- Baja ----
    bool DarDeBaja(int id);
    bool Reactivar(int id);
}
