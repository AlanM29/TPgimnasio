using System;
using System.Collections.Generic;

//  INTERFAZ: MenuPagos
//  Registrar, consultar y anular pagos, y exportar la recaudación.
//  Anular y exportar solo se muestran si el usuario tiene el permiso (el servicio lo controla igual).

public class MenuPagos
{
    private UsuarioBase _usuario;
    private ServicioPagos _servicio;

    public MenuPagos(UsuarioBase usuario) : this(usuario, new ServicioPagos(usuario))
    {
    }

    public MenuPagos(UsuarioBase usuario, ServicioPagos servicio)
    {
        _usuario = usuario;
        _servicio = servicio;
    }

    public void Mostrar()
    {
        int opcion;
        do
        {
            Console.WriteLine("===== PAGOS =====");
            Console.WriteLine("1. Registrar pago");
            Console.WriteLine("2. Consultar pagos de una membresía");
            Console.WriteLine("3. Consultar pagos de un socio (por DNI)");
            if (_usuario.PuedeAnularPagos)
            {
                Console.WriteLine("4. Anular pago");
            }
            if (_usuario.PuedeExportar)
            {
                Console.WriteLine("5. Exportar recaudación (CSV)");
            }
            Console.WriteLine("0. Volver");
            opcion = EntradaConsola.LeerEntero("Opción: ", 0, 5);
            Console.WriteLine();

            switch (opcion)
            {
                case 1:
                    EntradaConsola.Intentar(Registrar);
                    break;
                case 2:
                    EntradaConsola.Intentar(ConsultarPorMembresia);
                    break;
                case 3:
                    EntradaConsola.Intentar(ConsultarPorSocio);
                    break;
                case 4:
                    EntradaConsola.Intentar(Anular);
                    break;
                case 5:
                    EntradaConsola.Intentar(Exportar);
                    break;
            }

            Console.WriteLine();
        } while (opcion != 0);
    }

    private void Registrar()
    {
        Console.WriteLine("--- REGISTRAR PAGO ---");
        int idMembresia = EntradaConsola.LeerEntero("Id de la membresía: ", 1, int.MaxValue);
        decimal monto = EntradaConsola.LeerMonto("Monto: ");

        Console.WriteLine("Método de pago:");
        for (int i = 0; i < MetodosPago.Validos.Length; i++)
        {
            Console.WriteLine((i + 1) + ". " + MetodosPago.Validos[i]);
        }
        int eleccion = EntradaConsola.LeerEntero("Opción: ", 1, MetodosPago.Validos.Length);
        string metodo = MetodosPago.Validos[eleccion - 1];

        Pago pago = _servicio.Registrar(idMembresia, monto, metodo);
        Console.WriteLine("Pago registrado: " + pago);
    }

    private void ConsultarPorMembresia()
    {
        int idMembresia = EntradaConsola.LeerEntero("Id de la membresía: ", 1, int.MaxValue);
        MostrarLista(_servicio.ConsultarPorMembresia(idMembresia));
    }

    private void ConsultarPorSocio()
    {
        string dni = EntradaConsola.LeerTextoObligatorio("DNI del socio: ");
        MostrarLista(_servicio.ConsultarPorSocio(dni));
    }

    private void Anular()
    {
        int idPago = EntradaConsola.LeerEntero("Id del pago: ", 1, int.MaxValue);
        Pago pago = _servicio.ObtenerPorId(idPago);
        Console.WriteLine(pago);

        if (!EntradaConsola.Confirmar("¿Anular este pago?"))
        {
            Console.WriteLine("Operación cancelada.");
            return;
        }

        _servicio.Anular(idPago);
        Console.WriteLine("Pago anulado.");
    }

    private void Exportar()
    {
        Console.WriteLine("--- EXPORTAR RECAUDACIÓN ---");
        DateTime desde = EntradaConsola.LeerFecha("Desde (dd/MM/aaaa): ");
        DateTime hasta = EntradaConsola.LeerFecha("Hasta (dd/MM/aaaa): ");

        string ruta = _servicio.ExportarRecaudacion(desde, hasta);
        Console.WriteLine("Archivo generado: " + ruta);
    }

    private void MostrarLista(List<Pago> pagos)
    {
        if (pagos.Count == 0)
        {
            Console.WriteLine("No hay pagos para mostrar.");
            return;
        }

        foreach (Pago pago in pagos)
        {
            Console.WriteLine(pago);
        }
    }
}
