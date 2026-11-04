public interface ITipoMembresiaDAO
{
    // Devuelve todos los tipos de membresía (para mostrar las tarifas)
    List<TipoMembresia> ObtenerTodos();

    // Busca un tipo por su id. Devuelve null si no existe
    TipoMembresia? ObtenerPorId(int id);

    // Guarda un tipo nuevo (lo usa InicializadorBd para cargar los premium)
    // Devuelve el id que le asignó la base
    int Insertar(TipoMembresia tipo);

    // Cambia el precio base de un tipo existente
    void ModificarPrecio(int idTipo, decimal nuevoPrecio);
}