using System;
using System.Collections.Generic;

// --- MenuUsuarios ---
public class MenuUsuarios
{
    private UsuarioBase _usuario;
    private ServicioUsuarios _servicio;

    public MenuUsuarios(UsuarioBase usuario) : this(usuario, new ServicioUsuarios(usuario))
    {
    }

    public MenuUsuarios(UsuarioBase usuario, ServicioUsuarios servicio)
    {
        _usuario = usuario;
        _servicio = servicio;
    }

    public void Mostrar()
    {
        // ---- Control de permiso ----
        if (!_usuario.PuedeGestionarUsuarios)
        {
            Console.WriteLine("Operación no autorizada.");
            LogService.Registrar(_usuario.Nombre_Usuario, "ACCESO_DENEGADO", "Menú de usuarios");
            return;
        }

        int opcion;
        do
        {
            Console.WriteLine("===== GESTIÓN DE USUARIOS =====");
            Console.WriteLine("1. Ver usuarios");
            Console.WriteLine("2. Crear usuario");
            Console.WriteLine("3. Modificar datos de un usuario");
            Console.WriteLine("4. Cambiar contraseña");
            Console.WriteLine("5. Dar de baja");
            Console.WriteLine("6. Reactivar Usuario");
            Console.WriteLine("0. Volver al Menu Principal");
            opcion = EntradaConsola.LeerEntero("Opción: ", 0, 6);
            Console.WriteLine();

            switch (opcion)
            {
                case 1:
                    EntradaConsola.Intentar(VerUsuarios);
                    break;
                case 2:
                    EntradaConsola.Intentar(Crear);
                    break;
                case 3:
                    EntradaConsola.Intentar(Modificar);
                    break;
                case 4:
                    EntradaConsola.Intentar(CambiarContrasena);
                    break;
                case 5:
                    EntradaConsola.Intentar(DarDeBaja);
                    break;
                case 6:
                    EntradaConsola.Intentar(Reactivar);
                    break;
            }

            Console.WriteLine();
        } while (opcion != 0);
    }

    private void VerUsuarios()
    {
        List<UsuarioBase> usuarios = _servicio.Listar(false);

        if (usuarios.Count == 0)
        {
            Console.WriteLine("No hay usuarios cargados.");
            return;
        }

        Console.WriteLine("--- USUARIOS ---");
        foreach (UsuarioBase usuario in usuarios)
        {
            Console.WriteLine(Describir(usuario));
        }
    }

    private void Crear()
    {
        Console.WriteLine("--- NUEVO USUARIO ---");
        Console.WriteLine("Rol: 1. Admin   2. Recepcionista");
        int rol = EntradaConsola.LeerEntero("Opción: ", 1, 2);

        string nombreUsuario = EntradaConsola.LeerTextoObligatorio("Nombre de usuario (mín. 4, sin espacios): ");
        Console.Write("Contraseña (mín. 6 caracteres): ");
        string contrasena = EntradaConsola.LeerContrasenaOculta();
        string nombre = EntradaConsola.LeerTextoObligatorio("Nombre: ");
        string apellido = EntradaConsola.LeerTextoObligatorio("Apellido: ");
        string dni = EntradaConsola.LeerTextoObligatorio("DNI (7 u 8 números): ");
        string? email = EntradaConsola.LeerTextoOpcional("Email (Enter para omitir): ");
        string? telefono = EntradaConsola.LeerTextoOpcional("Teléfono (Enter para omitir): ");

        UsuarioBase creado = _servicio.Crear(rol == 1 ? "Admin" : "Recepcionista", nombreUsuario, contrasena,
                                             nombre, apellido, dni, email, telefono);
        Console.WriteLine("Usuario creado: " + Describir(creado));
    }

    private void Modificar()
    {
        int id = EntradaConsola.LeerEntero("Id del usuario: ", 1, int.MaxValue);
        UsuarioBase usuario = _servicio.ObtenerPorId(id);
        Console.WriteLine(Describir(usuario));

        string nombre = EntradaConsola.LeerConValorActual("Nombre", usuario.Nombre) ?? usuario.Nombre;
        string apellido = EntradaConsola.LeerConValorActual("Apellido", usuario.Apellido) ?? usuario.Apellido;
        string? email = EntradaConsola.LeerConValorActual("Email", usuario.Email);
        string? telefono = EntradaConsola.LeerConValorActual("Teléfono", usuario.Telefono);

        _servicio.ModificarDatos(id, nombre, apellido, email, telefono);
        Console.WriteLine("Datos actualizados.");
    }

    private void CambiarContrasena()
    {
        int id = EntradaConsola.LeerEntero("Id del usuario: ", 1, int.MaxValue);
        UsuarioBase usuario = _servicio.ObtenerPorId(id);
        Console.WriteLine(Describir(usuario));

        Console.Write("Contraseña nueva (mín. 6 caracteres): ");
        string nueva = EntradaConsola.LeerContrasenaOculta();

        _servicio.CambiarContrasena(id, nueva);
        Console.WriteLine("Contraseña actualizada.");
    }

    private void DarDeBaja()
    {
        int id = EntradaConsola.LeerEntero("Id del usuario: ", 1, int.MaxValue);
        UsuarioBase usuario = _servicio.ObtenerPorId(id);
        Console.WriteLine(Describir(usuario));

        if (!EntradaConsola.Confirmar("¿Dar de baja a este usuario?"))
        {
            Console.WriteLine("Operación cancelada.");
            return;
        }

        _servicio.DarDeBaja(id);
        Console.WriteLine("Usuario dado de baja.");
    }

    private void Reactivar()
    {
        int id = EntradaConsola.LeerEntero("Id del usuario: ", 1, int.MaxValue);
        _servicio.Reactivar(id);
        Console.WriteLine("Usuario reactivado.");
    }

    private string Describir(UsuarioBase u)
    {
        return "[" + u.Id + "] " + u.Nombre_Usuario + " | " + u.Apellido + ", " + u.Nombre + " | DNI " + u.Dni + " | " + u.Rol + " | " + (u.Activo ? "Activo" : "Inactivo");
    }
}
