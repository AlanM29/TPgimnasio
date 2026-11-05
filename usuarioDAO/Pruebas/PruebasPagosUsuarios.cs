using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;

//  PRUEBAS de usuarios (alta/baja/modificación), pagos y exportación.
//  Se ejecutan con:  dotnet run -- --pruebas-pagos
//  Usan la base real, con datos distintos en cada corrida (DNI y nombres únicos).

public static class PruebasPagosUsuarios
{
    private static int _ok;
    private static int _error;
    private static int _contador;

    public static void Ejecutar()
    {
        _ok = 0;
        _error = 0;
        Console.WriteLine("=== PRUEBAS DE USUARIOS Y PAGOS ===");

        string mensaje;
        if (!InicializadorBd.Inicializar(out mensaje))
        {
            Console.WriteLine(mensaje);
            return;
        }

        Seccion("1. Modelo Pago", PruebasModeloPago);
        Seccion("2. Permisos por rol", PruebasPermisos);
        Seccion("3. UsuarioDAO (base real)", PruebasUsuarioDao);
        Seccion("4. ServicioUsuarios", PruebasServicioUsuarios);
        Seccion("5. PagoDAO (base real)", PruebasPagoDao);
        Seccion("6. ServicioPagos y exportación", PruebasServicioPagos);

        Console.WriteLine("\n=== RESULTADO: " + _ok + " correctas, " + _error + " con error ===");
    }

    // ---------- 1. MODELO PAGO ----------
    private static void PruebasModeloPago()
    {
        DateTime hoy = DateTime.Now;

        Lanza<ArgumentException>("Monto 0 es rechazado", () => new Pago(1, 0, "Efectivo", hoy));
        Lanza<ArgumentException>("Monto negativo es rechazado", () => new Pago(1, -50, "Efectivo", hoy));
        Lanza<ArgumentException>("Monto que redondea a 0 (0,001) es rechazado", () => new Pago(1, 0.001m, "Efectivo", hoy));
        Lanza<ArgumentException>("Método inexistente ('Cheque') es rechazado", () => new Pago(1, 100, "Cheque", hoy));
        Lanza<ArgumentException>("Método vacío es rechazado", () => new Pago(1, 100, "", hoy));
        Lanza<ArgumentException>("Membresía 0 es rechazada", () => new Pago(0, 100, "Efectivo", hoy));
        Lanza<ArgumentException>("Estado inventado es rechazado", () => new Pago(1, 100, "Efectivo", hoy, "Pendiente"));

        Pago pago = new Pago(3, 25000, "tarjeta", hoy);
        Verificar("El método se normaliza ('tarjeta' -> 'Tarjeta')", pago.Metodo == "Tarjeta");
        Verificar("Un pago nuevo está 'Pagado' y con Id 0", pago.EstaPagado && pago.Id == 0);
        Verificar("Guarda monto y membresía", pago.Monto == 25000m && pago.IdMembresia == 3);

        pago.AsignarId(8);
        Verificar("AsignarId guarda el id", pago.Id == 8);

        pago.Anular();
        Verificar("Anular() lo deja 'Anulado'", pago.Estado == "Anulado" && !pago.EstaPagado);
        Lanza<InvalidOperationException>("Anular dos veces es rechazado", () => pago.Anular());
    }

    // ---------- 2. PERMISOS ----------
    private static void PruebasPermisos()
    {
        Admin admin = new Admin("adminx", "h", "Ana", "Pérez", "12345678", null, null, DateTime.Today);
        Recepcionista recep = new Recepcionista("recepx", "h", "Lu", "Gómez", "12345679", null, null, DateTime.Today);

        Verificar("Admin puede anular pagos", admin.PuedeAnularPagos);
        Verificar("Recepcionista NO puede anular pagos", !recep.PuedeAnularPagos);
    }

