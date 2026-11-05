public interface IValidadorMembresia
{
    bool TieneMembresiaVigenteyPagada(int idSocio);

}
// Versión provisoria: acepta a todos. Se reemplaza cuando existan Membresías y Pagos.
public class ValidadorMembresiaProvisorio : IValidadorMembresia
{
    public bool TieneMembresiaVigenteyPagada(int idSocio)
    {
        return true;
    }
}