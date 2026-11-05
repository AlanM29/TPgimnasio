public class TipoMembresia
{
    public int Id { get; private set; }
    public string Tipo { get; private set; }
    public decimal PrecioBase { get; private set; }
    public int DuracionDias { get; private set; }
    public decimal RecargoPorcentaje { get; private set; }

    public TipoMembresia(string tipo, decimal precioBase, int duracionDias,
                         decimal recargoPorcentaje = 0, int id = 0)
    {
        Tipo = ValidarTipo(tipo);
        PrecioBase = ValidarPrecio(precioBase);
        DuracionDias = ValidarDuracion(duracionDias);
        RecargoPorcentaje = ValidarRecargo(recargoPorcentaje);
        Id = id;
    }

    // Calcula lo que paga el socio por este tipo de membresía
    public decimal CalcularCuota() =>
        Math.Round(PrecioBase * (1 + RecargoPorcentaje / 100m), 2);

    public void AsignarId(int id) => Id = id;

    public void CambiarPrecio(decimal nuevoPrecio) => PrecioBase = ValidarPrecio(nuevoPrecio);

    private static string ValidarTipo(string? valor)
    {
        if (string.IsNullOrWhiteSpace(valor))
            throw new ArgumentException("El tipo de membresía no puede estar vacío");
        return valor.Trim();
    }

    private static decimal ValidarPrecio(decimal valor)
    {
        if (valor <= 0)
            throw new ArgumentException("El precio debe ser mayor a 0");
        return valor;
    }

    private static int ValidarDuracion(int valor)
    {
        if (valor <= 0)
            throw new ArgumentException("La duración debe ser mayor a 0 días");
        return valor;
    }

    private static decimal ValidarRecargo(decimal valor)
    {
        if (valor < 0 || valor > 100)
            throw new ArgumentException("El recargo debe estar entre 0 y 100");
        return valor;
    }
}