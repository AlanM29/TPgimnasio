//  PRUEBAS de ServicioAsistencias (DAOs falsos, sin base de datos).
//  Correr con:  dotnet run -- --pruebas-servicio-asistencias
public static class PruebasServicioAsistencias
{
    private static int _ok;
    private static int _error;

    public static void Ejecutar()
    {
        _ok = 0;
        _error = 0;
        Console.WriteLine("=== PRUEBAS DE ServicioAsistencias (DAOs falsos) ===\n");

        DateTime hoy = DateTime.Today;
        UsuarioBase recep = new Recepcionista("recepprueba", "h", "Lucía", "Gómez", "22222222", null, null, hoy);

        Licencia completa = new Licencia("COMPLETA", true, "ok", "2099-12-31");
        Licencia basica = new Licencia("BASICA", true, "ok", "2099-12-31");

        // ---- Plan COMPLETA ----
        Console.WriteLine("--- Plan COMPLETA ---");
        SocioDaoFalso socios = new SocioDaoFalso();
        Socio juan = new Socio("30111222", "Juan", "Pérez", null, null, hoy);
        Socio baja = new Socio("30333444", "María", "Gómez", null, null, hoy);
        socios.Insertar(juan);
        socios.Insertar(baja);
        baja.DarDeBaja();

        ValidadorFalso validador = new ValidadorFalso { Resultado = true };
        AsistenciaDaoFalso asistencias = new AsistenciaDaoFalso();
        ServicioAsistencias servicio = new ServicioAsistencias(asistencias,  validador, socios, completa);

        Asistencia a = servicio.Registrar(recep, "30111222");
        Verificar("Registra la asistencia de un socio con membresía al día", a.Id > 0);
        Verificar("Quedó guardada una asistencia", asistencias.Datos.Count == 1);
        Lanza<InvalidOperationException>("Una segunda asistencia el mismo día es rechazada",
            () => servicio.Registrar(recep, "30111222"));
        Lanza<InvalidOperationException>("DNI inexistente es rechazado",
            () => servicio.Registrar(recep, "99999999"));
        Lanza<InvalidOperationException>("Socio dado de baja es rechazado",
            () => servicio.Registrar(recep, "30333444"));
        Lanza<ArgumentException>("DNI con formato inválido es rechazado",
            () => servicio.Registrar(recep, "abc"));
        Verificar("Los rechazos no guardaron nada (sigue 1)", asistencias.Datos.Count == 1);

        // Membresía vencida o impaga con plan COMPLETA
        Socio ana = new Socio("30555666", "Ana", "Díaz", null, null, hoy);
        socios.Insertar(ana);
        validador.Resultado = false;
        Lanza<InvalidOperationException>("COMPLETA: sin membresía vigente y pagada es rechazada",
            () => servicio.Registrar(recep, "30555666"));
        Verificar("Tras el rechazo no se guardó nada (sigue 1)", asistencias.Datos.Count == 1);
        validador.Resultado = true;
        Verificar("Cuando la membresía está al día, ahora sí registra", servicio.Registrar(recep, "30555666").Id > 0);

        // ---- Plan BASICA ----
        Console.WriteLine("\n--- Plan BASICA (no valida membresía) ---");
        AsistenciaDaoFalso asistencias2 = new AsistenciaDaoFalso();
        ValidadorFalso validadorSiempreNo = new ValidadorFalso { Resultado = false };
        ServicioAsistencias servicioBasica = new ServicioAsistencias(asistencias2,validadorSiempreNo, socios, basica);

        Verificar("BASICA: registra aunque NO tenga membresía vigente y pagada",
                  servicioBasica.Registrar(recep, "30111222").Id > 0);
        Verificar("BASICA: ni siquiera consulta al validador", validadorSiempreNo.Consultas == 0);
        Lanza<InvalidOperationException>("BASICA: igual rechaza al socio dado de baja",
            () => servicioBasica.Registrar(recep, "30333444"));
        Lanza<InvalidOperationException>("BASICA: igual rechaza la segunda del día",
            () => servicioBasica.Registrar(recep, "30111222"));

        // ---- Consultas ----
        Console.WriteLine("\n--- Consultas ---");
        Verificar("HistorialDeSocio devuelve las asistencias de Juan", servicio.HistorialSocio("30111222").Count == 1);
        Verificar("Un socio sin asistencias devuelve lista vacía",
                  servicio.HistorialSocio("30333444").Count == 0);
        Lanza<InvalidOperationException>("Historial de un DNI inexistente es rechazado",
            () => servicio.HistorialSocio("99999999"));
        Verificar("AsistenciasDelDia(hoy) trae las 2 de hoy", servicio.AsistenciasdelDia(DateTime.Today).Count == 2);
        Verificar("ObtenerSocioPorId encuentra a Juan", servicio.ObtenerSocioPorId(juan.Id)?.Dni == "30111222");

        Console.WriteLine("\n=== RESULTADO: " + _ok + " correctas, " + _error + " con error ===");
    }

