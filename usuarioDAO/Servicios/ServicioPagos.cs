using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

//  ServicioPagos
//  Reglas de negocio de los pagos:
//    - registrar: la membresía debe existir y no tener ya un pago 'Pagado'
//    - anular: solo con permiso
//    - exportar la recaudación: solo con permiso (PuedeExportar)

public class ServicioPagos
{
    private UsuarioBase _actual;
    private IPagoDAO _pagoDao;
    private ISocioDAO _socioDao;

    // Constructor 
    public ServicioPagos(UsuarioBase actual) : this(actual, new PagoDAO(), new SocioDAO())
    {
    }

    public ServicioPagos(UsuarioBase actual, IPagoDAO pagoDao, ISocioDAO socioDao)
    {
        _actual = actual;
        _pagoDao = pagoDao;
        _socioDao = socioDao;
    }

    // --- REGISTRAR ---
    public Pago Registrar(int idMembresia, decimal monto, string metodo)
    {
        // Primer Pago
        Pago pago = new Pago(idMembresia, monto, metodo, DateTime.Now);

        if (!_pagoDao.ExisteMembresia(idMembresia))
        {
            throw new InvalidOperationException("No existe la membresía " + idMembresia + ".");
        }
        if (_pagoDao.EstaPagada(idMembresia))
        {
            throw new InvalidOperationException("La membresía " + idMembresia + " ya tiene un pago registrado.");
        }

        _pagoDao.Registrar(pago);

        LogService.Registrar(_actual.Nombre_Usuario, "PAGO_REGISTRADO",
                             "Pago " + pago.Id + " | membresía " + idMembresia + " | " + pago.Metodo);
        return pago;
    }

    // --- CONSULTAR ---

    public Pago ObtenerPorId(int idPago)
    {
        Pago? pago = _pagoDao.ObtenerPorId(idPago);
        if (pago == null)
        {
            throw new InvalidOperationException("No existe el pago " + idPago + ".");
        }
        return pago;
    }

    public List<Pago> ConsultarPorMembresia(int idMembresia)
    {
        if (!_pagoDao.ExisteMembresia(idMembresia))
        {
            throw new InvalidOperationException("No existe la membresía " + idMembresia + ".");
        }
        return _pagoDao.ObtenerPorMembresia(idMembresia);
    }

    // Se busca al socio por DNI 
    public List<Pago> ConsultarPorSocio(string dni)
    {
        Socio? socio = _socioDao.ObtenerPorDni(Validaciones.ValidarDni(dni));
        if (socio == null)
        {
            throw new InvalidOperationException("No existe un socio con ese DNI.");
        }
        return _pagoDao.ObtenerPorSocio(socio.Id);
    }

    // --- ANULAR ---    

    public void Anular(int idPago)
    {
        if (!_actual.PuedeAnularPagos)
        {
            LogService.Registrar(_actual.Nombre_Usuario, "ACCESO_DENEGADO", "Anular pago " + idPago);
            throw new UnauthorizedAccessException("Solo un Admin puede anular pagos.");
        }

        Pago pago = ObtenerPorId(idPago);
        if (!pago.EstaPagado)
        {
            throw new InvalidOperationException("El pago " + idPago + " ya está anulado.");
        }

        _pagoDao.Anular(idPago);
        LogService.Registrar(_actual.Nombre_Usuario, "PAGO_ANULADO", "Pago " + idPago);
    }

    // --- EXPORTAR LA RECAUDACIÓN ---

    // Genera un CSV con los pagos 'Pagado' entre dos fechas y devuelve la ruta del archivo.
    public string ExportarRecaudacion(DateTime desde, DateTime hasta)
    {
        if (!_actual.PuedeExportar)
        {
            LogService.Registrar(_actual.Nombre_Usuario, "ACCESO_DENEGADO", "Exportar recaudación");
            throw new UnauthorizedAccessException("No tenés permiso para exportar la recaudación.");
        }
        if (hasta.Date < desde.Date)
        {
            throw new ArgumentException("La fecha 'hasta' no puede ser anterior a la fecha 'desde'.");
        }

        // "hasta" se incluye completo: se consulta hasta el comienzo del día siguiente
        List<Pago> pagos = _pagoDao.ObtenerPagadosEntre(desde.Date, hasta.Date.AddDays(1));
        if (pagos.Count == 0)
        {
            throw new InvalidOperationException("No hay pagos en ese período, no se generó el archivo.");
        }

        StringBuilder texto = new StringBuilder();
        texto.AppendLine("id_pago;fecha;id_membresia;metodo;monto");

        decimal total = 0;
        foreach (Pago pago in pagos)
        {
            texto.AppendLine(pago.Id + ";" +
                             pago.FechaPago.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture) + ";" +
                             pago.IdMembresia + ";" + pago.Metodo + ";" + FormatearMonto(pago.Monto));
            total += pago.Monto;
        }
        texto.AppendLine(";;;TOTAL;" + FormatearMonto(total));

        string nombre = "recaudacion_" + desde.ToString("yyyyMMdd", CultureInfo.InvariantCulture) + "_" +
                        hasta.ToString("yyyyMMdd", CultureInfo.InvariantCulture) + ".csv";
        string ruta = Path.Combine(AppContext.BaseDirectory, nombre);

        // UTF8 con BOM
        File.WriteAllText(ruta, texto.ToString(), new UTF8Encoding(true));

        LogService.Registrar(_actual.Nombre_Usuario, "RECAUDACION_EXPORTADA",
                             pagos.Count + " pagos | " + nombre);
        return ruta;
    }

    private static string FormatearMonto(decimal monto)
    {
        return monto.ToString("F2", CultureInfo.InvariantCulture).Replace('.', ',');
    }
}
