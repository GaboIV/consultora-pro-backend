namespace ConsultoraPro.Domain.Security;

public sealed record PermissionDefinition(
    int Id,
    string Clave,
    string Nombre,
    string Modulo,
    string Descripcion);

public static class PermissionCatalog
{
    public const string Gerencia = "Gerencia";
    public const string Arquitecto = "Arquitecto";
    public const string LT = "LT";
    public const string Dev = "Dev";

    public static readonly IReadOnlyList<PermissionDefinition> All =
    [
        new(1, "clientes.ver", "Ver clientes", "Clientes", "Permite consultar clientes."),
        new(2, "clientes.crear", "Crear clientes", "Clientes", "Permite crear clientes."),
        new(3, "clientes.editar", "Editar clientes", "Clientes", "Permite modificar clientes."),
        new(4, "clientes.eliminar", "Eliminar clientes", "Clientes", "Permite desactivar clientes."),

        new(5, "proyectos.ver", "Ver proyectos", "Proyectos", "Permite consultar proyectos."),
        new(6, "proyectos.crear", "Crear proyectos", "Proyectos", "Permite crear proyectos."),
        new(7, "proyectos.editar", "Editar proyectos", "Proyectos", "Permite modificar proyectos."),
        new(8, "proyectos.eliminar", "Eliminar proyectos", "Proyectos", "Permite eliminar proyectos."),

        new(9, "ambientes.ver", "Ver ambientes", "Ambientes", "Permite consultar ambientes."),
        new(10, "ambientes.crear", "Crear ambientes", "Ambientes", "Permite crear ambientes."),
        new(11, "ambientes.editar", "Editar ambientes", "Ambientes", "Permite modificar ambientes, componentes, usuarios de prueba y recursos en la nube."),

        new(12, "credenciales.ver", "Ver credenciales", "Credenciales", "Permite consultar el listado de credenciales."),
        new(13, "credenciales.revelar", "Revelar credenciales", "Credenciales", "Permite revelar valores sensibles."),
        new(14, "credenciales.crear", "Crear credenciales", "Credenciales", "Permite crear credenciales."),
        new(15, "credenciales.editar", "Editar credenciales", "Credenciales", "Permite modificar credenciales."),

        new(16, "despliegues.ver", "Ver despliegues", "Despliegues", "Permite consultar despliegues."),
        new(17, "despliegues.ejecutar", "Ejecutar despliegues", "Despliegues", "Permite ejecutar despliegues."),
        new(18, "despliegues.historial", "Ver historial", "Despliegues", "Permite consultar historial de despliegues."),

        new(19, "equipo.ver", "Ver equipo", "Equipo", "Permite consultar miembros del equipo."),
        new(20, "equipo.crear", "Crear miembros", "Equipo", "Permite crear miembros del equipo."),
        new(21, "equipo.editar", "Editar miembros", "Equipo", "Permite modificar miembros del equipo."),
        new(22, "equipo.eliminar", "Eliminar miembros", "Equipo", "Permite eliminar miembros del equipo."),

        new(23, "roles.ver", "Ver roles", "Roles", "Permite consultar usuarios, roles y permisos."),
        new(24, "roles.crear", "Crear roles", "Roles", "Permite crear roles y usuarios."),
        new(25, "roles.editar", "Editar roles", "Roles", "Permite modificar roles, usuarios y permisos."),
        new(26, "roles.eliminar", "Eliminar roles", "Roles", "Permite eliminar roles y usuarios."),
        new(27, "roles.asignar", "Asignar roles", "Roles", "Permite asignar roles a usuarios.")
    ];

    public static readonly IReadOnlyDictionary<string, IReadOnlySet<string>> RolePermissions =
        new Dictionary<string, IReadOnlySet<string>>(StringComparer.OrdinalIgnoreCase)
        {
            [Gerencia] = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "clientes.ver",
                "proyectos.ver",
                "ambientes.ver",
                "despliegues.ver",
                "despliegues.historial",
                "equipo.ver",
                "roles.ver"
            },
            [Arquitecto] = All.Select(permission => permission.Clave).ToHashSet(StringComparer.OrdinalIgnoreCase),
            [LT] = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "clientes.ver",
                "proyectos.ver",
                "proyectos.editar",
                "ambientes.ver",
                "ambientes.crear",
                "ambientes.editar",
                "credenciales.ver",
                "credenciales.revelar",
                "despliegues.ver",
                "despliegues.ejecutar",
                "despliegues.historial",
                "equipo.ver"
            },
            [Dev] = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "clientes.ver",
                "proyectos.ver",
                "ambientes.ver",
                "despliegues.ver",
                "despliegues.historial",
                "equipo.ver"
            }
        };

    public static readonly IReadOnlyDictionary<string, string> RoleDescriptions =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            [Gerencia] = "Visibilidad ejecutiva sin acceso a secretos ni operaciones críticas.",
            [Arquitecto] = "Máximo nivel técnico con acceso completo al portal.",
            [LT] = "Liderazgo técnico con permisos operativos acotados.",
            [Dev] = "Desarrollador con acceso de consulta a proyectos y equipo."
        };
}
