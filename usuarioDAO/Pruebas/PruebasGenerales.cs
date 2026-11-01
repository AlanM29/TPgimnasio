using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;

// =====================================================
//  PRUEBAS AUTOMÁTICAS DE TODO EL SISTEMA
//  Cómo correrlas:  perfil "Pruebas" en Visual Studio
//                   o:  dotnet run -- --pruebas
//  Cada línea muestra [OK] o [ERROR]. Al final hay un resumen.
//  Incluye los casos que armó el compañero en pruebas.cs.
// =====================================================
public static class PruebasGenerales
{
    private static int _correctas;
    private static int _errores;

    public static void Ejecutar()
    {
        _correctas = 0;
        _errores = 0;

        Console.WriteLine("=== PRUEBAS AUTOMÁTICAS DEL SISTEMA ===");

        Seccion("1. Modelo de usuarios (UsuarioBase, Admin, Recepcionista)", PruebasModelo);
        Seccion("2. Fábrica de usuarios (UsuarioFactory)", PruebasFabrica);
        Seccion("3. Seguridad (hash de contraseñas)", PruebasSeguridad);
        Seccion("4. Licencia", PruebasLicencia);
        Seccion("5. Conexión y estructura de la base de datos", PruebasBaseDeDatos);
        Seccion("6. Login (LoginService + UsuarioDao)", PruebasLogin);
        Seccion("7. Registro de actividad (log.txt)", PruebasLog);
        Seccion("8. Contrato IUsuarioDao e InicializadorBd", PruebasDaoEInicializador);

        Console.WriteLine("\n=== RESULTADO: " + _correctas + " correctas, " + _errores + " con error ===");
        if (_errores == 0)
        {
            Console.WriteLine("Todo funciona como se espera.");
        }
    }

    // ---------- 1. MODELO DE USUARIOS ----------
    private static void PruebasModelo()
    {
        DateTime hoy = DateTime.Today;

        // Casos de validación (los del compañero)
        VerificarRechazo("Usuario vacío es rechazado", () => new Admin("", "h", "Ana", "Pérez", null, null, hoy));
        VerificarRechazo("Usuario corto (menos de 4) es rechazado", () => new Admin("ab", "h", "Ana", "Pérez", null, null, hoy));
        VerificarRechazo("Usuario con espacio es rechazado", () => new Admin("ana lopez", "h", "Ana", "Pérez", null, null, hoy));
        VerificarRechazo("Hash vacío es rechazado", () => new Admin("anita", "", "Ana", "Pérez", null, null, hoy));
        VerificarRechazo("Nombre vacío es rechazado", () => new Admin("anita", "h", "", "Pérez", null, null, hoy));
        VerificarRechazo("Apellido vacío es rechazado", () => new Admin("anita", "h", "Ana", "", null, null, hoy));
        VerificarRechazo("Email inválido es rechazado", () => new Admin("anita", "h", "Ana", "Pérez", "abc", null, hoy));
        VerificarRechazo("Teléfono inválido es rechazado", () => new Admin("anita", "h", "Ana", "Pérez", null, "abc", hoy));
        VerificarAceptado("Datos válidos son aceptados", () => new Admin("anita", "h", "Ana", "Pérez", "a@b.com", "+54 341 555", hoy));
        VerificarAceptado("Email y teléfono son opcionales (null)", () => new Recepcionista("lucia", "h2", "Lucía", "Gómez", null, null, hoy));

        // Estado del objeto
        Admin admin = new Admin("anita", "h", "Ana", "Pérez", null, null, hoy);
        Recepcionista recep = new Recepcionista("lucia", "h", "Lucía", "Gómez", null, null, hoy);

        Verificar("Fecha_Creacion se guarda (corrección aplicada)", admin.Fecha_Creacion == hoy);
        Verificar("Un usuario nuevo está activo", admin.Activo);
        Verificar("Un usuario nuevo tiene Id 0", admin.Id == 0);

        recep.DarDeBaja();
        Verificar("DarDeBaja() lo desactiva", !recep.Activo);
        recep.Reactivar();
        Verificar("Reactivar() lo activa de nuevo", recep.Activo);
        recep.AsignarId(7);
        Verificar("AsignarId(7) guarda el Id", recep.Id == 7);

        Recepcionista inactivo = new Recepcionista("pedro", "h", "Pedro", "Díaz", null, null, hoy, activo: false, id: 5);
        Verificar("Constructor completo: activo=false e id=5", !inactivo.Activo && inactivo.Id == 5);

        recep.CambiarContrasenahash("nuevoHash");
        Verificar("CambiarContrasenahash actualiza el hash", recep.Contrasenahash == "nuevoHash");
        VerificarRechazo("CambiarContrasenahash vacío es rechazado", () => recep.CambiarContrasenahash(""));

        recep.ActualizarContacto("Lucía", "Gómez", "lucia@mail.com", "341-555-1234");
        Verificar("ActualizarContacto guarda email y teléfono", recep.Email == "lucia@mail.com" && recep.Telefono == "341-555-1234");
        VerificarRechazo("ActualizarContacto con email inválido es rechazado", () => recep.ActualizarContacto("Lucía", "Gómez", "malo", null));

        // Permisos por rol
        Verificar("Rol del Admin es 'Admin'", admin.Rol == "Admin");
        Verificar("Admin puede dar de baja socios", admin.PuedeDarDeBajaSocios);
        Verificar("Admin puede modificar precios", admin.PuedeModificarPrecios);
        Verificar("Admin puede gestionar usuarios", admin.PuedeGestionarUsuarios);
        Verificar("Admin puede exportar", admin.PuedeExportar);

        Verificar("Rol del recepcionista es 'Recepcionista'", recep.Rol == "Recepcionista");
        Verificar("Recepcionista NO puede dar de baja socios", !recep.PuedeDarDeBajaSocios);
        Verificar("Recepcionista NO puede modificar precios", !recep.PuedeModificarPrecios);
        Verificar("Recepcionista NO puede gestionar usuarios", !recep.PuedeGestionarUsuarios);
        Verificar("Recepcionista NO puede exportar", !recep.PuedeExportar);

        // Polimorfismo: misma lista, distinto comportamiento
        List<UsuarioBase> lista = new List<UsuarioBase> { admin, recep };
        int puedenExportar = 0;
        foreach (UsuarioBase usuario in lista)
        {
            if (usuario.PuedeExportar)
            {
                puedenExportar++;
            }
        }
        Verificar("Polimorfismo: de 2 usuarios solo 1 puede exportar", puedenExportar == 1);
    }

