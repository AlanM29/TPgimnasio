using System;
using System.Collections.Generic;
using System.Data.SQLite;

//  DAO: PagoDAO
//  Es la ÚNICA clase que consulta la tabla Pago.

public class PagoDAO : DaoBase, IPagoDAO
{
    // Texto fijo escrito en este archivo (nunca viene del usuario), por eso se puede concatenar
    private const string Columnas = "p.id_pago, p.id_membresia, p.fecha_pago, p.monto, p.metodo_pago, p.estado";

    public int Registrar(Pago pago)
    {
        string sql = "INSERT INTO Pago (id_membresia, fecha_pago, monto, metodo_pago, estado) " +
                     "VALUES (@membresia, @fecha, @monto, @metodo, @estado)";

        int id = (int)EjecutarInsert(sql,
            ("@membresia", pago.IdMembresia),
            ("@fecha", FechaATexto(pago.FechaPago)),
            ("@monto", (double)pago.Monto),
            ("@metodo", pago.Metodo),
            ("@estado", pago.Estado));

        pago.AsignarId(id);
        return id;
    }

    public Pago? ObtenerPorId(int idPago)
    {
        return ConsultarUno<Pago>("SELECT " + Columnas + " FROM Pago p WHERE p.id_pago = @id",
                                  Mapear, ("@id", idPago));
    }

    public List<Pago> ObtenerPorMembresia(int idMembresia)
    {
        return ConsultarLista<Pago>("SELECT " + Columnas + " FROM Pago p " +
                                    "WHERE p.id_membresia = @id " +
                                    "ORDER BY p.fecha_pago DESC, p.id_pago DESC",
                                    Mapear, ("@id", idMembresia));
    }

    // Pago no tiene id_socio: se llega al socio a través de la membresía
    public List<Pago> ObtenerPorSocio(int idSocio)
    {
        return ConsultarLista<Pago>("SELECT " + Columnas + " FROM Pago p " +
                                    "INNER JOIN Membresia m ON m.id_membresia = p.id_membresia " +
                                    "WHERE m.id_socio = @id " +
                                    "ORDER BY p.fecha_pago DESC, p.id_pago DESC",
                                    Mapear, ("@id", idSocio));
    }

    // Las fechas son texto "yyyy-MM-dd HH:mm:ss", así que comparar texto es comparar fechas
    public List<Pago> ObtenerPagadosEntre(DateTime desde, DateTime hasta)
    {
        return ConsultarLista<Pago>("SELECT " + Columnas + " FROM Pago p " +
                                    "WHERE p.estado = 'Pagado' " +
                                    "  AND p.fecha_pago >= @desde AND p.fecha_pago < @hasta " +
                                    "ORDER BY p.fecha_pago, p.id_pago",
                                    Mapear,
                                    ("@desde", FechaATexto(desde)),
                                    ("@hasta", FechaATexto(hasta)));
    }

    public bool Anular(int idPago)
    {
        return EjecutarNoQuery("UPDATE Pago SET estado = 'Anulado' WHERE id_pago = @id AND estado = 'Pagado'",
                               ("@id", idPago)) > 0;
    }

    public bool EstaPagada(int idMembresia)
    {
        return Existe("SELECT 1 FROM Pago WHERE id_membresia = @id AND estado = 'Pagado' LIMIT 1",
                      ("@id", idMembresia));
    }

    public bool ExisteMembresia(int idMembresia)
    {
        return Existe("SELECT 1 FROM Membresia WHERE id_membresia = @id LIMIT 1", ("@id", idMembresia));
    }

    private Pago Mapear(SQLiteDataReader lector)
    {
        return new Pago(
            idMembresia: lector.GetInt32(1),
            monto: Convert.ToDecimal(lector.GetDouble(3)),
            metodo: lector.GetString(4),
            fechaPago: TextoAFecha(lector.GetString(2)),
            estado: lector.GetString(5),
            id: lector.GetInt32(0));
    }
}
