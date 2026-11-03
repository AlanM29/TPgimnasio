public interface ISocioDAO
{
    Socio? ObtenerPorId(int id);
    Socio? ObtenerPorDni(string dni);
    List<Socio> ObtenerTodos(bool soloActivos);
    List<Socio> BuscarPorApellido(string texto);
    int Insertar(Socio socio);          // devuelve el id generado
    bool Actualizar(Socio socio);       // nombre, apellido, email y teléfono
    bool DarDeBaja(int id);
    bool Reactivar(int id);
}