    // ---------- 2. FÁBRICA ----------
    private static void PruebasFabrica()
    {
        DateTime hoy = DateTime.Today;

        UsuarioBase a = UsuarioFactory.Crear("Admin", "root", "h", "Root", "Admin", null, null, hoy);
        Verificar("Crear('Admin') devuelve un Admin", a is Admin);

        UsuarioBase r = UsuarioFactory.Crear("Recepcionista", "recep1", "h", "Rita", "López", null, null, hoy);
        Verificar("Crear('Recepcionista') devuelve un Recepcionista", r is Recepcionista);

        UsuarioBase d = UsuarioFactory.Crear("Recepcionista", "recep2", "h", "Rita", "López", null, null, hoy, activo: false, id: 9);
        Verificar("La fábrica respeta activo=false e id=9", !d.Activo && d.Id == 9);

        VerificarRechazo("Un rol inventado ('Gerente') es rechazado",
            () => UsuarioFactory.Crear("Gerente", "gerente", "h", "Gabriel", "Hernández", null, null, hoy));
    }

    // ---------- 3. SEGURIDAD ----------
    private static void PruebasSeguridad()
    {
        string hash = Seguridad.CalcularHash("admin123");

        Verificar("El hash de 'admin123' coincide con el guardado en la base",
            hash == "240be518fabd2724ddb6f04eeb1da5967448d7e831c08c8fa822809f74c720a9");
        Verificar("El hash tiene 64 caracteres y está en minúsculas", hash.Length == 64 && hash == hash.ToLower());
        Verificar("El mismo texto da siempre el mismo hash", Seguridad.CalcularHash("abc") == Seguridad.CalcularHash("abc"));
        Verificar("Textos distintos dan hashes distintos", Seguridad.CalcularHash("abc") != Seguridad.CalcularHash("abd"));
    }

