using System.Data.SQLite;

//  DAO: TipoMembresiaDAO
//  Es la ÚNICA clase que consulta la tabla Tipo_Membresias.

public class TipoMembresiaDAO : DaoBase, ITipoMembresiaDAO
{
    // Texto fijo escrito en este archivo (nunca viene del usuario), por eso se puede concatenar
    private const string Columnas = "id_tipo, tipo, precio_base, duracion_dias, recargo_porcentaje";

    public List<TipoMembresia> ObtenerTodos()
    {
        return ConsultarLista<TipoMembresia>(
            "SELECT " + Columnas + " FROM Tipo_Membresias ORDER BY id_tipo", Mapear);
    }

    public TipoMembresia? ObtenerPorId(int id)
    {
        return ConsultarUno<TipoMembresia>(
            "SELECT " + Columnas + " FROM Tipo_Membresias WHERE id_tipo = @id",
            Mapear, ("@id", id));
    }

    public int Insertar(TipoMembresia tipo)
    {
        try
        {
            int id = (int)EjecutarInsert(
                "INSERT INTO Tipo_Membresias (tipo, precio_base, duracion_dias, recargo_porcentaje) " +
                "VALUES (@tipo, @precio, @dias, @recargo)",
                ("@tipo", tipo.Tipo),
                ("@precio", (double)tipo.PrecioBase),
                ("@dias", tipo.DuracionDias),
                ("@recargo", (double)tipo.RecargoPorcentaje));

            tipo.AsignarId(id);
            return id;
        }
        catch (SQLiteException ex) when (EsDuplicado(ex))
        {
            throw new InvalidOperationException("Ya existe un tipo de membresía con ese nombre.", ex);
        }
    }

    public void ModificarPrecio(int idTipo, decimal nuevoPrecio)
    {
        int filas = EjecutarNoQuery(
            "UPDATE Tipo_Membresias SET precio_base = @precio WHERE id_tipo = @id",
            ("@precio", (double)nuevoPrecio), ("@id", idTipo));

        if (filas == 0)
        {
            throw new ArgumentException("No existe un tipo de membresía con ese id");
        }
    }

    // Convierte una fila en un objeto TipoMembresia (usa sus validaciones)
    private TipoMembresia Mapear(SQLiteDataReader lector)
    {
        return new TipoMembresia(
            tipo: lector.GetString(1),
            precioBase: Convert.ToDecimal(lector.GetDouble(2)),
            duracionDias: lector.GetInt32(3),
            recargoPorcentaje: Convert.ToDecimal(lector.GetDouble(4)),
            id: lector.GetInt32(0));
    }
}