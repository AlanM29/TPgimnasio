using System.Data.SQLite;

public class SocioDAO : DaoBase, ISocioDAO
{
    // Texto fijo escrito en este archivo (nunca viene del usuario), por eso se puede concatenar
    private const string Columnas = "id_socio, dni, nombre, apellido, email, telefono, estado, fecha_alta";

    public Socio? ObtenerPorId(int id)
    {
        return ConsultarUno<Socio>("SELECT " + Columnas + " FROM Socios WHERE id_socio = @valor",
                                   Mapear, ("@valor", id));
    }

    public Socio? ObtenerPorDni(string dni)//hace lo mismo que por id, pero con el dni
    {
        return ConsultarUno<Socio>("SELECT " + Columnas + " FROM Socios WHERE dni = @valor",
                                   Mapear, ("@valor", dni));
    }

    public List<Socio> ObtenerTodos(bool soloActivos)
    {
        string condicion = soloActivos ? "WHERE estado = 'Activo' " : "";
        return ConsultarLista<Socio>("SELECT " + Columnas + " FROM Socios " + condicion +
                                     "ORDER BY nombre, apellido", Mapear);
    }

    public List<Socio> BuscarPorApellido(string texto)//busca por apellido, devuelve lista vacia si no encuentra
    {
        return ConsultarLista<Socio>("SELECT " + Columnas + " FROM Socios " +
                                     "WHERE apellido LIKE @valor ORDER BY apellido, nombre",
                                     Mapear, ("@valor", "%" + texto + "%"));// con comodines
    }

    public int Insertar(Socio socio)
    {
        try
        {
            string sql = "INSERT INTO Socios (dni, nombre, apellido, email, telefono, estado, fecha_alta) " +
                         "VALUES (@dni, @nombre, @apellido, @email, @telefono, @estado, @fecha)";

            int id = (int)EjecutarInsert(sql,
                ("@dni", socio.Dni),
                ("@nombre", socio.Nombre),
                ("@apellido", socio.Apellido),
                ("@email", socio.Email),        // si es null, se guarda NULL
                ("@telefono", socio.Telefono),  // si es null, se guarda NULL
                ("@estado", socio.Activo ? "Activo" : "Inactivo"),
                ("@fecha", FechaATexto(socio.FechaRegistro)));

            socio.AsignarId(id);
            return id;
        }
        catch (SQLiteException ex) when (ex.Message.Contains("UNIQUE constraint failed"))
        {
            if (ex.Message.Contains("Socios.dni"))
            {
                throw new InvalidOperationException("Ya existe un socio con ese DNI.", ex);
            }
            if (ex.Message.Contains("Socios.email"))
            {
                throw new InvalidOperationException("Ya existe un socio con ese email.", ex);
            }
            throw new InvalidOperationException("Ya existe un socio con esos datos.", ex);
        }
    }

    public bool Actualizar(Socio socio)
    {
        try
        {
            string sql = "UPDATE Socios SET nombre = @nombre, apellido = @apellido, " +
                         "email = @email, telefono = @telefono WHERE id_socio = @id";

            return EjecutarNoQuery(sql,
                ("@nombre", socio.Nombre),
                ("@apellido", socio.Apellido),
                ("@email", socio.Email),
                ("@telefono", socio.Telefono),
                ("@id", socio.Id)) > 0;
        }
        catch (SQLiteException ex) when (EsDuplicado(ex))
        {
            throw new InvalidOperationException("Ya existe otro socio con ese email.", ex);
        }
    }

    public bool DarDeBaja(int id)
    {
        return CambiarEstado(id, "Inactivo");
    }

    public bool Reactivar(int id)
    {
        return CambiarEstado(id, "Activo");
    }

    private bool CambiarEstado(int id, string estado)
    {
        return EjecutarNoQuery("UPDATE Socios SET estado = @estado WHERE id_socio = @id",
                               ("@estado", estado), ("@id", id)) > 0;
    }

    //convierte la fila en un objeto socio, usando su constructor y validaciones
    private Socio Mapear(SQLiteDataReader lector)
    {
        return new Socio(
            dni: lector.GetString(1),
            nombre: lector.GetString(2),
            apellido: lector.GetString(3),
            email: lector.IsDBNull(4) ? null : lector.GetString(4),
            telefono: lector.IsDBNull(5) ? null : lector.GetString(5),
            fechaRegistro: TextoAFecha(lector.GetString(7)),
            activo: lector.GetString(6) == "Activo",
            id: lector.GetInt32(0));
    }//GetString() lanza excepción si el valor es null, por eso se usa IsDBNull() antes de leer
}
