//  PRUEBAS de Asistencia y AsistenciaDao.  Correr con:  dotnet run -- --pruebas-asistencias
public static class PruebasAsistencias
{
    private static int _ok;
    private static int _error;

    public static void Ejecutar()
    {
        _ok = 0;
        _error = 0;
        Console.WriteLine("=== PRUEBAS DE ASISTENCIAS ===\n");

        string mensaje;
        if (!InicializadorBd.Inicializar(out mensaje))
        {
            Console.WriteLine(mensaje);
            return;
        }

        Console.WriteLine("--- Modelo ---");
        DateTime ahora = DateTime.Now;
        Rechazo("Socio con id 0", () => new Asistencia(0, ahora));
        Rechazo("Socio con id negativo", () => new Asistencia(-5, ahora));
        Rechazo("Fecha futura", () => new Asistencia(1, ahora.AddDays(1)));
        Aceptado("Datos válidos", () => new Asistencia(1, ahora));
        Verificar("Una asistencia nueva tiene Id 0", new Asistencia(1, ahora).Id == 0);

        Console.WriteLine("\n--- DAO (usa la base real) ---");
        ISocioDAO socios = new SocioDAO();
        IAsistenciaDAO dao = new AsistenciaDAO();

        // Un socio de prueba con DNI distinto en cada corrida
        string dni = (DateTime.Now.Ticks % 90000000 + 10000000).ToString();
        Socio socio = new Socio(dni, "Prueba", "Asist" + dni, null, null, DateTime.Now);
        socios.Insertar(socio);

        Verificar("Un socio nuevo no tiene asistencias hoy", !dao.ExisteAsistenciaEnFecha(socio.Id, DateTime.Today));

        Asistencia a1 = new Asistencia(socio.Id, DateTime.Now.AddMinutes(-30));
        int id = dao.Insertar(a1);
        Verificar("Insertar devuelve un id mayor a 0 y lo asigna", id > 0 && a1.Id == id);
        Verificar("Ahora sí existe asistencia hoy", dao.ExisteAsistenciaEnFecha(socio.Id, DateTime.Today));
        Verificar("No existe asistencia el día de ayer", !dao.ExisteAsistenciaEnFecha(socio.Id, DateTime.Today.AddDays(-1)));

        Asistencia ayer = new Asistencia(socio.Id, DateTime.Now.AddDays(-1));
        dao.Insertar(ayer);
        List<Asistencia> lista = dao.ObtenerPorSocio(socio.Id);
        Verificar("ObtenerPorSocio devuelve las 2 asistencias", lista.Count == 2);
        Verificar("Vienen ordenadas, la más reciente primero", lista[0].FechaHora > lista[1].FechaHora);
        Verificar("ObtenerPorFecha(hoy) incluye la de hoy",
                  dao.ObtenerPorFecha(DateTime.Today).Exists(x => x.Id == a1.Id));
        Verificar("ObtenerPorFecha(hoy) NO incluye la de ayer",
                  !dao.ObtenerPorFecha(DateTime.Today).Exists(x => x.Id == ayer.Id));
        Verificar("Un socio sin asistencias devuelve lista vacía", dao.ObtenerPorSocio(-1).Count == 0);

        try
        {
            dao.Insertar(new Asistencia(999999,DateTime.Now));
            Verificar("Insertar con un socio inexistente es rechazado", false);
        }
        catch (InvalidOperationException ex)
        {
            Verificar("Insertar con un socio inexistente es rechazado con mensaje claro", ex.Message.Contains("no existe"));
        }

        Console.WriteLine("\n=== RESULTADO: " + _ok + " correctas, " + _error + " con error ===");
    }

    private static void Verificar(string descripcion, bool condicion)
    {
        if (condicion) { _ok++; Console.WriteLine("[OK]    " + descripcion); }
        else { _error++; Console.WriteLine("[ERROR] " + descripcion); }
    }

    private static void Rechazo(string descripcion, Action accion)
    {
        try { accion(); Verificar(descripcion + " es rechazado", false); }
        catch (ArgumentException) { Verificar(descripcion + " es rechazado", true); }
    }

    private static void Aceptado(string descripcion, Action accion)
    {
        try { accion(); Verificar(descripcion, true); }
        catch (ArgumentException) { Verificar(descripcion, false); }
    }
}