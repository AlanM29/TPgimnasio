//  INTERFAZ: MenuSocios
//  Pide datos por consola y muestra resultados. No tiene reglas de negocio ni SQL:
//  todo eso lo resuelve ServicioSocios.
public class MenuSocios
{
    private readonly ServicioSocio _ServicioSocio;//crea una instancia de Serviciosocio para usar sus metodos
    private readonly UsuarioBase _UsuarioActual;//guarda el usuario que está usando el menú, para registrar en el log
    public MenuSocios(ServicioSocio _serviciosocio, UsuarioBase _usuarioactual)
    {
        _ServicioSocio = _serviciosocio;
        _UsuarioActual = _usuarioactual;
    }
    public void MostrarMenu()
    {
        int opcion;
        do
        {
            MostrarOpciones();
            opcion = LeerOpcion("opcion: ", 0, 4);
            EjecutarOpcion(opcion);
        } while (opcion != 0);
    }
    private void MostrarOpciones()
    {
        Console.WriteLine("1. Dar de alta un socio ");
        Console.WriteLine("2. Consultar socios ");
        Console.WriteLine("3. Modificar datos de un socio ");
        if (_UsuarioActual.PuedeDarDeBajaSocios)//si el usuario tiene permiso para dar de baja socios, muestra la opción
        {
            Console.WriteLine("4. Dar de baja un socio ");
        }
        Console.WriteLine("0. Salir");
        Console.Write("Seleccione una ");
    }
    private void EjecutarOpcion(int Opcion)
    {
        try
        {
            switch (Opcion)
            {
                case 1: Alta(); break;
                case 2: Consultar(); break;
                case 3: Modificar(); break;
                case 4: Baja(); break;
            }
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine("Error de validación: " + ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            Console.WriteLine("Operación inválida: " + ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            Console.WriteLine(ex.Message);
        }

        catch (Exception ex)
        {
            Console.WriteLine("Error inesperado: " + ex.Message);
        }

    }
    private void Alta()
    {
        Console.WriteLine("-------------------ALTA DE SOCIOS---------------------");
        string dni = LeerCampo("DNI: ", v => Validaciones.ValidarDni(v))!;
        string nombre = LeerCampo("Nombre: ", v => Validaciones.ValidarObligatorio(v, "El nombre"))!;
        string apellido = LeerCampo("Apellido: ", v => Validaciones.ValidarObligatorio(v, "El apellido"))!;
        string? email = LeerCampo("Email: ", v => Validaciones.ValidarEmail(v));
        string? telefono = LeerCampo("Telefono: ", v => Validaciones.ValidarTelefono(v));
        bool reactivado;
        Socio socio = _ServicioSocio.Alta(_UsuarioActual, dni, nombre, apellido, email, telefono, out reactivado);

        if (reactivado)
        {
            Console.WriteLine("\nEse DNI pertenecía a un socio dado de baja: se lo reactivó con los datos nuevos.");
        }
        else
        {
            Console.WriteLine("\nSocio registrado correctamente.");
        }
        MostrarDetalle(socio);
    }
    private void Consultar()
    {
        Console.WriteLine("-------------------CONSULTA DE SOCIOS---------------------");
        Console.WriteLine("1-Buscar por DNI");
        Console.WriteLine("2-Buscar por apellido");
        Console.WriteLine("3-Listar socios activos");
        Console.WriteLine("4-Listar todos los socios");
        Console.WriteLine("0-Volver");
        int tipo = LeerOpcion("Elija una opcion: ", 0, 4);
        switch (tipo)
        {
            case 1:
                string dni = LeerCampo(tipo + "-DNI: ", v => Validaciones.ValidarDni(v))!;
                Socio? socio = _ServicioSocio.BuscarPorDni(dni);
                if (socio != null)
                {
                    MostrarDetalle(socio);

                }
                else
                {
                    Console.WriteLine("No se encontró un socio con ese DNI.");
                }
                break;
            case 2:
                Console.Write("Apellido (o parte): ");
                MostrarLista(_ServicioSocio.BuscarPorApellido(Console.ReadLine() ?? ""));
                break;
            case 3:
                MostrarLista(_ServicioSocio.Listar(true));
                break;
            case 4:
                MostrarLista(_ServicioSocio.Listar(false));
                break;
        }


    }
    private void Modificar()
    {
        Console.WriteLine("-------------------MODIFICAR SOCIO---------------------");
        string dni = LeerCampo("DNI del socio: ", v => Validaciones.ValidarDni(v))!;
        Socio? socio = _ServicioSocio.BuscarPorDni(dni);
        if (socio == null) {
            Console.WriteLine("No se encontro un socio con ese DNI.");
                return;
        }
        MostrarDetalle(socio);
        Console.WriteLine("\nDejá vacío para conservar el valor actual.");

        string nombre = LeerCampoOActual("Nombre", socio.Nombre,
            v => Validaciones.ValidarObligatorio(v, "El nombre"))!;
        string apellido = LeerCampoOActual("Apellido", socio.Apellido,
            v => Validaciones.ValidarObligatorio(v, "El apellido"))!;
        string? email = LeerCampoOActual("Email", socio.Email, v => Validaciones.ValidarEmail(v));
        string? telefono = LeerCampoOActual("Teléfono", socio.Telefono, v => Validaciones.ValidarTelefono(v));
        Socio modificado = _ServicioSocio.Modificar(_UsuarioActual, socio.Id, nombre, apellido, email, telefono);

        Console.WriteLine("\nSocio modificado correctamente.");
        MostrarDetalle(modificado);
    }
    private void Baja()
    {
        if (!_UsuarioActual.PuedeDarDeBajaSocios)
        {
            Console.WriteLine("No tenés permiso para dar de baja socios.");
            return;
        }
        Console.WriteLine("-------------------DAR DE BAJA SOCIO----------------------");
        string dni = LeerCampo("DNI del socio: ", v => Validaciones.ValidarDni(v))!;
        Socio? socio = _ServicioSocio.BuscarPorDni(dni);
        if (socio == null)
        {
            Console.WriteLine("No se encontró un socio con ese DNI.");
            return;
        }
        MostrarDetalle(socio);
        if(!LeerSioNO("Confirmas la baja del socio?")){
            Console.WriteLine("baja cancelada");
            return;
        }
        _ServicioSocio.DarDeBaja(_UsuarioActual,socio.Id);//llama al metodo del servicio baja que actualiza el socio en la base de datos y registra en el log
        Console.WriteLine("Socio dado de baja correctamente.");
    }
    private static void MostrarDetalle(Socio s)
    {
        Console.WriteLine("  DNI:      " + s.Dni);
        Console.WriteLine("  Nombre:   " + s.Nombre + " " + s.Apellido);
        Console.WriteLine("  Email:    " + (s.Email ?? "(sin dato)"));
        Console.WriteLine("  Teléfono: " + (s.Telefono ?? "(sin dato)"));
        Console.WriteLine("  Alta:     " + s.FechaRegistro.ToString("dd/MM/yyyy"));
        Console.WriteLine("  Estado:   " + (s.Activo ? "Activo" : "Inactivo"));
    }
    private static void MostrarLista(List<Socio> socios)
    {
        if (socios.Count == 0)
        {
            Console.WriteLine("No se encontraron socios.");
            return;
        }

        // Encabezado: cada columna tiene un ancho fijo
        Console.WriteLine($"{"DNI",-10} {"Nombre",-14} {"Apellido",-14} {"Email",-26} {"Teléfono",-15} {"Alta",-10} {"Estado",-8}");
        Console.WriteLine(new string('-', 104));

        foreach (Socio s in socios)
        {
            Console.WriteLine(
                $"{s.Dni,-10} " +
                $"{Cortar(s.Nombre, 14),-14} " +
                $"{Cortar(s.Apellido, 14),-14} " +
                $"{Cortar(s.Email ?? "(sin dato)", 26),-26} " +
                $"{Cortar(s.Telefono ?? "(sin dato)", 15),-15} " +
                $"{s.FechaRegistro:dd/MM/yyyy} " +
                $"{(s.Activo ? "Activo" : "Inactivo"),-8}");
        }

        Console.WriteLine("\nTotal: " + socios.Count);
    }

    private static int LeerOpcion(string mensaje, int min, int max)
    {
        int opcion;
        Console.WriteLine(mensaje);
        while (!int.TryParse(Console.ReadLine(), out  opcion) || opcion > max || opcion < min) { 
            Console.WriteLine($"Ingrese la opcion entre {min} y {max}");
        }
        return opcion;
    }
    private static string? LeerCampo(string mensaje, Func<string?, string?> validar)//func<string?, string?> es un delegado que recibe un string y devuelve un string en este caso, se usa para validar el campo ingresado por el usuario
    {
        while (true)
        {
            Console.Write(mensaje);
            try
            {
                return validar(Console.ReadLine());//intenta validar el campo ingresado, si es invalido lanza una expecion y vuelve a pedir
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
        }
    }
    private static string? LeerCampoOActual(string etiqueta, string? valoractual, Func<string?, string?> validar) {

        while (true)
        {
            Console.Write(etiqueta + " [" + (valoractual ?? "sin dato") + "]: ");//muestra el valor entre corchete, si es null muestra "sin datos"
            string? texto = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(texto)){
                return valoractual; //devuelve el valor actual si el usuario no ingresa nada
            }
            try
            {
                return validar(texto);//intenta validar el campo usando el delegado, si da error vuelve a pedir el campo
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
        }
    }
    private static bool LeerSioNO(string mensaje)
    {
        while (true)
        {
            Console.WriteLine(mensaje + " (S/N): ");
            string? input = (Console.ReadLine()??"").Trim().ToUpper();//lee la entrada, si es null lo hace un string vacio, ademas quita espacios y pasa a mayus
            if(input == "S")
            {
                return true;
            }
            if(input == "N")
            {
                return false;
            }
            Console.WriteLine("Respuesta invalida; Ingrese S para SI o N para NO: ");
        }
    }
    private static string Cortar(string texto, int ancho)
    {
        if (texto.Length <= ancho)
        {
            return texto;
        }
        return texto.Substring(0, ancho - 1) + "…";
    }
}