    private static void Verificar(string descripcion, bool condicion)
    {
        if (condicion) { _ok++; Console.WriteLine("[OK]    " + descripcion); }
        else { _error++; Console.WriteLine("[ERROR] " + descripcion); }
    }

    private static void Lanza<T>(string descripcion, Action accion) where T : Exception
    {
        try { accion(); Verificar(descripcion, false); }
        catch (Exception ex) when (ex is T) { Verificar(descripcion, true); }
        catch (Exception ex) { Verificar(descripcion + " (lanzó " + ex.GetType().Name + ")", false); }
    }

    // ---------- Dobles de prueba ----------

    private class ValidadorFalso : IValidadorMembresia
    {
        public bool Resultado { get; set; }
        public int Consultas { get; private set; }

        public bool TieneMembresiaVigenteyPagada(int idSocio)
        {
            Consultas++;
            return Resultado;
        }
    }

    private class AsistenciaDaoFalso : IAsistenciaDAO
    {
        public List<Asistencia> Datos { get; } = new List<Asistencia>();
        private int _siguienteId = 1;

        public int Insertar(Asistencia asistencia)
        {
            asistencia.AsignarId(_siguienteId++);
            Datos.Add(asistencia);
            return asistencia.Id;
        }
        public List<Asistencia> ObtenerPorSocio(int idSocio) => Datos.FindAll(x => x.Id_Socio == idSocio);
        public List<Asistencia> ObtenerPorFecha(DateTime fecha) => Datos.FindAll(x => x.FechaHora.Date == fecha.Date);
        public bool ExisteAsistenciaEnFecha(int idSocio, DateTime fecha) =>
            Datos.Exists(x => x.Id_Socio == idSocio && x.FechaHora.Date == fecha.Date);
    }

    private class SocioDaoFalso : ISocioDAO
    {
        private readonly List<Socio> _datos = new List<Socio>();
        private int _siguienteId = 1;

        public Socio? ObtenerPorId(int id) => _datos.Find(s => s.Id == id);
        public Socio? ObtenerPorDni(string dni) => _datos.Find(s => s.Dni == dni);
        public List<Socio> ObtenerTodos(bool soloActivos) => _datos.FindAll(s => !soloActivos || s.Activo);
        public List<Socio> BuscarPorApellido(string texto) => _datos.FindAll(s => s.Apellido.Contains(texto));
        public int Insertar(Socio socio) { socio.AsignarId(_siguienteId++); _datos.Add(socio); return socio.Id; }
        public bool Actualizar(Socio socio) => _datos.Exists(s => s.Id == socio.Id);
        public bool DarDeBaja(int id) { Socio? s = ObtenerPorId(id); if (s == null) return false; s.DarDeBaja(); return true; }
        public bool Reactivar(int id) { Socio? s = ObtenerPorId(id); if (s == null) return false; s.Reactivar(); return true; }
    }
}