using System.Data.SQLite;

//  DAO: MembresiaDAO
//  Es la ÚNICA clase que consulta la tabla Membresia.
//  Cada consulta une con Tipo_Membresias (JOIN) para armar el objeto completo.

public class MembresiaDAO : DaoBase, IMembresiaDAO
{
    // Textos fijos escritos en este archivo (nunca vienen del usuario)
    private const string Columnas =
        "m.id_membresia, m.id_socio, m.fecha_inicio, m.fecha_expiracion, m.cuota_abonada, " +
        "t.id_tipo, t.tipo, t.precio_base, t.duracion_dias, t.recargo_porcentaje";

    private const string Desde =
        " FROM Membresia m INNER JOIN Tipo_Membresias t ON t.id_tipo = m.id_tipo ";

    public int Insertar(Membresia membresia)
    {
        try
        {
            int id = (int)EjecutarInsert(
                "INSERT INTO Membresia (id_socio, id_tipo, fecha_inicio, fecha_expiracion, cuota_abonada) " +
                "VALUES (@socio, @tipo, @inicio, @fin, @cuota)",
                ("@socio", membresia.IdSocio),
                ("@tipo", membresia.Tipo.Id),
                ("@inicio", FechaATexto(membresia.FechaInicio)),
                ("@fin", FechaATexto(membresia.FechaExpiracion)),
                ("@cuota", (double)membresia.CuotaAbonada));

            membresia.AsignarId(id);
            return id;
        }
        catch (SQLiteException ex) when (ex.Message.Contains("FOREIGN KEY constraint failed"))
        {
            throw new InvalidOperationException("El socio o el tipo de membresía no existe.", ex);
        }
    }

    public Membresia? ObtenerPorId(int idMembresia)
    {
        return ConsultarUno<Membresia>(
            "SELECT " + Columnas + Desde + "WHERE m.id_membresia = @id",
            Mapear, ("@id", idMembresia));
    }

    public List<Membresia> ObtenerHistorialPorSocio(int idSocio)
    {
        return ConsultarLista<Membresia>(
            "SELECT " + Columnas + Desde + "WHERE m.id_socio = @id " +
            "ORDER BY m.fecha_inicio DESC, m.id_membresia DESC",
            Mapear, ("@id", idSocio));
    }

    // date() compara solo la fecha (sin la hora), igual que Membresia.EstaVigente
    public Membresia? ObtenerVigentePorSocio(int idSocio, DateTime fecha)
    {
        return ConsultarUno<Membresia>(
            "SELECT " + Columnas + Desde + "WHERE m.id_socio = @socio " +
            "  AND date(m.fecha_inicio) <= date(@fecha) " +
            "  AND date(m.fecha_expiracion) >= date(@fecha) " +
            "ORDER BY m.fecha_expiracion DESC LIMIT 1",
            Mapear, ("@socio", idSocio), ("@fecha", FechaATexto(fecha)));
    }

    // Convierte una fila (membresía + su tipo) en un objeto Membresia
    private Membresia Mapear(SQLiteDataReader lector)
    {
        TipoMembresia tipo = new TipoMembresia(
            tipo: lector.GetString(6),
            precioBase: Convert.ToDecimal(lector.GetDouble(7)),
            duracionDias: lector.GetInt32(8),
            recargoPorcentaje: Convert.ToDecimal(lector.GetDouble(9)),
            id: lector.GetInt32(5));

        return new Membresia(
            idSocio: lector.GetInt32(1),
            tipo: tipo,
            fechaInicio: TextoAFecha(lector.GetString(2)),
            fechaExpiracion: lector.IsDBNull(3) ? null : TextoAFecha(lector.GetString(3)),
            cuotaAbonada: Convert.ToDecimal(lector.GetDouble(4)),
            id: lector.GetInt32(0));
    }
}