//  LoginService
public class LoginService
{
    private IUsuarioDAO _usuarioDao;

    // Constructor normal: usa el DAO real (SQLite)
    public LoginService() : this(new UsuarioDAO())
    {
    }

    // Constructor que recibe cualquier DAO 
    public LoginService(IUsuarioDAO usuarioDao)
    {
        _usuarioDao = usuarioDao;
    }

    // Devuelve el usuario Admin o Recepcionista si los datos son correctos, o null si no
    public UsuarioBase? Ingresar(string nombreUsuario, string contrasena)
    {
        string hash = Seguridad.CalcularHash(contrasena);
        return _usuarioDao.ObtenerPorCredenciales(nombreUsuario, hash);
    }
}