    // ---------- 3. USUARIO DAO ----------
    private static void PruebasUsuarioDao()
    {
        IUsuarioDAO dao = new UsuarioDAO();
        string dni = NuevoDni();
        string nombreUsuario = NuevoNombreUsuario();

        UsuarioBase nuevo = new Recepcionista(nombreUsuario, Seguridad.CalcularHash("clave123"),
                                              "Juan", "Prueba", dni, null, "341-555-0000", DateTime.Now);
        int id = dao.Insertar(nuevo);
        Verificar("Insertar devuelve un id mayor a 0 y lo asigna al objeto", id > 0 && nuevo.Id == id);

        UsuarioBase? leido = dao.ObtenerPorId(id);
        Verificar("ObtenerPorId lo encuentra como Recepcionista",
                  leido is Recepcionista && leido.Nombre_Usuario == nombreUsuario && leido.Dni == dni && leido.Activo);
        Verificar("ObtenerPorNombreUsuario lo encuentra", dao.ObtenerPorNombreUsuario(nombreUsuario)?.Id == id);
        Verificar("Un id inexistente devuelve null", dao.ObtenerPorId(-1) == null);
        Verificar("Puede iniciar sesión con sus credenciales",
                  dao.ObtenerPorCredenciales(nombreUsuario, Seguridad.CalcularHash("clave123"))?.Id == id);

        // Duplicados
        Lanza<InvalidOperationException>("Nombre de usuario repetido es rechazado", () =>
            dao.Insertar(new Recepcionista(nombreUsuario, "h", "Otro", "Otro", NuevoDni(), null, null, DateTime.Now)));
        Lanza<InvalidOperationException>("DNI repetido es rechazado", () =>
            dao.Insertar(new Recepcionista(NuevoNombreUsuario(), "h", "Otro", "Otro", dni, null, null, DateTime.Now)));

        // Admin
        UsuarioBase admin = new Admin(NuevoNombreUsuario(), "h", "Ana", "Jefa", NuevoDni(), null, null, DateTime.Now);
        int idAdmin = dao.Insertar(admin);
        Verificar("Un Admin insertado se lee como Admin", dao.ObtenerPorId(idAdmin) is Admin);

        // Modificación
        leido!.ActualizarContacto("Juan Carlos", "Prueba", "jc" + dni + "@mail.com", null);
        leido.CambiarContrasenahash(Seguridad.CalcularHash("otra456"));
        Verificar("Actualizar devuelve true", dao.Actualizar(leido));

        UsuarioBase? releido = dao.ObtenerPorId(id);
        Verificar("Actualizar persistió nombre, email y teléfono nulo",
                  releido!.Nombre == "Juan Carlos" && releido.Email == "jc" + dni + "@mail.com" && releido.Telefono == null);
        Verificar("La contraseña nueva funciona y la vieja no",
                  dao.ObtenerPorCredenciales(nombreUsuario, Seguridad.CalcularHash("otra456")) != null &&
                  dao.ObtenerPorCredenciales(nombreUsuario, Seguridad.CalcularHash("clave123")) == null);

        // Baja y reactivación
        Verificar("DarDeBaja devuelve true", dao.DarDeBaja(id));
        Verificar("Tras la baja figura inactivo", dao.ObtenerPorId(id)!.Activo == false);
        Verificar("Tras la baja ya no puede iniciar sesión",
                  dao.ObtenerPorCredenciales(nombreUsuario, Seguridad.CalcularHash("otra456")) == null);
        Verificar("ObtenerTodos(soloActivos) NO lo incluye", !dao.ObtenerTodos(true).Exists(u => u.Id == id));
        Verificar("ObtenerTodos(todos) SÍ lo incluye", dao.ObtenerTodos(false).Exists(u => u.Id == id));
        Verificar("DarDeBaja de un id inexistente devuelve false", !dao.DarDeBaja(-1));

        Verificar("Reactivar devuelve true", dao.Reactivar(id));
        Verificar("Tras reactivar puede entrar de nuevo",
                  dao.ObtenerPorCredenciales(nombreUsuario, Seguridad.CalcularHash("otra456")) != null);

        // Los usuarios de prueba se dejan inactivos para que no queden usables
        dao.DarDeBaja(id);
        dao.DarDeBaja(idAdmin);
    }