    // ---------- 4. LICENCIA ----------
    private static void PruebasLicencia()
    {
        LicenciaService servicio = new LicenciaService();
        string codigoCompleta = LicenciaService.GenerarCodigo("COMPLETA");
        string codigoBasica = LicenciaService.GenerarCodigo("BASICA");
        string firmaCompleta = codigoCompleta.Split('-')[2];

        Licencia completa = servicio.Validar(codigoCompleta, "");
        Licencia basica = servicio.Validar(codigoBasica, "");

        Verificar("Código COMPLETA es válido", completa.EsValida && completa.Plan == "COMPLETA");
        Verificar("Código BASICA es válido", basica.EsValida && basica.Plan == "BASICA");
        Verificar("Acepta el código escrito en minúsculas", servicio.Validar(codigoCompleta.ToLower(), "").EsValida);

        Verificar("Código vacío es inválido", !servicio.Validar("", "").EsValida);
        Verificar("Formato incorrecto es inválido", !servicio.Validar("ABC", "").EsValida);
        Verificar("Plan inexistente es inválido", !servicio.Validar("GYM-PLUS-12345678", "").EsValida);
        Verificar("Firma falsa es inválida", !servicio.Validar("GYM-COMPLETA-00000000", "").EsValida);
        Verificar("Plan editado a mano (BASICA con firma de COMPLETA) es inválido",
            !servicio.Validar("GYM-BASICA-" + firmaCompleta, "").EsValida);

        Verificar("Licencia vencida es inválida", !servicio.Validar(codigoCompleta, "2000-01-01").EsValida);
        Verificar("Fecha con formato incorrecto es inválida", !servicio.Validar(codigoCompleta, "31/12/2099").EsValida);
        Verificar("Licencia con vencimiento futuro es válida", servicio.Validar(codigoCompleta, "2099-12-31").EsValida);

        // Qué módulos habilita cada plan
        string[] modulos = { Modulos.Socios, Modulos.Membresias, Modulos.Pagos, Modulos.Asistencias };

        bool completaHabilitaTodo = true;
        foreach (string modulo in modulos)
        {
            if (!completa.PermiteModulo(modulo))
            {
                completaHabilitaTodo = false;
            }
        }
        Verificar("Plan COMPLETA habilita los 4 módulos", completaHabilitaTodo);

        Verificar("Plan BASICA habilita Socios", basica.PermiteModulo(Modulos.Socios));
        Verificar("Plan BASICA habilita Asistencias", basica.PermiteModulo(Modulos.Asistencias));
        Verificar("Plan BASICA bloquea Membresías", !basica.PermiteModulo(Modulos.Membresias));
        Verificar("Plan BASICA bloquea Pagos", !basica.PermiteModulo(Modulos.Pagos));

        Licencia invalida = servicio.Validar("", "");
        bool invalidaBloqueaTodo = true;
        foreach (string modulo in modulos)
        {
            if (invalida.PermiteModulo(modulo))
            {
                invalidaBloqueaTodo = false;
            }
        }
        Verificar("Una licencia inválida bloquea todos los módulos", invalidaBloqueaTodo);

        // El archivo real que viene con el proyecto
        Licencia delArchivo = servicio.Cargar();
        Verificar("licencia.txt del proyecto es válida (" + delArchivo.Motivo + ")", delArchivo.EsValida);
    }

    // ---------- 5. BASE DE DATOS ----------
    private static void PruebasBaseDeDatos()
    {
        Verificar("El archivo gimnasio.sqlite3 existe junto al .exe", ConexionBd.ExisteBaseDeDatos());

        string mensaje;
        Verificar("Se puede abrir la conexión", ConexionBd.ProbarConexion(out mensaje));

        string[] tablas = { "Socios", "Tipo_Membresias", "empleados", "Usuario", "Membresia", "Pago", "asistencias" };

        using (SqliteConnection conexion = ConexionBd.ObtenerConexion())
        {
            foreach (string tabla in tablas)
            {
                string sql = "SELECT COUNT(*) FROM sqlite_master WHERE type = 'table' AND name = @nombre COLLATE NOCASE";
                using (SqliteCommand comando = new SqliteCommand(sql, conexion))
                {
                    comando.Parameters.AddWithValue("@nombre", tabla);
                    Verificar("Existe la tabla " + tabla, Convert.ToInt32(comando.ExecuteScalar()) == 1);
                }
            }

            using (SqliteCommand comando = new SqliteCommand("SELECT COUNT(*) FROM Usuario", conexion))
            {
                Verificar("Hay al menos un usuario cargado", Convert.ToInt32(comando.ExecuteScalar()) >= 1);
            }

            using (SqliteCommand comando = new SqliteCommand("PRAGMA foreign_keys", conexion))
            {
                Verificar("Las claves foráneas están activadas", Convert.ToInt32(comando.ExecuteScalar()) == 1);
            }
        }
    }

    // ---------- 6. LOGIN ----------
    private static void PruebasLogin()
    {
        LoginService login = new LoginService();

        UsuarioBase? admin = login.Ingresar("admin", "admin123");
        Verificar("admin / admin123 puede entrar", admin != null);
        Verificar("Y entra como Admin", admin is Admin);
        Verificar("Trae los datos del empleado (JOIN)", admin != null && !string.IsNullOrEmpty(admin.Nombre));

        UsuarioBase? recep = login.Ingresar("recepcion", "recep123");
        Verificar("recepcion / recep123 puede entrar", recep != null);
        Verificar("Y entra como Recepcionista", recep is Recepcionista);

        Verificar("Contraseña incorrecta es rechazada", login.Ingresar("admin", "contraseñaMala") == null);
        Verificar("Usuario inexistente es rechazado", login.Ingresar("fantasma", "admin123") == null);
        Verificar("Contraseña de otro usuario es rechazada", login.Ingresar("admin", "recep123") == null);
        Verificar("Inyección SQL (' OR '1'='1) es rechazada", login.Ingresar("' OR '1'='1", "' OR '1'='1") == null);
        Verificar("Inyección SQL con comentario (--) es rechazada", login.Ingresar("admin' --", "x") == null);
    }

