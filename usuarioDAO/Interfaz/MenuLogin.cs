using System;
using System.Data.SQLite;

//  INTERFAZ: MenuLogin
public static class MenuLogin
{
    private const int IntentosMaximos = 3;

    // Devuelve el Usuario logueado, o null si falló los 3 intentos
    public static UsuarioBase? PedirCredenciales()
    {
        LoginService login = new LoginService();
        int intentos = 0;

        while (intentos < IntentosMaximos)
        {
            Console.Write("Usuario: ");
            string nombre = Console.ReadLine() ?? "";

            Console.Write("Contraseña: ");
            string clave = EntradaConsola.LeerContrasenaOculta();

            // Validación: no se aceptan campos vacíos
            if (string.IsNullOrWhiteSpace(nombre) || string.IsNullOrWhiteSpace(clave))
            {
                Console.WriteLine("Tenés que completar usuario y contraseña.\n");
                continue;
            }

            UsuarioBase? usuario;
            try
            {
                usuario = login.Ingresar(nombre.Trim(), clave);
            }
            catch (Exception ex) when (ex is SQLiteException || ex is ArgumentException)
            {
                // Falla de la base o un dato guardado inválido: no es culpa del usuario, no cuenta como intento
                Console.WriteLine("No se pudo iniciar sesión por un error del sistema: " + ex.Message);
                LogService.Registrar(nombre.Trim(), "ERROR_LOGIN", ex.Message);
                return null;
            }
            if (usuario != null)
            {
                LogService.Registrar(usuario.Nombre_Usuario, "LOGIN", "Ingreso correcto (rol " + usuario.Rol + ")");
                return usuario;
            }

            intentos++;
            LogService.Registrar(nombre.Trim(), "LOGIN_FALLIDO", "Usuario o contraseña incorrectos");
            Console.WriteLine("Usuario o contraseña incorrectos. Intentos restantes: " +
                              (IntentosMaximos - intentos) + "\n");
        }

        LogService.Registrar("sistema", "BLOQUEO", "Se superaron los " + IntentosMaximos + " intentos de login");
        return null;
    }
}