    // ---------- 4. SERVICIO USUARIOS ----------
    private static void PruebasServicioUsuarios()
    {
        UsuarioBase admin = new Admin("adminfalso", "h", "Ana", "Pérez", "12345678", null, null, DateTime.Today);
        UsuarioBase recep = new Recepcionista("recepfalso", "h", "Lu", "Gómez", "12345679", null, null, DateTime.Today);

        ServicioUsuarios comoRecep = new ServicioUsuarios(recep);
        Lanza<UnauthorizedAccessException>("Recepcionista no puede listar usuarios", () => comoRecep.Listar(false));
        Lanza<UnauthorizedAccessException>("Recepcionista no puede crear usuarios", () =>
            comoRecep.Crear("Recepcionista", NuevoNombreUsuario(), "clave123", "A", "B", NuevoDni(), null, null));
        Lanza<UnauthorizedAccessException>("Recepcionista no puede dar de baja", () => comoRecep.DarDeBaja(1));

        ServicioUsuarios comoAdmin = new ServicioUsuarios(admin);
        Lanza<ArgumentException>("Contraseña de menos de 6 caracteres es rechazada", () =>
            comoAdmin.Crear("Recepcionista", NuevoNombreUsuario(), "123", "A", "B", NuevoDni(), null, null));
        Lanza<ArgumentException>("Rol inventado es rechazado", () =>
            comoAdmin.Crear("Gerente", NuevoNombreUsuario(), "clave123", "A", "B", NuevoDni(), null, null));
        Lanza<ArgumentException>("DNI inválido es rechazado", () =>
            comoAdmin.Crear("Recepcionista", NuevoNombreUsuario(), "clave123", "A", "B", "abc", null, null));

        string nombreUsuario = NuevoNombreUsuario();
        UsuarioBase creado = comoAdmin.Crear("Admin", nombreUsuario, "clave123", "Nuevo", "Admin", NuevoDni(), null, null);
        Verificar("Admin puede crear usuarios (queda con id)", creado.Id > 0 && creado is Admin);
        Verificar("Se guarda el HASH de la contraseña, no el texto",
                  creado.Contrasenahash == Seguridad.CalcularHash("clave123"));
        Verificar("El usuario creado puede iniciar sesión", new LoginService().Ingresar(nombreUsuario, "clave123") != null);

        comoAdmin.ModificarDatos(creado.Id, "Nuevo2", "Admin", null, null);
        Verificar("ModificarDatos cambia el nombre", comoAdmin.ObtenerPorId(creado.Id).Nombre == "Nuevo2");

        comoAdmin.CambiarContrasena(creado.Id, "claveNueva1");
        Verificar("CambiarContrasena: entra con la nueva", new LoginService().Ingresar(nombreUsuario, "claveNueva1") != null);

        Lanza<InvalidOperationException>("Modificar un usuario inexistente es rechazado", () =>
            comoAdmin.ModificarDatos(-1, "A", "B", null, null));

        // Un admin real no puede darse de baja a sí mismo
        ServicioUsuarios comoCreado = new ServicioUsuarios(creado);
        Lanza<InvalidOperationException>("Nadie puede darse de baja a sí mismo", () => comoCreado.DarDeBaja(creado.Id));

        comoAdmin.DarDeBaja(creado.Id);
        Verificar("DarDeBaja: ya no puede iniciar sesión", new LoginService().Ingresar(nombreUsuario, "claveNueva1") == null);
        Lanza<InvalidOperationException>("Dar de baja dos veces es rechazado", () => comoAdmin.DarDeBaja(creado.Id));

        comoAdmin.Reactivar(creado.Id);
        Verificar("Reactivar: vuelve a poder entrar", new LoginService().Ingresar(nombreUsuario, "claveNueva1") != null);
        Lanza<InvalidOperationException>("Reactivar a un usuario activo es rechazado", () => comoAdmin.Reactivar(creado.Id));

        comoAdmin.DarDeBaja(creado.Id);// no queda usable
    }

