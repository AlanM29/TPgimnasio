var hoy = DateTime.Today;

var admin = new Admin("admin", "hash1", "Ana", "Pérez", "ana@mail.com", "341-555-1234", hoy);
var recep = new Recepcionista("lucia", "hash2", "Lucía", "Gómez", null, null, hoy);
foreach (UsuarioBase u in new List<UsuarioBase> { admin, recep })
    Console.WriteLine($"{u.Nombre_Usuario} | {u.Nombre} {u.Apellido} | {u.Rol} | " +
                      $"Baja:{u.PuedeDarDeBajaSocios} Precios:{u.PuedeModificarPrecios} " +
                      $"Usuarios:{u.PuedeGestionarUsuarios} Exportar:{u.PuedeExportar} | " +
                      $"Activo:{u.Activo} | Id:{u.Id}");
recep.DarDeBaja();
recep.AsignarId(7);
Console.WriteLine($"Tras baja: Activo={recep.Activo}, Id={recep.Id}");

// Prueba 4: activo e id por constructor (parámetros con nombre)
var inactivo = new Recepcionista("pedro", "h", "Pedro", "Díaz", null, null, hoy, activo: false, id: 5);
Console.WriteLine($"Constructor completo: Activo={inactivo.Activo}, Id={inactivo.Id}");

void Probar(string descripcion, Action accion)
{
    try { accion(); Console.WriteLine($"{descripcion}: ACEPTADO"); }
    catch (ArgumentException ex) { Console.WriteLine($"{descripcion}: {ex.Message}"); }
}

Probar("Usuario vacío", () => new Admin("", "h", "Ana", "Pérez", null, null, hoy));
Probar("Usuario corto", () => new Admin("ab", "h", "Ana", "Pérez", null, null, hoy));
Probar("Usuario con espacio", () => new Admin("ana lopez", "h", "Ana", "Pérez", null, null, hoy));
Probar("Hash vacío", () => new Admin("anita", "", "Ana", "Pérez", null, null, hoy));
Probar("Nombre vacío", () => new Admin("anita", "h", "", "Pérez", null, null, hoy));
Probar("Apellido vacío", () => new Admin("anita", "h", "Ana", "", null, null, hoy));
Probar("Email inválido", () => new Admin("anita", "h", "Ana", "Pérez", "abc", null, hoy));
Probar("Teléfono inválido", () => new Admin("anita", "h", "Ana", "Pérez", null, "abc", hoy));
Probar("Datos válidos", () => new Admin("anita", "h", "Ana", "Pérez", "a@b.com", "+54 341 555", hoy));

// Prueba 6: fábrica
Console.WriteLine(UsuarioFactory.Crear("Admin", "root", "h", "Root", "Admin", null, null, hoy).GetType().Name);
Console.WriteLine(UsuarioFactory.Crear("Recepcionista", "recep1", "h", "Rita", "López", null, null, hoy).GetType().Name);
var desdeBase = UsuarioFactory.Crear("Recepcionista", "recep2", "h", "Rita", "López", null, null, hoy, activo: false, id: 9);
Console.WriteLine($"Fábrica completa: Activo={desdeBase.Activo}, Id={desdeBase.Id}");
Probar("Rol inventado", () => UsuarioFactory.Crear("Gerente", "gerente", "h", "G", "H", null, null, hoy));
