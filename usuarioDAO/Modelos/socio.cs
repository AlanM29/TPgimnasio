public class Socio : Persona
{
 
    //  Es una Persona inscripta en el gimnasio. Todo lo común lo hereda de Persona.

    public Socio(string dni, string nombre, string apellido, string? email,
                  string? telefono, DateTime fechaRegistro, bool activo = true, int id = 0)
         : base(dni, nombre, apellido, email, telefono, fechaRegistro, activo, id)
    { }
    public override string ToString()
    {
        return "[" + Id + "] " + Apellido + ", " + Nombre + " | DNI " + Dni +
               " | " + (Activo ? "Activo" : "Inactivo");
    }

}