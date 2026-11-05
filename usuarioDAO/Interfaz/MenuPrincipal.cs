using System;
using System.Collections.Generic;


//  MenuPrincipal
//  Une todo: el ROL del usuario y la LICENCIA del sistema
public class MenuPrincipal
{
    private UsuarioBase _usuario;
    private Licencia _licencia;

    public MenuPrincipal(UsuarioBase usuario, Licencia licencia)
    {
        _usuario = usuario;
        _licencia = licencia;
    }

    public void Mostrar()
    {
        int opcion;

        do
        {
            MostrarOpciones();
            opcion = LeerNumero("Opción: ", 0, 7);
            Console.WriteLine();

            switch (opcion)
            {
                case 1:
                    AbrirModulo(Modulos.Socios, "Socios");
                    break;
                case 2:
                    AbrirModulo(Modulos.Membresias, "Membresías", () => new MenuMembresias(new ServicioMembresias(new TipoMembresiaDAO(), new MembresiaDAO()), _usuario).Mostrar());
                    break;
                case 3:
                    AbrirModulo(Modulos.Pagos, "Pagos", () => new MenuPagos(_usuario).Mostrar());
                    break;
                case 4:
                    AbrirModulo(Modulos.Asistencias, "Asistencias");
                    break;
                case 5:
                    VerRegistro();
                    break;
                case 6:
                    Console.WriteLine(_licencia);
                    break;
                case 7:
                    AbrirUsuarios();
                    break;
            }

            Console.WriteLine();
        } while (opcion != 0);

        LogService.Registrar(_usuario.Nombre_Usuario, "LOGOUT", "Cierre de sesión");
        Console.WriteLine("Sesión cerrada. ¡Hasta luego!");
    }

    private void MostrarOpciones()
    {
        Console.WriteLine("===== MENÚ PRINCIPAL =====");
        Console.WriteLine("1. Socios" + Marca(Modulos.Socios));
        Console.WriteLine("2. Membresías" + Marca(Modulos.Membresias));
        Console.WriteLine("3. Pagos" + Marca(Modulos.Pagos));
        Console.WriteLine("4. Asistencias" + Marca(Modulos.Asistencias));

        // Solo el admin ve esta opción 
        if (_usuario.PuedeGestionarUsuarios)
        {
            Console.WriteLine("5. Ver registro de actividad");
        }

        Console.WriteLine("6. Estado de la licencia");

        // Solo el admin ve esta opción 
        if (_usuario.PuedeGestionarUsuarios)
        {
            Console.WriteLine("7. Gestión de usuarios");
        }
        Console.WriteLine("0. Salir");
    }

    // Si la licencia no incluye el módulo, se marca en el menú
    private string Marca(string modulo)
    {
        if (_licencia.PermiteModulo(modulo))
        {
            return "";
        }
        return "  (no incluido en la licencia)";
    }

    // Control REAL de la licencia: si no incluye el módulo, no se abre
    // "abrir" es el menú real del módulo. Si todavía no está conectado, queda null y se muestra el aviso.
    // Cuando estén los menús de Socios, Membresías y Asistencias se pasan igual que el de Pagos
    private void AbrirModulo(string modulo, string nombre, Action? abrir = null)
    {
        if (!_licencia.PermiteModulo(modulo))
        {
            Console.WriteLine("El módulo " + nombre + " no está incluido en tu licencia (plan " + _licencia.Plan + ").");
            LogService.Registrar(_usuario.Nombre_Usuario, "MODULO_BLOQUEADO", nombre);
            return;
        }

        LogService.Registrar(_usuario.Nombre_Usuario, "MODULO_ABIERTO", nombre);

        if (modulo == Modulos.Socios)
        {
            new MenuSocios(new ServicioSocio(new SocioDAO()), _usuario).MostrarMenu();
            return;
        }
        if (modulo == Modulos.Asistencias)
        {
            ServicioAsistencias servicio = new ServicioAsistencias(
                new AsistenciaDAO(), new ValidadorMembresiaProvisorio(), new SocioDAO(), _licencia);
            new MenuAsistencias(servicio, _usuario).MostrarMenu();
            return;
        }

        Console.WriteLine("[Módulo " + nombre + "] (acá se conecta el módulo real)");

    }

    // Control: solo el admin puede ver el registro
    private void VerRegistro()
    {
        if (!_usuario.PuedeGestionarUsuarios)
        {
            Console.WriteLine("No tenés permiso para ver el registro de actividad.");
            LogService.Registrar(_usuario.Nombre_Usuario, "ACCESO_DENEGADO", "Registro de actividad");
            return;
        }

        Console.WriteLine("--- ÚLTIMOS 15 MOVIMIENTOS ---");
        List<string> lineas = LogService.ObtenerUltimos(15);

        if (lineas.Count == 0)
        {
            Console.WriteLine("El registro está vacío.");
            return;
        }

        foreach (string linea in lineas)
        {
            Console.WriteLine(linea);
        }
    }

    private int LeerNumero(string mensaje, int minimo, int maximo)
    {
        int numero;
        Console.Write(mensaje);
        while (!int.TryParse(Console.ReadLine(), out numero) || numero < minimo || numero > maximo)
        {
            Console.Write("Ingresá un número entre " + minimo + " y " + maximo + ": ");
        }
        return numero;
    }
}
