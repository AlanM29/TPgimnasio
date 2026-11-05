using System;
using System.Globalization;

//  MODELO: Pago
//  Un pago de una membresía. Valida sus datos en el constructor (monto mayor a 0, método válido).
//  Un pago no se borra: se ANULA (cambia de estado).

public static class MetodosPago
{
    public const string Efectivo = "Efectivo";
    public const string Tarjeta = "Tarjeta";
    public const string Transferencia = "Transferencia";

    // Son los mismos valores que acepta el CHECK de la tabla Pago
    public static readonly string[] Validos = { Efectivo, Tarjeta, Transferencia };

    // Devuelve el método con su escritura oficial ("efectivo" -> "Efectivo") o lanza ArgumentException
    public static string Validar(string? metodo)
    {
        string texto = Validaciones.ValidarObligatorio(metodo, "El método de pago");

        foreach (string valido in Validos)
        {
            if (string.Equals(valido, texto, StringComparison.OrdinalIgnoreCase))
            {
                return valido;
            }
        }
        throw new ArgumentException("Método de pago inválido. Opciones: " + string.Join(", ", Validos) + ".");
    }
}

public static class EstadosPago
{
    public const string Pagado = "Pagado";
    public const string Anulado = "Anulado";
}

public class Pago
{
    public int Id { get; private set; }
    public int IdMembresia { get; private set; }
    public DateTime FechaPago { get; private set; }
    public decimal Monto { get; private set; }
    public string Metodo { get; private set; }
    public string Estado { get; private set; }

    public bool EstaPagado => Estado == EstadosPago.Pagado;

    public Pago(int idMembresia, decimal monto, string metodo, DateTime fechaPago,
                string estado = EstadosPago.Pagado, int id = 0)
    {
        if (idMembresia <= 0)
        {
            throw new ArgumentException("La membresía del pago no es válida.");
        }

        decimal montoRedondeado = Math.Round(monto, 2);
        if (montoRedondeado <= 0)
        {
            throw new ArgumentException("El monto debe ser mayor a 0.");
        }

        if (estado != EstadosPago.Pagado && estado != EstadosPago.Anulado)
        {
            throw new ArgumentException("Estado de pago inválido: '" + estado + "'.");
        }

        IdMembresia = idMembresia;
        Monto = montoRedondeado;
        Metodo = MetodosPago.Validar(metodo);
        FechaPago = fechaPago;
        Estado = estado;
        Id = id;
    }

    // El DAO lo llama después de insertar, con el id que generó la base
    public void AsignarId(int id) => Id = id;

    public void Anular()
    {
        if (Estado == EstadosPago.Anulado)
        {
            throw new InvalidOperationException("El pago ya está anulado.");
        }
        Estado = EstadosPago.Anulado;
    }

    public override string ToString()
    {
        return "[" + Id + "] " + FechaPago.ToString("dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture) +
               " | Membresía " + IdMembresia + " | " + Metodo +
               " | $" + Monto.ToString("F2", CultureInfo.InvariantCulture) + " | " + Estado;
    }
}
