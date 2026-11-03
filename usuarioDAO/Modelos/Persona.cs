using System.Net;

public abstract class Persona 
{
    public int Id { get; private set; }
    public string Nombre { get; private set; }
    public string Apellido { get; private set; }
    public string Dni { get; private set; }
    public string? Email { get; private set; }
    public string? Telefono { get; private set; }
    public DateTime FechaRegistro { get; private set; }
    public bool Activo { get; private set; }
    protected Persona(string dni, string nombre, string apellido, string? email,
                      string? telefono, DateTime fechaRegistro, bool activo = true, int id = 0)
    {
        Dni = Validaciones.ValidarDni(dni);
        Nombre = Validaciones.ValidarObligatorio(nombre, "El nombre");
        Apellido = Validaciones.ValidarObligatorio(apellido, "El apellido");
        Email = Validaciones.ValidarEmail(email);
        Telefono = Validaciones.ValidarTelefono(telefono);
        FechaRegistro = fechaRegistro;
        Activo = activo;
        Id = id;
    }
    // El DAO lo llama después de insertar, con el id que generó la base
    public void AsignarId(int id) => Id = id;
    public void ActualizarContacto(string nombre, string apellido, string? email, string? telefono)// actualiza los datos del usuario; el dni no se modifica
    {
        Nombre = Validaciones.ValidarObligatorio(nombre, "El nombre");
        Apellido = Validaciones.ValidarObligatorio(apellido, "El apellido");
        Email = Validaciones.ValidarEmail(email);
        Telefono = Validaciones.ValidarTelefono(telefono);
    }
    // no se borra, solo cambia de estado
    public void DarDeBaja() => Activo = false;
    public void Reactivar() => Activo = true;
}