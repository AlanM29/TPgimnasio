public interface IMembresiaDAO
{
    // Guarda una membresía nueva y devuelve el id que le asignó la base
    int Insertar(Membresia membresia);

    // Busca una membresía por id. Devuelve null si no existe
    // (la va a necesitar Alan para verificar que existe antes de registrar un pago)
    Membresia? ObtenerPorId(int idMembresia);

    // Todas las membresías de un socio, de la más nueva a la más vieja
    List<Membresia> ObtenerHistorialPorSocio(int idSocio);

    // La membresía que está vigente en esa fecha, o null si no tiene ninguna
    Membresia? ObtenerVigentePorSocio(int idSocio, DateTime fecha);
}