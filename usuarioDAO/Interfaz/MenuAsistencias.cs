public class MenuAsistencias
{
    private readonly ServicioAsistencias _servicio;
    private readonly UsuarioBase _usuario;
    public MenuAsistencias(ServicioAsistencias servicio, UsuarioBase usuario)
    {
        _servicio = servicio;
        _usuario = usuario;
    }
    public void MostrarMenu()
    {
        int opcion;
        do
        {
            Console.WriteLine("=== Menú de Asistencias ===");
            Console.WriteLine("1. Registrar Asistencia");
            Console.WriteLine("2. Historial de Asistencias de un Socio");
            Console.WriteLine("3. Asistencias de Hoy");
            Console.WriteLine("0. Salir");
            opcion = LeerOpcion("Opción: ", 0, 3);
            Ejecutar(opcion);
        } while(opcion != 0); 
    }
    private void Ejecutar(int opcion)
    {
        try
        {
            switch (opcion)
            {
                case 1: Registrar(); break;
                case 2: Historial(); break;
                case 3: DelDia(); break;
            }
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine("Dato inválido: " + ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            Console.WriteLine(ex.Message);
        }
        catch (Exception ex)
        {
            // El programa no se cierra: informa y vuelve al menú
            Console.WriteLine("Ocurrió un error inesperado: " + ex.Message);
            LogService.Registrar(_usuario.Nombre_Usuario, "ERROR", ex.Message);
        }
    }
    public void Registrar()
    {
        Console.WriteLine("---- Registrar asistencia ----");
        string dni = LeerDni();
        Asistencia asistencia = _servicio.Registrar(_usuario, dni);
        Socio socio = _servicio.ObtenerSocio(dni);
        Console.WriteLine("\nAsistencia registrada: " + socio.Nombre + " " + socio.Apellido +
                          " - " + asistencia.FechaHora.ToString("dd/MM/yyyy HH:mm"));
    }
    public void Historial()
    {
        Console.WriteLine("----Historial de un socio ----");
        string dni = LeerDni();//lee el dni para buscar el socio
        Socio socio = _servicio.ObtenerSocio(dni);
        List<Asistencia> lista = _servicio.HistorialSocio(dni);
        Console.WriteLine("\n" + socio.Nombre + " " + socio.Apellido + " (DNI " + socio.Dni + ")");
        if (lista.Count == 0)
        {
            Console.WriteLine("Todavía no registró asistencias.");
            return;
        }

        foreach (Asistencia a in lista)
        {
            Console.WriteLine("  " + a.FechaHora.ToString("dd/MM/yyyy HH:mm"));
        }
        Console.WriteLine("\nTotal: " + lista.Count);//total de asistencias del socio
    }
    public void DelDia()
    {
        Console.WriteLine("--- Asistencias de hoy ---");
        List<Asistencia> lista = _servicio.AsistenciasdelDia(DateTime.Today);
        if(lista.Count == 0)
        {
            Console.WriteLine("No se registraron asistencias hoy.");
            return;
        }
        Console.WriteLine($"{"Hora",-6} {"DNI",-10} {"Socio",-30}");
        Console.WriteLine(new string('-', 48));//new string crea una cadena de caracteres repetidos
        foreach (Asistencia a in lista)
        {
            Socio? s = _servicio.ObtenerSocioPorId(a.Id_Socio);//obtengo el socio por su id
            string dni = s != null ? s.Dni : "?";//si el socio no existe muestra un signo de interrogacion
            string nombre = s != null ? s.Apellido + ", " + s.Nombre : "(desconocido)";//si no existe muestra desconocido
            Console.WriteLine($"{a.FechaHora:HH:mm}  {dni,-10} {nombre,-30}");
        }
        Console.WriteLine("\nTotal: " + lista.Count);//muestra total de asistencias del dia
    }
    //Lecturas(Despues las acomodo en una clase)
    private static string LeerDni()
    {
        while (true)
        {
            Console.Write("DNI del socio: ");
            try
            {
                return Validaciones.ValidarDni(Console.ReadLine());
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
        }
    }

    private static int LeerOpcion(string mensaje, int min, int max)
    {
        int opcion;
        Console.Write(mensaje);
        while (!int.TryParse(Console.ReadLine(), out opcion) || opcion < min || opcion > max)
        {
            Console.Write("Ingresá un número entre " + min + " y " + max + ": ");
        }
        return opcion;
    }

}