    // ---------- 5. PAGO DAO ----------
    private static void PruebasPagoDao()
    {
        IPagoDAO dao = new PagoDAO();
        int idSocio;
        int idMembresia = CrearMembresia(out idSocio);

        Verificar("ExisteMembresia encuentra la membresía creada", dao.ExisteMembresia(idMembresia));
        Verificar("ExisteMembresia con id inexistente da false", !dao.ExisteMembresia(-1));
        Verificar("Una membresía nueva no está pagada", !dao.EstaPagada(idMembresia));

        Pago pago = new Pago(idMembresia, 25000.50m, "Transferencia", DateTime.Now);
        int idPago = dao.Registrar(pago);
        Verificar("Registrar devuelve un id y lo asigna al objeto", idPago > 0 && pago.Id == idPago);
        Verificar("Ahora la membresía figura pagada", dao.EstaPagada(idMembresia));

        Pago? leido = dao.ObtenerPorId(idPago);
        Verificar("ObtenerPorId trae monto, método y estado",
                  leido != null && leido.Monto == 25000.50m && leido.Metodo == "Transferencia" && leido.EstaPagado);
        Verificar("ObtenerPorId inexistente devuelve null", dao.ObtenerPorId(-1) == null);

        Verificar("ObtenerPorMembresia devuelve exactamente 1", dao.ObtenerPorMembresia(idMembresia).Count == 1);
        Verificar("ObtenerPorSocio lo encuentra a través de la membresía",
                  dao.ObtenerPorSocio(idSocio).Exists(p => p.Id == idPago));
        Verificar("ObtenerPorSocio de un socio sin pagos da lista vacía", dao.ObtenerPorSocio(-1).Count == 0);
        Verificar("ObtenerPagadosEntre (hoy) lo incluye",
                  dao.ObtenerPagadosEntre(DateTime.Today, DateTime.Today.AddDays(1)).Exists(p => p.Id == idPago));
        Verificar("ObtenerPagadosEntre (ayer) NO lo incluye",
                  !dao.ObtenerPagadosEntre(DateTime.Today.AddDays(-2), DateTime.Today).Exists(p => p.Id == idPago));

        Verificar("Anular devuelve true", dao.Anular(idPago));
        Verificar("Tras anular, la membresía deja de figurar pagada", !dao.EstaPagada(idMembresia));
        Verificar("El pago figura 'Anulado'", dao.ObtenerPorId(idPago)!.Estado == "Anulado");
        Verificar("Anular de nuevo devuelve false", !dao.Anular(idPago));
        Verificar("ObtenerPagadosEntre ya no incluye al anulado",
                  !dao.ObtenerPagadosEntre(DateTime.Today, DateTime.Today.AddDays(1)).Exists(p => p.Id == idPago));
    }

    // ---------- 6. SERVICIO PAGOS ----------
    private static void PruebasServicioPagos()
    {
        UsuarioBase admin = new Admin("adminfalso", "h", "Ana", "Pérez", "12345678", null, null, DateTime.Today);
        UsuarioBase recep = new Recepcionista("recepfalso", "h", "Lu", "Gómez", "12345679", null, null, DateTime.Today);
        ServicioPagos comoAdmin = new ServicioPagos(admin);
        ServicioPagos comoRecep = new ServicioPagos(recep);

        int idSocio;
        int idMembresia = CrearMembresia(out idSocio);

        Lanza<InvalidOperationException>("Pagar una membresía inexistente es rechazado",
            () => comoRecep.Registrar(999999999, 1000, "Efectivo"));
        Lanza<ArgumentException>("Monto 0 es rechazado por el servicio",
            () => comoRecep.Registrar(idMembresia, 0, "Efectivo"));
        Lanza<ArgumentException>("Método inválido es rechazado por el servicio",
            () => comoRecep.Registrar(idMembresia, 1000, "Cheque"));

        Pago pago = comoRecep.Registrar(idMembresia, 25000, "efectivo");
        Verificar("El recepcionista puede registrar un pago", pago.Id > 0 && pago.Metodo == "Efectivo");

        Lanza<InvalidOperationException>("Segundo pago de la misma membresía es rechazado",
            () => comoRecep.Registrar(idMembresia, 25000, "Efectivo"));

        Verificar("ConsultarPorMembresia lo devuelve", comoRecep.ConsultarPorMembresia(idMembresia).Count == 1);
        Lanza<InvalidOperationException>("ConsultarPorMembresia inexistente es rechazado",
            () => comoRecep.ConsultarPorMembresia(-1));
        Lanza<InvalidOperationException>("ConsultarPorSocio con DNI inexistente es rechazado",
            () => comoRecep.ConsultarPorSocio("00000001"));

        string dniSocio = new SocioDAO().ObtenerPorId(idSocio)!.Dni;
        Verificar("ConsultarPorSocio (por DNI) lo devuelve",
                  comoRecep.ConsultarPorSocio(dniSocio).Exists(p => p.Id == pago.Id));

        // Anular: solo Admin
        Lanza<UnauthorizedAccessException>("Recepcionista NO puede anular pagos", () => comoRecep.Anular(pago.Id));
        Verificar("El pago sigue 'Pagado' tras el intento", comoRecep.ObtenerPorId(pago.Id).EstaPagado);
        Lanza<InvalidOperationException>("Anular un pago inexistente es rechazado", () => comoAdmin.Anular(-1));

        // Exportar: solo con permiso
        Lanza<UnauthorizedAccessException>("Recepcionista NO puede exportar",
            () => comoRecep.ExportarRecaudacion(DateTime.Today, DateTime.Today));
        Lanza<ArgumentException>("Exportar con 'hasta' anterior a 'desde' es rechazado",
            () => comoAdmin.ExportarRecaudacion(DateTime.Today, DateTime.Today.AddDays(-1)));
        Lanza<InvalidOperationException>("Exportar un período sin pagos es rechazado",
            () => comoAdmin.ExportarRecaudacion(new DateTime(2000, 1, 1), new DateTime(2000, 1, 2)));

        string ruta = comoAdmin.ExportarRecaudacion(DateTime.Today, DateTime.Today);
        Verificar("El Admin exporta y el archivo existe", File.Exists(ruta));
        string contenido = File.ReadAllText(ruta);
        Verificar("El CSV tiene encabezado, el pago y la fila TOTAL",
                  contenido.Contains("id_pago;fecha;id_membresia;metodo;monto") &&
                  contenido.Contains(pago.Id + ";") && contenido.Contains("TOTAL"));
        File.Delete(ruta);

        comoAdmin.Anular(pago.Id);
        Verificar("El Admin puede anular el pago", !comoAdmin.ObtenerPorId(pago.Id).EstaPagado);
        Lanza<InvalidOperationException>("Anular un pago ya anulado es rechazado", () => comoAdmin.Anular(pago.Id));

        Pago segundo = comoRecep.Registrar(idMembresia, 25000, "Tarjeta");
        Verificar("Tras anular, la membresía puede volver a pagarse", segundo.Id > 0 && segundo.Id != pago.Id);
        comoAdmin.Anular(segundo.Id);// no queda en la recaudación
    }

