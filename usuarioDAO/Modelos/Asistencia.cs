public class Asistencia
{
    public int Id { get; private set; }
    public int Id_Socio { get; private set; }
    public DateTime FechaHora { get; private set;}
    public Asistencia(int id_socio, DateTime fechaHora, int id = 0)
    {
        if(id_socio <= 0)
        {
            throw new ArgumentException("El id del socio debe ser valido");
        }
        if(fechaHora> DateTime.Now.AddMinutes(1))
        {
            throw new ArgumentException("La fecha y hora no puede ser futura");
        }
        Id = id;
        Id_Socio = id_socio;
        FechaHora = fechaHora;
    }
    public void AsignarId(int id) => Id = id;

    public override string ToString()
    {
        return "[" + Id + "] Socio " + Id_Socio + " | " + FechaHora.ToString("dd/MM/yyyy HH:mm");
    }
}
