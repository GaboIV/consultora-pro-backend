namespace ConsultoraPro.Domain.Security;

/// <summary>
/// Expande un conjunto de permisos concedidos a su cierre transitivo según
/// <see cref="PermissionCatalog.Implies"/>. Es el único lugar donde se aplican las implicaciones,
/// de modo que la emisión del JWT, la autorización por policy y el frontend operen sobre el mismo
/// conjunto efectivo de claves.
/// </summary>
public static class PermissionExpander
{
    public static IReadOnlySet<string> Expand(IEnumerable<string> granted)
    {
        var result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var pending = new Stack<string>(granted);

        while (pending.Count > 0)
        {
            var key = pending.Pop();
            if (!result.Add(key))
                continue;

            if (PermissionCatalog.Implies.TryGetValue(key, out var implied))
            {
                foreach (var implication in implied)
                    pending.Push(implication);
            }
        }

        return result;
    }
}
