using System;
using Microsoft.Data.Sqlite;


//  DAO: InicializadorBd
//  Deja la base de datos lista para usar:
//    1) crea las tablas que falten 
//    2) carga los datos iniciales SOLO si la tabla está vacía

public static class InicializadorBd
{
    // Abre la base (la crea si no existe) y la deja lista.
    // Devuelve false si algo falla, y explica el motivo en "mensaje".
    public static bool Inicializar(out string mensaje)
    {
        try
        {
            using (SqliteConnection conexion = ConexionBd.ObtenerConexion())
            {
                Inicializar(conexion);
            }
            mensaje = "Base de datos lista.";
            return true;
        }
        catch (SqliteException ex)
        {
            mensaje = "No se pudo preparar la base de datos: " + ex.Message;
            return false;
        }
    }

    // Versión que recibe la conexión
    public static void Inicializar(SqliteConnection conexion)
    {
        CrearTablas(conexion);
        CargarTiposDeMembresia(conexion);
        CargarUsuariosIniciales(conexion);
    }

    // ---------- 1. TABLAS ----------
    // Primero las tablas "padre" y después las que dependen de ellas
    private static void CrearTablas(SqliteConnection conexion)
    {
        string[] tablas =
        {
            @"CREATE TABLE IF NOT EXISTS Tipo_Membresias (
                id_tipo       INTEGER PRIMARY KEY AUTOINCREMENT,
                tipo          TEXT    NOT NULL UNIQUE,
                precio_base   REAL    CHECK (precio_base IS NULL OR precio_base > 0),
                duracion_dias INTEGER NOT NULL CHECK (duracion_dias > 0)
            )",

            @"CREATE TABLE IF NOT EXISTS Socios (
                id_socio   INTEGER PRIMARY KEY AUTOINCREMENT,
                nombre     TEXT NOT NULL,
                apellido   TEXT NOT NULL,
                email      TEXT UNIQUE,
                telefono   TEXT,
                Estado     TEXT NOT NULL DEFAULT 'Activo',
                fecha_alta TEXT NOT NULL DEFAULT (datetime('now', 'localtime'))
            )",

