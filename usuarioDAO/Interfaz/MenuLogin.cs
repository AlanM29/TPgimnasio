using System;

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
            string clave = LeerContrasenaOculta();

            // Validación: no se aceptan campos vacíos
            if (string.IsNullOrWhiteSpace(nombre) || string.IsNullOrWhiteSpace(clave))
            {
                Console.WriteLine("Tenés que completar usuario y contraseña.\n");
                continue;
            }

            UsuarioBase? usuario = login.Ingresar(nombre.Trim(), clave);
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

    // Lee la contraseña 
    private static string LeerContrasenaOculta()
    {
        string clave = "";
        ConsoleKeyInfo tecla;

        do
        {
            tecla = Console.ReadKey(true);

            if (tecla.Key == ConsoleKey.Backspace)
            {
                if (clave.Length > 0)
                {
                    clave = clave.Substring(0, clave.Length - 1);
                    Console.Write("\b \b");
                }
            }
            else if (tecla.Key != ConsoleKey.Enter && !char.IsControl(tecla.KeyChar))
            {
                clave += tecla.KeyChar;
                Console.Write("*");
            }
        } while (tecla.Key != ConsoleKey.Enter);

        Console.WriteLine();
        return clave;
    }
}
