public interface IAsistenciaDAO
{
    int Insertar(Asistencia asistencia);                       // devuelve el id generado
    List<Asistencia> ObtenerPorSocio(int idSocio);             // las más recientes primero
    List<Asistencia> ObtenerPorFecha(DateTime fecha);          // todas las de un día
    bool ExisteAsistenciaEnFecha(int idSocio, DateTime fecha); // ¿ya vino ese día?
}