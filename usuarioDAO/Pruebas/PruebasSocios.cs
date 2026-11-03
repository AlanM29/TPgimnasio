public static class PruebasSocios
{
    private static int _ok;
    private static int _error;

    public static void Ejecutar()
    {
        _ok = 0;
        _error = 0;
        Console.WriteLine("=== PRUEBAS DE SOCIOS ===\n");

        string mensaje;
        if (!InicializadorBd.Inicializar(out mensaje))
        {
            Console.WriteLine(mensaje);
            return;
        }

        PruebasModelo();
        PruebasDao();

        Console.WriteLine("\n=== RESULTADO: " + _ok + " correctas, " + _error + " con error ===");
    }
    private static void PruebasModelo()
    {
        Console.WriteLine("--- Modelo ---");
        DateTime hoy = DateTime.Today;

        Rechazo("DNI vacío", () => new Socio("", "Ana", "Pérez", null, null, hoy));
        Rechazo("DNI con letras", () => new Socio("12ab5678", "Ana", "Pérez", null, null, hoy));
        Rechazo("DNI con puntos", () => new Socio("12.345.678", "Ana", "Pérez", null, null, hoy));
        Rechazo("DNI de 6 dígitos", () => new Socio("123456", "Ana", "Pérez", null, null, hoy));
        Rechazo("DNI de 9 dígitos", () => new Socio("123456789", "Ana", "Pérez", null, null, hoy));
        Rechazo("Nombre vacío", () => new Socio("12345678", "", "Pérez", null, null, hoy));
        Rechazo("Apellido vacío", () => new Socio("12345678", "Ana", "  ", null, null, hoy));
        Rechazo("Email inválido", () => new Socio("12345678", "Ana", "Pérez", "abc", null, hoy));
        Rechazo("Teléfono inválido", () => new Socio("12345678", "Ana", "Pérez", null, "abc", hoy));
        Aceptado("DNI de 7 dígitos válido", () => new Socio("1234567", "Ana", "Pérez", null, null, hoy));
        Aceptado("Datos completos válidos", () => new Socio("12345678", "Ana", "Pérez", "a@b.com", "341-555-1234", hoy));

        Socio s = new Socio("12345678", "Ana", "Pérez", "  ", "", hoy);
        Verificar("Email y teléfono vacíos se guardan como null", s.Email == null && s.Telefono == null);
        Verificar("Un socio nuevo está activo y con Id 0", s.Activo && s.Id == 0);

        s.DarDeBaja();
        Verificar("DarDeBaja() lo desactiva", !s.Activo);
        s.Reactivar();
        Verificar("Reactivar() lo activa", s.Activo);

        s.ActualizarContacto("María", "Gómez", "m@g.com", "341-555-0000");
        Verificar("ActualizarContacto cambia los datos", s.Nombre == "María" && s.Apellido == "Gómez");
        Rechazo("ActualizarContacto con email inválido", () => s.ActualizarContacto("María", "Gómez", "malo", null));
        // ---------- Persona (herencia) ----------  ← pegá desde acá
        Console.WriteLine("\n--- Persona (herencia) ---");
        Persona p1 = new Socio("12345678", "Ana", "Pérez", null, null, hoy);
        Persona p2 = new Admin("anita", "h", "Ana", "Pérez", "87654321", null, null, hoy);
        Verificar("Un Socio es una Persona", p1 is Persona);
        Verificar("Un Admin es una Persona y también un UsuarioBase", p2 is Persona && p2 is UsuarioBase);
        Verificar("Ambos conservan el DNI", p1.Dni == "12345678" && p2.Dni == "87654321");
        Rechazo("Un usuario con DNI inválido", () => new Admin("anita", "h", "Ana", "Pérez", "abc", null, null, hoy));
        Rechazo("Un usuario con DNI vacío", () => new Recepcionista("lucia", "h", "Lu", "Gómez", "", null, null, hoy));

}

    private static void PruebasDao()
    {
        Console.WriteLine("\n--- DAO (usa la base real) ---");
        ISocioDAO dao = new SocioDAO();

        // DNI y apellido distintos en cada corrida, para poder repetir las pruebas
        string dni = (DateTime.Now.Ticks % 90000000 + 10000000).ToString();
        string apellido = "Tester" + dni;

        Socio nuevo = new Socio(dni, "Juan", apellido, "juan" + dni + "@mail.com", "341-555-0000", DateTime.Now);
        int id = dao.Insertar(nuevo);
        Verificar("Insertar devuelve un id mayor a 0 y lo asigna al objeto", id > 0 && nuevo.Id == id);

        Socio? porDni = dao.ObtenerPorDni(dni);
        Verificar("ObtenerPorDni lo encuentra con sus datos",
                  porDni != null && porDni.Id == id && porDni.Nombre == "Juan" && porDni.Activo);
        Verificar("ObtenerPorId lo encuentra", dao.ObtenerPorId(id)?.Dni == dni);
        Verificar("Un DNI inexistente devuelve null", dao.ObtenerPorDni("00000001") == null);
        Verificar("Un id inexistente devuelve null", dao.ObtenerPorId(-1) == null);

        try
        {
            dao.Insertar(new Socio(dni, "Otro", "Otro", null, null, DateTime.Now));
            Verificar("DNI duplicado es rechazado", false);
        }
        catch (InvalidOperationException ex)
        {
            Verificar("DNI duplicado es rechazado con mensaje claro", ex.Message.Contains("DNI"));
        }

        Verificar("BuscarPorApellido encuentra exactamente 1", dao.BuscarPorApellido(apellido).Count == 1);
        Verificar("BuscarPorApellido sin coincidencias devuelve lista vacía",
                  dao.BuscarPorApellido("NoExiste" + dni).Count == 0);

        porDni!.ActualizarContacto("Juan Carlos", apellido, null, "341-555-9999");
        Verificar("Actualizar devuelve true", dao.Actualizar(porDni));
        Socio? releido = dao.ObtenerPorId(id);
        Verificar("Actualizar persistió nombre, teléfono y email nulo",
                  releido!.Nombre == "Juan Carlos" && releido.Telefono == "341-555-9999" && releido.Email == null);

        Verificar("DarDeBaja devuelve true", dao.DarDeBaja(id));
        Verificar("Tras la baja el socio figura inactivo", dao.ObtenerPorId(id)!.Activo == false);
        Verificar("ObtenerTodos(soloActivos) NO lo incluye", !dao.ObtenerTodos(true).Exists(x => x.Id == id));
        Verificar("ObtenerTodos(todos) SÍ lo incluye", dao.ObtenerTodos(false).Exists(x => x.Id == id));
        Verificar("DarDeBaja de un id inexistente devuelve false", !dao.DarDeBaja(-1));

        Verificar("Reactivar devuelve true", dao.Reactivar(id));
        Verificar("Tras reactivar figura activo", dao.ObtenerPorId(id)!.Activo);
    }

    // ---------- Ayudas ----------

    private static void Verificar(string descripcion, bool condicion)
    {
        if (condicion)
        {
            _ok++;
            Console.WriteLine("[OK]    " + descripcion);
        }
        else
        {
            _error++;
            Console.WriteLine("[ERROR] " + descripcion);
        }
    }

    private static void Rechazo(string descripcion, Action accion)
    {
        try
        {
            accion();
            Verificar(descripcion + " es rechazado", false);
        }
        catch (ArgumentException)
        {
            Verificar(descripcion + " es rechazado", true);
        }
    }

    private static void Aceptado(string descripcion, Action accion)
    {
        try
        {
            accion();
            Verificar(descripcion, true);
        }
        catch (ArgumentException)
        {
            Verificar(descripcion, false);
        }
    }
}