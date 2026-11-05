 public class Program
    {
      public static void Main(string[] args)
      {
            // Modo pruebas de socios: dotnet run -- --pruebas-socios
            if (args.Length > 0 && args[0] == "--pruebas-socios")
            {
                PruebasSocios.Ejecutar();
                Console.WriteLine("\nPresioná una tecla para salir...");
                Console.ReadKey();
                return;
            }
         if (args.Length > 0 && args[0] == "--pruebas-servicio-asistencias")
         {
            PruebasServicioAsistencias.Ejecutar();
            Console.WriteLine("\nPresioná una tecla para salir...");
            Console.ReadKey();
            return;
         }


        // Modo pruebas: dotnet run -- --pruebas  (o el perfil "Pruebas" en Visual Studio)
        if (args.Length > 0 && args[0] == "--pruebas")
            {
                PruebasGenerales.Ejecutar();
                Console.WriteLine("\nPresioná una tecla para salir...");
                Console.ReadKey();
                return;
            }
          if (args.Length > 0 && args[0] == "--pruebas-servicio-socios")
           {
            PruebasServicioSocios.Ejecutar();
            Console.WriteLine("\nPresioná una tecla para salir...");
            Console.ReadKey();
            return;
           }
        if (args.Length > 0 && args[0] == "--pruebas-asistencias")
        {
            PruebasAsistencias.Ejecutar();
            Console.WriteLine("\nPresioná una tecla para salir...");
            Console.ReadKey();
            return;
        }

        Console.WriteLine("=== SISTEMA DE GESTIÓN DEL GIMNASIO ===\n");

            // 1. LICENCIA: si es inválida, el sistema se bloquea
            LicenciaService licenciaService = new LicenciaService();
            Licencia licencia = licenciaService.Cargar();

            if (!licencia.EsValida)
            {
                Console.WriteLine("SISTEMA BLOQUEADO");
                Console.WriteLine(licencia.Motivo);
                LogService.Registrar("sistema", "LICENCIA_INVALIDA", licencia.Motivo);
                Console.WriteLine("\nPresioná una tecla para salir...");
                Console.ReadKey();
                return;
            }

            LogService.Registrar("sistema", "INICIO", "Licencia " + licencia.Plan + " válida");

            // 2. BASE DE DATOS: se prepara sola (tablas y datos iniciales) si hace falta
            string mensajeBd;
            if (!InicializadorBd.Inicializar(out mensajeBd))
            {
                Console.WriteLine(mensajeBd);
                LogService.Registrar("sistema", "ERROR_BD", mensajeBd);
                Console.WriteLine("\nPresioná una tecla para salir...");
                Console.ReadKey();
                return;
            }

            // 3. Inicio de sesión
            UsuarioBase? actual = MenuLogin.PedirCredenciales();
            if (actual == null)
            {
                Console.WriteLine("Demasiados intentos fallidos. Cerrando el sistema.");
                return;
            }

            Console.WriteLine("\nBienvenido, " + actual.Nombre + " " + actual.Apellido + " (rol: " + actual.Rol + ")\n");

            // 4. Menú principal: depende del ROL y de la LICENCIA
            MenuPrincipal menu = new MenuPrincipal(actual, licencia);
            menu.Mostrar();
       }
 }