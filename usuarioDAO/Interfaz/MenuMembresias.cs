public class MenuMembresias
{
    private readonly ServicioMembresias _servicio;
    private readonly UsuarioBase _usuario;

    public MenuMembresias(ServicioMembresias servicio, UsuarioBase usuarioActual)
    {
        _servicio = servicio;
        _usuario = usuarioActual;
    }

    public void Mostrar()
    {
        int opcion;
        do
        {
            Console.WriteLine("\n=== MEMBRESÍAS ===");
            Console.WriteLine("1. Contratar membresía");
            Console.WriteLine("2. Ver estado de un socio");
            Console.WriteLine("3. Ver tarifas");
            if (_usuario.PuedeModificarPrecios)
                Console.WriteLine("4. Modificar precio de un tipo");
            Console.WriteLine("0. Volver");
            opcion = LeerEntero("Opción: ");

            try
            {
                switch (opcion)
                {
                    case 1: Contratar(); break;
                    case 2: VerEstado(); break;
                    case 3: VerTarifas(); break;
                    case 4: ModificarPrecio(); break;
                    case 0: break;
                    default: Console.WriteLine("Opción inválida"); break;
                }
            }
            catch (Exception ex) when (ex is ArgumentException
                                    || ex is InvalidOperationException
                                    || ex is UnauthorizedAccessException)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        } while (opcion != 0);
    }

    private void VerTarifas()
    {
        Console.WriteLine("\nTarifas:");
        foreach (TipoMembresia t in _servicio.ObtenerTipos())
            Console.WriteLine($"{t.Id}. {t.Tipo} | {t.DuracionDias} días | Cuota: ${t.CalcularCuota():N2}");
    }

    private void Contratar()
    {
        VerTarifas();
        int idSocio = LeerEntero("Id del socio: ");
        int idTipo = LeerEntero("Id del tipo de membresía: ");
        Membresia m = _servicio.Contratar(idSocio, idTipo, DateTime.Today);
        Console.WriteLine($"Membresía contratada. Vence el {m.FechaExpiracion:dd/MM/yyyy}. Cuota: ${m.CuotaAbonada:N2}");
    }

    private void VerEstado()
    {
        int idSocio = LeerEntero("Id del socio: ");
        Membresia? vigente = _servicio.ObtenerVigente(idSocio, DateTime.Today);

        if (vigente == null)
            Console.WriteLine("El socio no tiene una membresía vigente.");
        else
            Console.WriteLine($"Vigente: {vigente.Tipo.Tipo} | vence el {vigente.FechaExpiracion:dd/MM/yyyy} " +
                              $"({vigente.DiasRestantes(DateTime.Today)} días restantes)");

        Console.WriteLine("Historial:");
        foreach (Membresia m in _servicio.ObtenerHistorial(idSocio))
            Console.WriteLine($"- {m.Tipo.Tipo}: {m.FechaInicio:dd/MM/yyyy} a {m.FechaExpiracion:dd/MM/yyyy} | ${m.CuotaAbonada:N2}");
    }

    private void ModificarPrecio()
    {
        VerTarifas();
        int idTipo = LeerEntero("Id del tipo a modificar: ");
        decimal precio = LeerDecimal("Nuevo precio base: ");
        _servicio.ModificarPrecio(_usuario, idTipo, precio);
        Console.WriteLine("Precio actualizado.");
    }

    // Repite la pregunta hasta que el usuario escriba un número válido
    private static int LeerEntero(string mensaje)
    {
        int valor;
        Console.Write(mensaje);
        while (!int.TryParse(Console.ReadLine(), out valor))
            Console.Write("Ingresá un número entero: ");
        return valor;
    }

    private static decimal LeerDecimal(string mensaje)
    {
        decimal valor;
        Console.Write(mensaje);
        while (!decimal.TryParse(Console.ReadLine(), out valor))
            Console.Write("Ingresá un número válido: ");
        return valor;
    }
}