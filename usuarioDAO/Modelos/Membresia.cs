public class Membresia
{
    public int Id { get; private set; }
    public int IdSocio { get; private set; }
    public TipoMembresia Tipo { get; private set; }
    public DateTime FechaInicio { get; private set; }
    public DateTime FechaExpiracion { get; private set; }
    public decimal CuotaAbonada { get; private set; }

    // Si no se indican fechaExpiracion ni cuotaAbonada, se calculan a partir del tipo.
    // Cuando el DAO lee una membresía de la base, pasa los valores guardados.
    public Membresia(int idSocio, TipoMembresia tipo, DateTime fechaInicio,
                     DateTime? fechaExpiracion = null, decimal? cuotaAbonada = null, int id = 0)
    {
        if (idSocio <= 0)
            throw new ArgumentException("La membresía debe pertenecer a un socio válido");
        if (tipo is null)
            throw new ArgumentException("La membresía debe tener un tipo");

        IdSocio = idSocio;
        Tipo = tipo;
        FechaInicio = fechaInicio;
        FechaExpiracion = fechaExpiracion ?? fechaInicio.AddDays(tipo.DuracionDias);
        CuotaAbonada = cuotaAbonada ?? tipo.CalcularCuota();
        Id = id;

        if (FechaExpiracion <= FechaInicio)
            throw new ArgumentException("La fecha de expiración debe ser posterior a la de inicio");
        if (CuotaAbonada < 0)
            throw new ArgumentException("La cuota abonada no puede ser negativa");
    }

    public void AsignarId(int id) => Id = id;

    // true si la fecha dada está dentro del período de la membresía
    public bool EstaVigente(DateTime fecha) =>
        fecha.Date >= FechaInicio.Date && fecha.Date <= FechaExpiracion.Date;

    public int DiasRestantes(DateTime fecha) =>
        EstaVigente(fecha) ? (FechaExpiracion.Date - fecha.Date).Days : 0;
}