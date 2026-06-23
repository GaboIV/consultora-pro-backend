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
    public const string Soporte = "Soporte";

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
        new(25, "roles.editar", "Editar roles", "Roles", "Permite modificar roles, usuarios and permisos."),
        new(26, "roles.eliminar", "Eliminar roles", "Roles", "Permite eliminar roles y usuarios."),
        new(27, "roles.asignar", "Asignar roles", "Roles", "Permite asignar roles a usuarios."),

        new(28, "kanban.ver", "Ver kanban", "Kanban", "Permite consultar tableros y tarjetas."),
        new(29, "kanban.crear", "Crear en kanban", "Kanban", "Permite crear tableros y tarjetas."),
        new(30, "kanban.editar", "Editar kanban", "Kanban", "Permite editar columnas, tarjetas, mover y asignar."),
        new(31, "kanban.comentar", "Comentar tarjetas", "Kanban", "Permite comentar en tarjetas."),
        new(32, "kanban.eliminar", "Eliminar en kanban", "Kanban", "Permite eliminar/archivar tableros y tarjetas."),
        new(33, "kanban.gestionar", "Gestionar tableros", "Kanban", "Permite administrar miembros y configuración del tablero."),

        new(34, "repositorios.ver", "Ver repositorios", "Repositorios", "Permite consultar listado de repositorios."),
        new(35, "repositorios.editar", "Editar repositorios", "Repositorios", "Permite modificar repositorios."),

        new(36, "equipo.asignar-proyectos", "Asignar proyectos", "Equipo", "Permite definir a qué proyectos accede cada miembro sin acceso total."),

        // --- Ámbito por módulo: "ver todos" amplía el alcance base "ver" (que equivale a "ver asignados"). ---
        new(37, "clientes.ver.todos", "Ver todos los clientes", "Clientes", "Amplía el alcance a todos los clientes, no solo los de proyectos asignados."),
        new(38, "proyectos.ver.todos", "Ver todos los proyectos", "Proyectos", "Amplía el alcance a todos los proyectos, no solo los asignados."),
        new(39, "ambientes.ver.todos", "Ver todos los ambientes", "Ambientes", "Amplía el alcance a los ambientes de todos los proyectos."),
        new(40, "repositorios.ver.todos", "Ver todos los repositorios", "Repositorios", "Amplía el alcance a los repositorios de todos los proyectos."),

        // --- Credenciales: niveles excluyentes y rankeados (full ⊃ ver-todo ⊃ básico). ---
        new(41, "credenciales.nivel.full", "Credenciales: acceso total", "Credenciales", "Ver, revelar, crear, editar y aprobar solicitudes de credenciales."),
        new(42, "credenciales.nivel.ver-todo", "Credenciales: ver todo", "Credenciales", "Ver todos los datos de credenciales incluyendo secretos, sin crear ni editar."),
        new(43, "credenciales.nivel.basico", "Credenciales: datos básicos", "Credenciales", "Ver datos no sensibles; para revelar secretos debe enviar una solicitud."),
        new(44, "credenciales.solicitud.aprobar", "Aprobar solicitudes de revelación", "Credenciales", "Permite resolver solicitudes de revelación de secretos."),

        // --- Screenshots (antes sin permiso propio). ---
        new(45, "screenshots.ver", "Ver screenshots", "Screenshots", "Permite ver las capturas de un proyecto."),
        new(46, "screenshots.editar", "Editar screenshots", "Screenshots", "Permite agregar o eliminar capturas de un proyecto."),

        // --- Usuarios (separado de Roles). ---
        new(47, "usuarios.ver", "Ver usuarios", "Usuarios", "Permite consultar el listado de usuarios."),
        new(48, "usuarios.editar", "Editar usuarios", "Usuarios", "Permite crear y modificar usuarios."),
        new(49, "usuarios.cambiar-password", "Cambiar contraseña", "Usuarios", "Permite restablecer la contraseña de otros usuarios."),
        new(50, "usuarios.eliminar", "Eliminar usuarios", "Usuarios", "Permite eliminar o desactivar usuarios."),
        new(51, "usuarios.asignar-proyectos", "Asignar proyectos a usuarios", "Usuarios", "Permite definir a qué proyectos accede cada usuario sin acceso total."),

        // --- Tipos de Solución (catálogo de tipos de proyecto). ---
        new(52, "tipos-solucion.ver", "Ver tipos de solución", "Tipos de Solución", "Permite consultar el catálogo de tipos de solución."),
        new(53, "tipos-solucion.crear", "Crear tipos de solución", "Tipos de Solución", "Permite crear tipos de solución."),
        new(54, "tipos-solucion.editar", "Editar tipos de solución", "Tipos de Solución", "Permite modificar tipos de solución."),
        new(55, "tipos-solucion.eliminar", "Eliminar tipos de solución", "Tipos de Solución", "Permite eliminar tipos de solución sin proyectos asociados.")
    ];

    /// <summary>
    /// Implicaciones del catálogo: conceder la clave izquierda otorga también las de la derecha.
    /// La expansión transitiva la resuelve <see cref="PermissionExpander"/> en un único lugar, de modo
    /// que servidor y cliente nunca discrepan. Permite que las claves nuevas satisfagan las policies
    /// existentes (compatibilidad) y modela los niveles rankeados de Credenciales.
    /// </summary>
    public static readonly IReadOnlyDictionary<string, IReadOnlyList<string>> Implies =
        new Dictionary<string, IReadOnlyList<string>>(StringComparer.OrdinalIgnoreCase)
        {
            // "ver todos" implica el acceso base "ver" (= ver asignados).
            ["clientes.ver.todos"] = ["clientes.ver"],
            ["proyectos.ver.todos"] = ["proyectos.ver"],
            ["ambientes.ver.todos"] = ["ambientes.ver"],
            ["repositorios.ver.todos"] = ["repositorios.ver"],

            // Niveles de credenciales: cada nivel implica el inferior y las acciones legacy equivalentes.
            ["credenciales.nivel.full"] = ["credenciales.nivel.ver-todo", "credenciales.crear", "credenciales.editar", "credenciales.solicitud.aprobar"],
            ["credenciales.nivel.ver-todo"] = ["credenciales.nivel.basico", "credenciales.revelar"],
            ["credenciales.nivel.basico"] = ["credenciales.ver"],

            // Acciones que implican poder ver.
            ["screenshots.editar"] = ["screenshots.ver"],
            ["usuarios.editar"] = ["usuarios.ver"],
            ["usuarios.cambiar-password"] = ["usuarios.ver"],
            ["usuarios.eliminar"] = ["usuarios.ver"]
        };

    /// <summary>Módulos cuyo alcance "ver todos" se configura con la clave <c>&lt;modulo&gt;.ver.todos</c>.</summary>
    public static readonly IReadOnlyList<string> ScopedModules = ["clientes", "proyectos", "ambientes", "repositorios"];

    /// <summary>Roles cuyos usuarios acceden a todos los proyectos sin asignación explícita.</summary>
    public static readonly IReadOnlySet<string> FullProjectAccessRoles =
        new HashSet<string>(StringComparer.OrdinalIgnoreCase) { Gerencia, Arquitecto, LT };

    /// <summary>Roles creados por el seeder; sus flags estructurales no se pueden editar.</summary>
    public static readonly IReadOnlySet<string> SystemRoles =
        new HashSet<string>(StringComparer.OrdinalIgnoreCase) { Gerencia, Arquitecto, LT, Dev, Soporte };

    public static readonly IReadOnlyDictionary<string, IReadOnlySet<string>> RolePermissions =
        new Dictionary<string, IReadOnlySet<string>>(StringComparer.OrdinalIgnoreCase)
        {
            [Gerencia] = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "clientes.ver",
                "clientes.ver.todos",
                "proyectos.ver",
                "proyectos.ver.todos",
                "ambientes.ver",
                "ambientes.ver.todos",
                "despliegues.ver",
                "despliegues.historial",
                "equipo.ver",
                "equipo.asignar-proyectos",
                "roles.ver",
                "usuarios.ver",
                "kanban.ver",
                "kanban.comentar",
                "repositorios.ver",
                "repositorios.ver.todos",
                "screenshots.ver",
                "tipos-solucion.ver"
            },
            [Arquitecto] = All.Select(permission => permission.Clave).ToHashSet(StringComparer.OrdinalIgnoreCase),
            [LT] = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "clientes.ver",
                "clientes.ver.todos",
                "proyectos.ver",
                "proyectos.ver.todos",
                "proyectos.editar",
                "ambientes.ver",
                "ambientes.ver.todos",
                "ambientes.crear",
                "ambientes.editar",
                "credenciales.nivel.ver-todo",
                "despliegues.ver",
                "despliegues.ejecutar",
                "despliegues.historial",
                "equipo.ver",
                "equipo.asignar-proyectos",
                "kanban.ver",
                "kanban.crear",
                "kanban.editar",
                "kanban.comentar",
                "kanban.eliminar",
                "kanban.gestionar",
                "repositorios.ver",
                "repositorios.ver.todos",
                "repositorios.editar",
                "screenshots.ver",
                "screenshots.editar",
                "tipos-solucion.ver"
            },
            [Dev] = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "clientes.ver",
                "proyectos.ver",
                "ambientes.ver",
                "despliegues.ver",
                "despliegues.historial",
                "equipo.ver",
                "kanban.ver",
                "kanban.crear",
                "kanban.editar",
                "kanban.comentar",
                "repositorios.ver",
                "screenshots.ver"
            },
            [Soporte] = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "clientes.ver",
                "proyectos.ver",
                "ambientes.ver",
                "ambientes.editar",
                "equipo.ver",
                "kanban.ver",
                "kanban.crear",
                "kanban.editar",
                "kanban.comentar",
                "kanban.eliminar",
                "kanban.gestionar",
                "screenshots.ver",
                "screenshots.editar"
            }
        };

    public static readonly IReadOnlyDictionary<string, string> RoleDescriptions =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            [Gerencia] = "Visibilidad ejecutiva sin acceso a secretos ni operaciones críticas.",
            [Arquitecto] = "Máximo nivel técnico con acceso completo al portal.",
            [LT] = "Liderazgo técnico con permisos operativos acotados.",
            [Dev] = "Desarrollador con acceso de consulta a proyectos y equipo.",
            [Soporte] = "Soporte técnico con accesos restringidos a sus proyectos asignados."
        };
}