            @"CREATE TABLE IF NOT EXISTS empleados (
                id_empleado   INTEGER PRIMARY KEY AUTOINCREMENT,
                nombre        TEXT NOT NULL,
                apellido      TEXT NOT NULL,
                email         TEXT UNIQUE,
                telefono      TEXT,
                puesto        TEXT NOT NULL,
                fecha_ingreso TEXT NOT NULL DEFAULT (datetime('now', 'localtime')),
                estado        TEXT NOT NULL DEFAULT 'Activo'
            )",

            @"CREATE TABLE IF NOT EXISTS Usuario (
                id_usuario     INTEGER PRIMARY KEY AUTOINCREMENT,
                id_empleado    INTEGER NOT NULL,
                nombre_usuario TEXT NOT NULL UNIQUE,
                contrasenahash TEXT NOT NULL,
                rol            TEXT NOT NULL CHECK (rol IN ('admin', 'usuario')),
                FOREIGN KEY (id_empleado) REFERENCES empleados (id_empleado)
                    ON UPDATE CASCADE ON DELETE RESTRICT
            )",

            @"CREATE TABLE IF NOT EXISTS Membresia (
                id_membresia     INTEGER PRIMARY KEY AUTOINCREMENT,
                id_socio         INTEGER NOT NULL,
                id_tipo          INTEGER NOT NULL,
                fecha_inicio     TEXT NOT NULL,
                fecha_expiracion TEXT,
                cuota_abonada    REAL NOT NULL DEFAULT 0 CHECK (cuota_abonada >= 0),
                FOREIGN KEY (id_socio) REFERENCES Socios (id_socio)
                    ON UPDATE CASCADE ON DELETE RESTRICT,
                FOREIGN KEY (id_tipo) REFERENCES Tipo_Membresias (id_tipo)
                    ON UPDATE CASCADE ON DELETE RESTRICT
            )",

            @"CREATE TABLE IF NOT EXISTS Pago (
                id_pago      INTEGER PRIMARY KEY AUTOINCREMENT,
                id_membresia INTEGER NOT NULL,
                fecha_pago   TEXT NOT NULL DEFAULT (datetime('now', 'localtime')),
                monto        REAL NOT NULL CHECK (monto > 0),
                metodo_pago  TEXT NOT NULL,
                estado       TEXT NOT NULL,
                FOREIGN KEY (id_membresia) REFERENCES Membresia (id_membresia)
                    ON UPDATE CASCADE ON DELETE RESTRICT
            )",

            @"CREATE TABLE IF NOT EXISTS asistencias (
                id_asis  INTEGER PRIMARY KEY AUTOINCREMENT,
                id_socio INTEGER NOT NULL,
                tiempo   TEXT NOT NULL DEFAULT (datetime('now', 'localtime')),
                FOREIGN KEY (id_socio) REFERENCES Socios (id_socio)
                    ON UPDATE CASCADE ON DELETE RESTRICT
            )"
        };

        foreach (string sql in tablas)
        {
            EjecutarSql(conexion, sql);
        }
    }

    // ---------- 2. DATOS INICIALES ----------
    private static void CargarTiposDeMembresia(SqliteConnection conexion)
    {
        if (Contar(conexion, "Tipo_Membresias") > 0)
        {
            return;
        }

        InsertarTipo(conexion, "Mensual", 25000, 30);
        InsertarTipo(conexion, "Trimestral", 65000, 90);
        InsertarTipo(conexion, "Anual", 220000, 365);
    }

    private static void InsertarTipo(SqliteConnection conexion, string tipo, double precio, int dias)
    {
        string sql = "INSERT INTO Tipo_Membresias (tipo, precio_base, duracion_dias) " +
                     "VALUES (@tipo, @precio, @dias)";

        using (SqliteCommand comando = new SqliteCommand(sql, conexion))
        {
            comando.Parameters.AddWithValue("@tipo", tipo);
            comando.Parameters.AddWithValue("@precio", precio);
            comando.Parameters.AddWithValue("@dias", dias);
            comando.ExecuteNonQuery();
        }
    }

    // Si no hay ningún usuario, crea un admin y un recepcionista para poder entrar
    private static void CargarUsuariosIniciales(SqliteConnection conexion)
    {
        if (Contar(conexion, "Usuario") > 0)
        {
            return;
        }

        int idAdmin = ObtenerOCrearEmpleado(conexion, "Admin", "Sistema",
                                            "admin@gimnasio.com", "0000000000", "Administrador");
        InsertarUsuario(conexion, idAdmin, "admin", Seguridad.CalcularHash("admin123"), "admin");

        int idRecepcion = ObtenerOCrearEmpleado(conexion, "Recepcion", "Gimnasio",
                                                "recepcion@gimnasio.com", "0000000001", "Recepcionista");
        InsertarUsuario(conexion, idRecepcion, "recepcion", Seguridad.CalcularHash("recep123"), "usuario");
    }

    private static int ObtenerOCrearEmpleado(SqliteConnection conexion, string nombre, string apellido,
                                             string email, string telefono, string puesto)
    {
        // OR IGNORE: si ya existía un empleado con ese email, no lo duplica
        string sqlInsertar = "INSERT OR IGNORE INTO empleados (nombre, apellido, email, telefono, puesto) " +
                             "VALUES (@nombre, @apellido, @email, @telefono, @puesto)";

        using (SqliteCommand comando = new SqliteCommand(sqlInsertar, conexion))
        {
            comando.Parameters.AddWithValue("@nombre", nombre);
            comando.Parameters.AddWithValue("@apellido", apellido);
            comando.Parameters.AddWithValue("@email", email);
            comando.Parameters.AddWithValue("@telefono", telefono);
            comando.Parameters.AddWithValue("@puesto", puesto);
            comando.ExecuteNonQuery();
        }

        using (SqliteCommand comando = new SqliteCommand("SELECT id_empleado FROM empleados WHERE email = @email", conexion))
        {
            comando.Parameters.AddWithValue("@email", email);
            return Convert.ToInt32(comando.ExecuteScalar());
        }
    }

    private static void InsertarUsuario(SqliteConnection conexion, int idEmpleado, string nombreUsuario,
                                        string contrasenaHash, string rol)
    {
        string sql = "INSERT INTO Usuario (id_empleado, nombre_usuario, contrasenahash, rol) " +
                     "VALUES (@empleado, @usuario, @hash, @rol)";

        using (SqliteCommand comando = new SqliteCommand(sql, conexion))
        {
            comando.Parameters.AddWithValue("@empleado", idEmpleado);
            comando.Parameters.AddWithValue("@usuario", nombreUsuario);
            comando.Parameters.AddWithValue("@hash", contrasenaHash);
            comando.Parameters.AddWithValue("@rol", rol);
            comando.ExecuteNonQuery();
        }
    }

    // ---------- AYUDAS ----------
    private static void EjecutarSql(SqliteConnection conexion, string sql)
    {
        using (SqliteCommand comando = new SqliteCommand(sql, conexion))
        {
            comando.ExecuteNonQuery();
        }
    }

    // Solo se usa con nombres de tabla escritos en este mismo archivo
    private static int Contar(SqliteConnection conexion, string tabla)
    {
        using (SqliteCommand comando = new SqliteCommand("SELECT COUNT(*) FROM " + tabla, conexion))
        {
            return Convert.ToInt32(comando.ExecuteScalar());
        }
    }
}
