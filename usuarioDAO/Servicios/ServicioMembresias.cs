public class ServicioMembresias
{
    private readonly ITipoMembresiaDAO _tipoDao;
    private readonly IMembresiaDAO _membresiaDao;

    // Recibe las interfaces, no las clases concretas: el servicio no sabe nada de SQLite
    public ServicioMembresias(ITipoMembresiaDAO tipoDao, IMembresiaDAO membresiaDao)
    {
        _tipoDao = tipoDao;
        _membresiaDao = membresiaDao;
    }

    public List<TipoMembresia> ObtenerTipos() => _tipoDao.ObtenerTodos();

    // Solo puede modificar precios un usuario con el permiso correspondiente
    public void ModificarPrecio(UsuarioBase usuario, int idTipo, decimal nuevoPrecio)
    {
        if (!usuario.PuedeModificarPrecios)
            throw new UnauthorizedAccessException("No tenés permiso para modificar precios");

        TipoMembresia tipo = ObtenerTipoOFallar(idTipo);
        tipo.CambiarPrecio(nuevoPrecio);   // valida que el precio sea mayor a 0
        _tipoDao.ModificarPrecio(idTipo, tipo.PrecioBase);
    }

    public Membresia Contratar(int idSocio, int idTipo, DateTime fechaInicio)
    {
        TipoMembresia tipo = ObtenerTipoOFallar(idTipo);

        if (_membresiaDao.ObtenerVigentePorSocio(idSocio, fechaInicio) != null)
            throw new InvalidOperationException("El socio ya tiene una membresía vigente");

        var membresia = new Membresia(idSocio, tipo, fechaInicio);
        membresia.AsignarId(_membresiaDao.Insertar(membresia));
        return membresia;
    }

    public Membresia? ObtenerVigente(int idSocio, DateTime fecha) =>
        _membresiaDao.ObtenerVigentePorSocio(idSocio, fecha);

    public List<Membresia> ObtenerHistorial(int idSocio) =>
        _membresiaDao.ObtenerHistorialPorSocio(idSocio);

    private TipoMembresia ObtenerTipoOFallar(int idTipo) =>
        _tipoDao.ObtenerPorId(idTipo)
        ?? throw new ArgumentException("No existe un tipo de membresía con ese id");
}