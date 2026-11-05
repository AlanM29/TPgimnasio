using System.ComponentModel.DataAnnotations;

public class ServicioAsistencias
{
    private readonly IAsistenciaDAO _asistenciaDAO;    //Llama a la asistencia, membrecia y sociodao para delegar persistencia de datos
    private readonly IValidadorMembresia _validadorMembresia;
    private readonly ISocioDAO _socioDAO;
    private readonly Licencia _licencia;
    public ServicioAsistencias(IAsistenciaDAO asistenciaDAO, IValidadorMembresia validadorMembresia, ISocioDAO socioDAO, Licencia licencia)
    {
        _asistenciaDAO = asistenciaDAO;
        _validadorMembresia = validadorMembresia;
        _socioDAO = socioDAO;
        _licencia = licencia;
    }
    public Asistencia Registrar(UsuarioBase actor, string dni)
    {
        string DNIvalido = Validaciones.ValidarDni(dni);// chequea que el dni sea valido
        Socio? socio = _socioDAO.ObtenerPorDni(DNIvalido);// busca el socio por dni
        if (socio == null || !socio.Activo)
        {
            throw new InvalidOperationException("No existe un socio con ese DNI") ?? new InvalidOperationException("El socio no esta activo");// si no existe un socio con ese dni o no esta activo, lanza una excepcion
        }
        if (_asistenciaDAO.ExisteAsistenciaEnFecha(socio.Id, DateTime.Today))
        {
            throw new InvalidOperationException("El socio ya registró su asistencia hoy.");//una asistenvia por socio por dia(se puede cambiar si se quiere permitir mas de una asistencia por dia)
        }
        if (_licencia.PermiteModulo(Modulos.Membresias))// esto se cambia cuando se implementen las membresias y pagos, por ahora acepta a todos
        {
            if (!_validadorMembresia.TieneMembresiaVigenteyPagada(socio.Id))
            {
                LogService.Registrar(actor.Nombre_Usuario, "ASISTENCIA_RECHAZADA",
                                     "DNI " + socio.Dni + " sin membresía vigente y pagada");
                throw new InvalidOperationException(
                    "El socio no tiene una membresía vigente y pagada.");
            }
        }
        Asistencia asistencia = new Asistencia(socio.Id, DateTime.Now);//crea la asistencia con la fecha y hora actual
        _asistenciaDAO.Insertar(asistencia);

        LogService.Registrar(actor.Nombre_Usuario, "ASISTENCIA", "DNI " + socio.Dni);//registra en el log que se registro la asistencia de un socio
        return asistencia;
    }
    public List<Asistencia> HistorialSocio(string dni)
    {
       Socio socio = _socioDAO.ObtenerPorDni(Validaciones.ValidarDni(dni)) ?? throw new InvalidOperationException("No existe un socio con ese DNI");//busca el socio por dni y si no existe lanza una excepcion
        return _asistenciaDAO.ObtenerPorSocio(socio.Id);//devuelve la lista de asistencias del socio
    }
    public List<Asistencia> AsistenciasdelDia(DateTime fecha)
    {
        return _asistenciaDAO.ObtenerPorFecha(fecha.Date);//devuelve la lista de asistencias del dia
    }
    // El menú necesita el nombre del socio para mostrar la lista del día
    public Socio? ObtenerSocioPorId(int id)
    {
        return _socioDAO.ObtenerPorId(id);
    }

    public Socio ObtenerSocio(string dni)
    {
        Socio? socio = _socioDAO.ObtenerPorDni(Validaciones.ValidarDni(dni));//busca al socio por dni
        if (socio == null)
        {
            throw new InvalidOperationException("No existe un socio con ese DNI.");
        }
        return socio;
    }
}