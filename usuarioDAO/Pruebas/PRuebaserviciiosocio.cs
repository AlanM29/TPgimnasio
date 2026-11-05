//  PRUEBAS de ServicioSocios con un DAO falso (sin base de datos).
//  Correr con:  dotnet run -- --pruebas-servicio-socios
public static class PruebasServicioSocios
{
    private static int _ok;
    private static int _error;

    public static void Ejecutar()
    {
        _ok = 0;
        _error = 0;
        Console.WriteLine("=== PRUEBAS DE ServicioSocios (DAO falso, sin base de datos) ===\n");

        DateTime hoy = DateTime.Today;
        UsuarioBase admin = new Admin("adminprueba", "h", "Ana", "Pérez", "11111111", null, null, hoy);
        UsuarioBase recep = new Recepcionista("recepprueba", "h", "Lucía", "Gómez", "22222222", null, null, hoy);
        ServicioSocio servicio = new ServicioSocio(new SocioDaoFalso());
        bool reactivado;

        Console.WriteLine("--- Alta ---");
        Socio s1 = servicio.Alta(admin, "30111222", "Juan", "Pérez", "juan@mail.com", "341-555-1111", out reactivado);
        Verificar("El socio queda con id asignado y no es una reactivación", s1.Id > 0 && !reactivado);
        Socio s2 = servicio.Alta(recep, "30333444", "María", "Gómez", null, null, out reactivado);
        Verificar("Un Recepcionista también puede dar de alta", s2.Id > 0);
        Lanza<InvalidOperationException>("DNI de un socio activo es rechazado",
            () => servicio.Alta(admin, "30111222", "Otro", "Otro", null, null, out _));
        Lanza<ArgumentException>("DNI con letras es rechazado",
            () => servicio.Alta(admin, "abc", "A", "B", null, null, out _));
        Lanza<ArgumentException>("Nombre vacío es rechazado",
            () => servicio.Alta(admin, "30555666", "", "B", null, null, out _));
        Verificar("Las altas rechazadas no guardaron nada (siguen 2 socios)", servicio.Listar(false).Count == 2);

        Console.WriteLine("\n--- Consultas ---");
        Verificar("BuscarPorDni encuentra a Juan", servicio.BuscarPorDni("30111222")?.Nombre == "Juan");
        Verificar("BuscarPorDni de un DNI inexistente devuelve null", servicio.BuscarPorDni("99999999") == null);
        Lanza<ArgumentException>("BuscarPorDni con formato inválido es rechazado", () => servicio.BuscarPorDni("12"));
        Verificar("BuscarPorApellido encuentra a Pérez (sin importar mayúsculas)",
            servicio.BuscarPorApellido("pérez").Count == 1);
        Lanza<ArgumentException>("BuscarPorApellido vacío es rechazado", () => servicio.BuscarPorApellido("  "));

        Console.WriteLine("\n--- Modificación ---");
        Socio m = servicio.Modificar(recep, s1.Id, "Juan Carlos", "Pérez", null, "341-555-9999");
        Verificar("Modificar cambia nombre y teléfono", m.Nombre == "Juan Carlos" && m.Telefono == "341-555-9999");
        Verificar("Modificar no cambia el DNI", m.Dni == "30111222");
        Lanza<InvalidOperationException>("Modificar un id inexistente es rechazado",
            () => servicio.Modificar(admin, -1, "A", "B", null, null));
        Lanza<ArgumentException>("Modificar con email inválido es rechazado",
            () => servicio.Modificar(admin, s1.Id, "Juan", "Pérez", "malo", null));

        Console.WriteLine("\n--- Baja y permisos ---");
        Lanza<UnauthorizedAccessException>("Un Recepcionista NO puede dar de baja",
            () => servicio.DarDeBaja(recep, s2.Id));
        Verificar("Tras el intento denegado el socio sigue activo", servicio.BuscarPorDni("30333444")!.Activo);
        servicio.DarDeBaja(admin, s2.Id);
        Verificar("Un Admin SÍ puede y el socio queda inactivo", !servicio.BuscarPorDni("30333444")!.Activo);
        Lanza<InvalidOperationException>("Baja de un socio ya inactivo es rechazada",
            () => servicio.DarDeBaja(admin, s2.Id));
        Lanza<InvalidOperationException>("Baja de un id inexistente es rechazada",
            () => servicio.DarDeBaja(admin, -1));
        Lanza<UnauthorizedAccessException>("El permiso se chequea antes que la existencia",
            () => servicio.DarDeBaja(recep, -1));
        Lanza<InvalidOperationException>("Modificar un socio inactivo es rechazado",
            () => servicio.Modificar(admin, s2.Id, "M", "G", null, null));
        Verificar("Listar(soloActivos) excluye al inactivo", servicio.Listar(true).Count == 1);
        Verificar("Listar(todos) incluye al inactivo", servicio.Listar(false).Count == 2);

        Console.WriteLine("\n--- Reactivación por alta ---");
        Socio r = servicio.Alta(recep, "30333444", "María Laura", "Gómez", "ml@mail.com", null, out reactivado);
        Verificar("Alta con DNI de un socio inactivo lo reactiva", reactivado && r.Activo);
        Verificar("La reactivación conserva el mismo Id", r.Id == s2.Id);
        Verificar("La reactivación actualiza los datos", r.Nombre == "María Laura" && r.Email == "ml@mail.com");
        Verificar("No se duplicó el socio (siguen 2)", servicio.Listar(false).Count == 2);

        Console.WriteLine("\n=== RESULTADO: " + _ok + " correctas, " + _error + " con error ===");
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

    // Comprueba que la acción lance exactamente el tipo de excepción esperado
    private static void Lanza<T>(string descripcion, Action accion) where T : Exception
    {
        try
        {
            accion();
            Verificar(descripcion, false);
        }
        catch (Exception ex) when (ex is T)
        {
            Verificar(descripcion, true);
        }
        catch (Exception ex)
        {
            Verificar(descripcion + " (lanzó " + ex.GetType().Name + ")", false);
        }
    }

    // DAO falso: cumple el contrato ISocioDao pero guarda todo en una lista en memoria
    private class SocioDaoFalso : ISocioDAO
    {
        private readonly List<Socio> _datos = new List<Socio>();
        private int _siguienteId = 1;

        public Socio? ObtenerPorId(int id) => _datos.Find(s => s.Id == id);
        public Socio? ObtenerPorDni(string dni) => _datos.Find(s => s.Dni == dni);
        public List<Socio> ObtenerTodos(bool soloActivos) => _datos.FindAll(s => !soloActivos || s.Activo);
        public List<Socio> BuscarPorApellido(string texto) =>
            _datos.FindAll(s => s.Apellido.Contains(texto, StringComparison.OrdinalIgnoreCase));

        public int Insertar(Socio socio)
        {
            socio.AsignarId(_siguienteId++);
            _datos.Add(socio);
            return socio.Id;
        }

        public bool Actualizar(Socio socio) => _datos.Exists(s => s.Id == socio.Id);

        public bool DarDeBaja(int id)
        {
            Socio? s = ObtenerPorId(id);
            if (s == null) return false;
            s.DarDeBaja();
            return true;
        }

        public bool Reactivar(int id)
        {
            Socio? s = ObtenerPorId(id);
            if (s == null) return false;
            s.Reactivar();
            return true;
        }
    }
}