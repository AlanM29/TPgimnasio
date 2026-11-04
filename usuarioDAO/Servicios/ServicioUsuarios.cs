using System;
using System.Collections.Generic;

//  ServicioUsuarios
public class ServicioUsuarios
{
    private const int LargoMinimoContrasena = 6;

    private UsuarioBase _actual;
    private IUsuarioDAO _usuarioDao;

    // Constructor 
    public ServicioUsuarios(UsuarioBase actual) : this(actual, new UsuarioDAO())
    {
    }

    // Constructor que recibe cualquier DAO
    public ServicioUsuarios(UsuarioBase actual, IUsuarioDAO usuarioDao)
    {
        _actual = actual;
        _usuarioDao = usuarioDao;
    }

    // --- CONSULTA ---

    public List<UsuarioBase> Listar(bool soloActivos)
    {
        ExigirPermiso();
        return _usuarioDao.ObtenerTodos(soloActivos);
    }

    public UsuarioBase ObtenerPorId(int id)
    {
        ExigirPermiso();
        return BuscarOFallar(id);
    }

    // --- ALTA ---

    // Recibe la contraseña en texto plano y guarda solo su hash
    public UsuarioBase Crear(string rol, string nombreUsuario, string contrasena, string nombre,string apellido, string dni, string? email, string? telefono)
    {
        ExigirPermiso();
        ValidarContrasena(contrasena);

        // La factory y los constructores validan los datos
        UsuarioBase nuevo = UsuarioFactory.Crear(rol, nombreUsuario, Seguridad.CalcularHash(contrasena),nombre, apellido, dni, email, telefono, DateTime.Now);
        _usuarioDao.Insertar(nuevo);

        LogService.Registrar(_actual.Nombre_Usuario, "USUARIO_CREADO",
                             nuevo.Nombre_Usuario + " (rol " + nuevo.Rol + ")");
        return nuevo;
    }

    // --- MODIFICACIÓN ---

    public void ModificarDatos(int id, string nombre, string apellido, string? email, string? telefono)
    {
        ExigirPermiso();
        UsuarioBase usuario = BuscarOFallar(id);

        usuario.ActualizarContacto(nombre, apellido, email, telefono);
        _usuarioDao.Actualizar(usuario);

        LogService.Registrar(_actual.Nombre_Usuario, "USUARIO_MODIFICADO", usuario.Nombre_Usuario);
    }

    public void CambiarContrasena(int id, string contrasenaNueva)
    {
        ExigirPermiso();
        ValidarContrasena(contrasenaNueva);
        UsuarioBase usuario = BuscarOFallar(id);

        usuario.CambiarContrasenahash(Seguridad.CalcularHash(contrasenaNueva));
        _usuarioDao.Actualizar(usuario);

        LogService.Registrar(_actual.Nombre_Usuario, "USUARIO_CLAVE_CAMBIADA", usuario.Nombre_Usuario);
    }

    // --- BAJA ---

    public void DarDeBaja(int id)
    {
        ExigirPermiso();

        // protección para que el sistema no se quede sin acceso de administración.
        if (id == _actual.Id)
        {
            throw new InvalidOperationException("No podés dar de baja tu propio usuario.");
        }

        UsuarioBase usuario = BuscarOFallar(id);
        if (!usuario.Activo)
        {
            throw new InvalidOperationException("El usuario '" + usuario.Nombre_Usuario + "' ya está dado de baja.");
        }

        _usuarioDao.DarDeBaja(id);
        LogService.Registrar(_actual.Nombre_Usuario, "USUARIO_BAJA", usuario.Nombre_Usuario);
    }

    public void Reactivar(int id)
    {
        ExigirPermiso();

        UsuarioBase usuario = BuscarOFallar(id);
        if (usuario.Activo)
        {
            throw new InvalidOperationException("El usuario '" + usuario.Nombre_Usuario + "' ya está activo.");
        }

        _usuarioDao.Reactivar(id);
        LogService.Registrar(_actual.Nombre_Usuario, "USUARIO_REACTIVADO", usuario.Nombre_Usuario);
    }

    // --- AYUDAS ---

    private void ExigirPermiso()
    {
        if (!_actual.PuedeGestionarUsuarios)
        {
            LogService.Registrar(_actual.Nombre_Usuario, "ACCESO_DENEGADO", "Gestión de usuarios");
            throw new UnauthorizedAccessException("No tenés permiso para gestionar usuarios.");
        }
    }

    private UsuarioBase BuscarOFallar(int id)
    {
        UsuarioBase? usuario = _usuarioDao.ObtenerPorId(id);
        if (usuario == null)
        {
            throw new InvalidOperationException("No existe un usuario con id " + id + ".");
        }
        return usuario;
    }

    private static void ValidarContrasena(string contrasena)
    {
        if (string.IsNullOrWhiteSpace(contrasena) || contrasena.Length < LargoMinimoContrasena)
        {
            throw new ArgumentException("La contraseña debe tener al menos " + LargoMinimoContrasena + " caracteres.");
        }
    }
}
