using System;
using System.Data.SQLite;

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
            using (SQLiteConnection conexion = ConexionBd.ObtenerConexion())
            {
                Inicializar(conexion);
            }
            mensaje = "Base de datos lista.";
            return true;
        }
        catch (SQLiteException ex)
        {
            mensaje = "No se pudo preparar la base de datos: " + ex.Message;
            return false;
        }
    }

    // Versión que recibe la conexión
    public static void Inicializar(SQLiteConnection conexion)
    {
        CrearTablas(conexion);
        CargarTiposDeMembresia(conexion);
        CargarUsuariosIniciales(conexion);
    }

    // ---------- 1. TABLAS ----------
    // Primero las tablas "padre" y después las que dependen de ellas
    private static void CrearTablas(SQLiteConnection conexion)
    {
        string[] tablas =
        {
            @"CREATE TABLE IF NOT EXISTS Tipo_Membresias (
                id_tipo       INTEGER PRIMARY KEY AUTOINCREMENT,
                tipo          TEXT    NOT NULL UNIQUE,
                precio_base   REAL    CHECK (precio_base IS NULL OR precio_base > 0),
                duracion_dias INTEGER NOT NULL CHECK (duracion_dias > 0), 
                recargo_porcentaje REAL NOT NULL DEFAULT 0
                    CHECK (recargo_porcentaje >= 0 AND recargo_porcentaje <=  100)
              )",

            @"CREATE TABLE IF NOT EXISTS Socios (
                id_socio   INTEGER PRIMARY KEY AUTOINCREMENT,
                dni        TEXT NOT NULL UNIQUE,
                nombre     TEXT NOT NULL,
                apellido   TEXT NOT NULL,
                email      TEXT UNIQUE,
                telefono   TEXT,
                estado     TEXT NOT NULL DEFAULT 'Activo',
                fecha_alta TEXT NOT NULL DEFAULT (datetime('now', 'localtime'))
            )",

            @"CREATE TABLE IF NOT EXISTS Empleados (
                id_empleado   INTEGER PRIMARY KEY AUTOINCREMENT,
                dni           TEXT NOT NULL UNIQUE,
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
                FOREIGN KEY (id_empleado) REFERENCES Empleados (id_empleado)
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
                metodo_pago  TEXT NOT NULL CHECK (metodo_pago IN ('Efectivo','Tarjeta','Transferencia')),
                estado       TEXT NOT NULL DEFAULT 'Pagado' CHECK (estado IN ('Pagado','Anulado')),
                FOREIGN KEY (id_membresia) REFERENCES Membresia (id_membresia)
                     ON UPDATE CASCADE ON DELETE RESTRICT
            )",

            @"CREATE TABLE IF NOT EXISTS Asistencias (
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
    private static void CargarTiposDeMembresia(SQLiteConnection conexion)
    {
        if (Contar(conexion, "Tipo_Membresias") > 0)
        {
            return;
        }

        // Cuota = precio base * (1 + recargo / 100). Los premium llevan 20% de recargo
        InsertarTipo(conexion, "Mensual", 25000, 30, 0);
        InsertarTipo(conexion, "Trimestral", 65000, 90, 0);
        InsertarTipo(conexion, "Anual", 220000, 365, 0);
        InsertarTipo(conexion, "Mensual Premium", 25000, 30, 20);
        InsertarTipo(conexion, "Trimestral Premium", 65000, 90, 20);
        InsertarTipo(conexion, "Anual Premium", 220000, 365, 20);
    }

    private static void InsertarTipo(SQLiteConnection conexion, string tipo, double precio, int dias, double recargo)
    {
        string sql = "INSERT INTO Tipo_Membresias (tipo, precio_base, duracion_dias, recargo_porcentaje) " +
                     "VALUES (@tipo, @precio, @dias, @recargo)";

        using (SQLiteCommand comando = new SQLiteCommand(sql, conexion))
        {
            comando.Parameters.AddWithValue("@tipo", tipo);
            comando.Parameters.AddWithValue("@precio", precio);
            comando.Parameters.AddWithValue("@dias", dias);
            comando.Parameters.AddWithValue("@recargo", recargo);
            comando.ExecuteNonQuery();
        }
    }

    // Si no hay ningún usuario, crea un admin y un recepcionista para poder entrar
    private static void CargarUsuariosIniciales(SQLiteConnection conexion)
    {
        if (Contar(conexion, "Usuario") > 0)
        {
            return;
        }

        int idAdmin = ObtenerOCrearEmpleado(conexion, "12345678", "Admin", "Sistema",
                                            "admin@gimnasio.com", "0000000000", "Administrador");
        InsertarUsuario(conexion, idAdmin, "admin", Seguridad.CalcularHash("admin123"), "admin");

        int idRecepcion = ObtenerOCrearEmpleado(conexion, "12345679", "Recepcion", "Gimnasio",
                                                "recepcion@gimnasio.com", "0000000001", "Recepcionista");
        InsertarUsuario(conexion, idRecepcion, "recepcion", Seguridad.CalcularHash("recep123"), "usuario");
    }

  private static int ObtenerOCrearEmpleado(SQLiteConnection conexion, string dni, string nombre,
        string apellido, string email, string telefono, string puesto)
    {
        string sqlInsertar = "INSERT OR IGNORE INTO Empleados (dni, nombre, apellido, email, telefono, puesto) " +
                             "VALUES (@dni, @nombre, @apellido, @email, @telefono, @puesto)";
        using (SQLiteCommand comando = new SQLiteCommand(sqlInsertar, conexion))
        {
            comando.Parameters.AddWithValue("@dni", dni);
            comando.Parameters.AddWithValue("@nombre", nombre);
            comando.Parameters.AddWithValue("@apellido", apellido);
            comando.Parameters.AddWithValue("@email", email);
            comando.Parameters.AddWithValue("@telefono", telefono);
            comando.Parameters.AddWithValue("@puesto", puesto);
            comando.ExecuteNonQuery();
        }
        using (SQLiteCommand comando = new SQLiteCommand("SELECT id_empleado FROM Empleados WHERE email = @email", conexion))
        {
            comando.Parameters.AddWithValue("@email", email);
            return Convert.ToInt32(comando.ExecuteScalar());
        }
    }

    private static void InsertarUsuario(SQLiteConnection conexion, int idEmpleado, string nombreUsuario,
                                        string contrasenaHash, string rol)// Inserta un nuevo usuario en su respectiva tabla. Se asume que el empleado ya esta cargado
    {
        string sql = "INSERT INTO Usuario (id_empleado, nombre_usuario, contrasenahash, rol) " +
                     "VALUES (@empleado, @usuario, @hash, @rol)";

        using (SQLiteCommand comando = new SQLiteCommand(sql, conexion))
        {
            comando.Parameters.AddWithValue("@empleado", idEmpleado);
            comando.Parameters.AddWithValue("@usuario", nombreUsuario);
            comando.Parameters.AddWithValue("@hash", contrasenaHash);
            comando.Parameters.AddWithValue("@rol", rol);
            comando.ExecuteNonQuery();
        }
    }

    // ---------- AYUDAS ----------
    private static void EjecutarSql(SQLiteConnection conexion, string sql)//ayuda a ejecutar un sql cualquiera, se asume que es correcto y no tiene parametros
    {
        using (SQLiteCommand comando = new SQLiteCommand(sql, conexion))
        {
            comando.ExecuteNonQuery();
        }
    }

    // Solo se usa con nombres de tabla escritos en este mismo archivo
    private static int Contar(SQLiteConnection conexion, string tabla)
    {
        using (SQLiteCommand comando = new SQLiteCommand("SELECT COUNT(*) FROM " + tabla, conexion))
        {
            return Convert.ToInt32(comando.ExecuteScalar());
        }
    }
}
