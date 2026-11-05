public interface IPagoDAO
{
    // Guarda el pago y le asigna el id generado.
    int Registrar(Pago pago);

    Pago? ObtenerPorId(int idPago);

    // Pagos de una membresía (el más nuevo primero)
    List<Pago> ObtenerPorMembresia(int idMembresia);

    // Pagos de todas las membresías de un socio (el más nuevo primero)
    List<Pago> ObtenerPorSocio(int idSocio);

    // Pagos en estado 'Pagado' entre dos fechas (desde incluida, hasta EXCLUIDA)
    List<Pago> ObtenerPagadosEntre(DateTime desde, DateTime hasta);

    // Cambia el estado a 'Anulado'. Devuelve false si no existe o ya estaba anulado.
    bool Anular(int idPago);

    // true si la membresía tiene un pago en estado 'Pagado'
    bool EstaPagada(int idMembresia);

    // true si existe la membresía (la tabla Membresia la maneja otro módulo; acá solo se consulta)
    bool ExisteMembresia(int idMembresia);
}