    // =====================================================
    //  Ayudas
    // =====================================================

    // Crea un socio y una membresía (insertada con SQL directo: ese DAO es de otro módulo)
    private static int CrearMembresia(out int idSocio)
    {
        string dni = NuevoDni();
        Socio socio = new Socio(dni, "Pago", "Tester" + dni, null, null, DateTime.Now);
        idSocio = new SocioDAO().Insertar(socio);

        using (SQLiteConnection conexion = ConexionBd.ObtenerConexion())
        {
            string sql = "INSERT INTO Membresia (id_socio, id_tipo, fecha_inicio, fecha_expiracion, cuota_abonada) " +
                         "VALUES (@socio, (SELECT id_tipo FROM Tipo_Membresias LIMIT 1), @inicio, @fin, 0)";
            using (SQLiteCommand comando = new SQLiteCommand(sql, conexion))
            {
                comando.Parameters.AddWithValue("@socio", idSocio);
                comando.Parameters.AddWithValue("@inicio", DateTime.Today.ToString("yyyy-MM-dd"));
                comando.Parameters.AddWithValue("@fin", DateTime.Today.AddDays(30).ToString("yyyy-MM-dd"));
                comando.ExecuteNonQuery();
            }
            return (int)conexion.LastInsertRowId;
        }
    }

    // DNI de 8 dígitos distinto en cada llamada
    private static string NuevoDni()
    {
        _contador++;
        return (10000000 + (DateTime.Now.Ticks % 80000000) + _contador).ToString();
    }

    private static string NuevoNombreUsuario()
    {
        _contador++;
        return "test" + _contador + (DateTime.Now.Ticks % 1000000);
    }

    private static void Seccion(string titulo, Action pruebas)
    {
        Console.WriteLine("\n--- " + titulo + " ---");
        try
        {
            pruebas();
        }
        catch (Exception ex)
        {
            Verificar("La sección terminó sin errores inesperados (" + ex.GetType().Name + ": " + ex.Message + ")", false);
        }
    }

    private static void Verificar(string descripcion, bool condicion)
    {
        if (condicion)
        {
            _ok++;
            Console.WriteLine("[OK]    " + descripcion);
        }
        else
        {
            _error++;
            Console.WriteLine("[ERROR] " + descripcion);
        }
    }

    // La acción tiene que lanzar la excepción indicada
    private static void Lanza<T>(string descripcion, Action accion) where T : Exception
    {
        bool lanzada = false;
        try
        {
            accion();
        }
        catch (T)
        {
            lanzada = true;
        }
        Verificar(descripcion, lanzada);
    }
}
