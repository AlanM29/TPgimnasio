using System;
using System.Collections.Generic;


//  INTERFAZ: MenuPrincipal
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
            opcion = LeerNumero("Opción: ", 0, 6);
            Console.WriteLine();

            switch (opcion)
            {
                case 1:
                    AbrirModulo(Modulos.Socios, "Socios");
                    break;
                case 2:
                    AbrirModulo(Modulos.Membresias, "Membresías");
                    break;
                case 3:
                    AbrirModulo(Modulos.Pagos, "Pagos");
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

        // Solo el admin ve esta opción (depende del ROL)
        if (_usuario.PuedeGestionarUsuarios)
        {
            Console.WriteLine("5. Ver registro de actividad");
        }

        Console.WriteLine("6. Estado de la licencia");
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
    private void AbrirModulo(string modulo, string nombre)
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
