//  SERVICIO: ServicioSocios
//  Reglas de negocio de los socios. No imprime nada y no tiene SQL:
//  valida, decide y delega el guardado en el DAO.
//  Ante un problema lanza una excepción, y el menú la muestra.
public class ServicioSocio {
   private readonly ISocioDAO _sociodao;// DAO de socios,para delegar persistencia de datos
    public ServicioSocio(ISocioDAO socioDAO)
    {
        _sociodao = socioDAO;
    }
    //ALTA:
    // Si el DNI pertenece a un socio dado de baja, lo reactiva (conserva su historial).
    // "reactivado" le avisa al menú qué pasó.
    public Socio Alta(UsuarioBase actor, string dni, string nombre, string apellido,
                      string? email, string? telefono, out bool reactivado)
    {
        reactivado = false;

        // Crear el objeto valida todos los datos (lanza ArgumentException si algo está mal)
        Socio nuevo = new Socio(dni, nombre, apellido, email, telefono, DateTime.Now);

        Socio? existente = _sociodao.ObtenerPorDni(nuevo.Dni);//busca si ya existe un socio con ese dni

        if (existente == null)
        {
            _sociodao.Insertar(nuevo);
            LogService.Registrar(actor.Nombre_Usuario, "SOCIO_ALTA", "DNI " + nuevo.Dni);// si no existe, lo registra en el log que se dio de alta a un socio
            return nuevo;
        }

        if (existente.Activo)
        {
            throw new InvalidOperationException("Ya existe un socio activo con ese DNI (" +
                existente.Apellido + ", " + existente.Nombre + ").");//si existe un socio activo con ese dni, lanza una excepcion
        }

        // El DNI es de un socio dado de baja: se reactiva con los datos nuevos
        existente.ActualizarContacto(nuevo.Nombre, nuevo.Apellido, nuevo.Email, nuevo.Telefono);
        _sociodao.Actualizar(existente);
        _sociodao.Reactivar(existente.Id);
        existente.Reactivar();
        reactivado = true;

        LogService.Registrar(actor.Nombre_Usuario, "SOCIO_REACTIVADO", "DNI " + existente.Dni);//registra en el log que se reactivo un socio
        return existente;
    }
    public Socio? BuscarPorDni(string dni)
    {
        string limpio = Validaciones.ValidarDni(dni);   // lanza ArgumentException si el formato no sirve
        return _sociodao.ObtenerPorDni(limpio);
    }

    public List<Socio> BuscarPorApellido(string texto)
    {
        if (string.IsNullOrWhiteSpace(texto))
        {
            throw new ArgumentException("Ingresá al menos una letra del apellido.");//si el texto está vacío o es solo espacios, lanza una excepcion
        }
        return _sociodao.BuscarPorApellido(texto.Trim());
    }

    public List<Socio> Listar(bool soloActivos)//crea una lista de socios, si soloactivos = true, solo develve activos(false = devuelve todos)
    {
        return _sociodao.ObtenerTodos(soloActivos);
    }
    public Socio Modificar(UsuarioBase actor, int id ,string nombre,string apellido,string? email,string? telefono)
    {
        Socio socio = ObtenerPorId(id);
        if (!socio.Activo)
        {
            throw new InvalidOperationException("No se puede modificar un socio dado de baja.");
        }
        socio.ActualizarContacto(nombre, apellido, email, telefono);//actualiza los datos del socio, menos dni y fecha de registro( activo no se modifica aqui e id nunca se modifica

        if (!_sociodao.Actualizar(socio))
        {
            throw new InvalidOperationException("No se pudieron guardar los cambios.");
        }
        LogService.Registrar(actor.Nombre_Usuario, "SOCIO_MODIFICADO", "DNI " + socio.Dni);//registra en el log que se modifico un socio
        return socio;
    }
    public void DarDeBaja(UsuarioBase actor, int id)
    {
        if (!actor.PuedeDarDeBajaSocios)//se verifica que el usuario que quiere dar de baja tenga los permisos necesarios.
        {
            LogService.Registrar(actor.Nombre_Usuario, "ACCESO_DENEGADO", "Baja de socio");
            throw new UnauthorizedAccessException("No tenés permiso para dar de baja socios.");
        }
        Socio socio = ObtenerPorId(id);
        if (!socio.Activo)
        {
            throw new InvalidOperationException("El socio ya estaba dado de baja.");
        }
        socio.DarDeBaja();//se da de baja al socio y cambia su estado a inactivo
        if (!_sociodao.DarDeBaja(id))
        {
            throw new InvalidOperationException("No se pudo dar de baja al socio.");
        }
        LogService.Registrar(actor.Nombre_Usuario, "SOCIO_BAJA", "DNI " + socio.Dni);//registra en el log que se dio de baja a un socio
    }

   public Socio ObtenerPorId(int id)
    {
        Socio? socio = _sociodao.ObtenerPorId(id);
        if(socio == null)
        {
            throw new InvalidOperationException("No existe un socio con ese ID.");
        }
        return socio;
    }
}