    // ---------- 7. LOG ----------
    private static void PruebasLog()
    {
        string marca = "prueba-" + DateTime.Now.Ticks;
        LogService.Registrar("PRUEBA", "TEST", marca);

        bool encontrada = false;
        foreach (string linea in LogService.ObtenerUltimos(5))
        {
            if (linea.Contains(marca))
            {
                encontrada = true;
            }
        }

        Verificar("Registrar() escribe en log.txt y ObtenerUltimos() lo lee", encontrada);
        Verificar("ObtenerUltimos(2) no devuelve más de 2 líneas", LogService.ObtenerUltimos(2).Count <= 2);
    }

    // ---------- 8. INTERFAZ IUsuarioDao E INICIALIZADOR ----------
    private static void PruebasDaoEInicializador()
    {
        // LoginService con un DAO "de mentira": prueba la lógica sin tocar la base real
        UsuarioDaoFalso falso = new UsuarioDaoFalso();
        LoginService login = new LoginService(falso);

        UsuarioBase? usuario = login.Ingresar("falso", "clave123");
        Verificar("LoginService funciona con cualquier IUsuarioDao (DAO falso)", usuario is Admin);
        Verificar("El servicio le manda al DAO el HASH, no la contraseña en texto plano",
            falso.UltimoHashRecibido == Seguridad.CalcularHash("clave123") && falso.UltimoHashRecibido != "clave123");
        Verificar("Si el DAO no encuentra al usuario, el login devuelve null", login.Ingresar("otro", "x") == null);

        // InicializadorBd sobre una base en memoria (no toca el archivo real)
        using (SqliteConnection memoria = new SqliteConnection("Data Source=:memory:"))
        {
            memoria.Open();
            InicializadorBd.Inicializar(memoria);

            Verificar("InicializadorBd crea las 7 tablas",
                ContarFilas(memoria, "SELECT COUNT(*) FROM sqlite_master WHERE type = 'table' AND name NOT LIKE 'sqlite_%'") == 7);
            Verificar("... y carga los 3 tipos de membresía", ContarFilas(memoria, "SELECT COUNT(*) FROM Tipo_Membresias") == 3);
            Verificar("... y los 2 usuarios iniciales", ContarFilas(memoria, "SELECT COUNT(*) FROM Usuario") == 2);

            InicializadorBd.Inicializar(memoria);
            Verificar("Ejecutarlo dos veces no duplica los datos",
                ContarFilas(memoria, "SELECT COUNT(*) FROM Usuario") == 2 &&
                ContarFilas(memoria, "SELECT COUNT(*) FROM Tipo_Membresias") == 3);
        }
    }

    private static int ContarFilas(SqliteConnection conexion, string sql)
    {
        using (SqliteCommand comando = new SqliteCommand(sql, conexion))
        {
            return Convert.ToInt32(comando.ExecuteScalar());
        }
    }

    // DAO falso: cumple el contrato IUsuarioDao pero no usa ninguna base de datos
    private class UsuarioDaoFalso : IUsuarioDao
    {
        public string? UltimoHashRecibido;

        public UsuarioBase? ObtenerPorCredenciales(string nombreUsuario, string contrasenaHash)
        {
            UltimoHashRecibido = contrasenaHash;

            if (nombreUsuario == "falso")
            {
                return new Admin("falso", contrasenaHash, "Falso", "Prueba", null, null, DateTime.Today);
            }
            return null;
        }
    }

    // =====================================================
    //  Herramientas de las pruebas
    // =====================================================
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
            _correctas++;
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("  [OK]    " + descripcion);
        }
        else
        {
            _errores++;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("  [ERROR] " + descripcion);
        }
        Console.ResetColor();
    }

    // La acción tiene que lanzar una ArgumentException (dato rechazado)
    private static void VerificarRechazo(string descripcion, Action accion)
    {
        bool rechazado = false;
        try
        {
            accion();
        }
        catch (ArgumentException)
        {
            rechazado = true;
        }
        Verificar(descripcion, rechazado);
    }

    // La acción NO tiene que lanzar ArgumentException (dato aceptado)
    private static void VerificarAceptado(string descripcion, Action accion)
    {
        bool aceptado = true;
        try
        {
            accion();
        }
        catch (ArgumentException)
        {
            aceptado = false;
        }
        Verificar(descripcion, aceptado);
    }
}
