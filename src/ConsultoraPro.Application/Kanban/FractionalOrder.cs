namespace ConsultoraPro.Application.Kanban;

/// <summary>
/// Posicionamiento por "rango fraccional" (LexoRank simplificado, D6). Al insertar una
/// tarjeta/columna entre dos vecinas se calcula un orden intermedio, evitando reescribir
/// el resto de elementos en cada drag &amp; drop.
/// </summary>
public static class FractionalOrder
{
    public const double Step = 1000d;

    /// <summary>
    /// Devuelve un valor de orden situado entre <paramref name="before"/> (vecino anterior)
    /// y <paramref name="after"/> (vecino siguiente). Cualquiera puede ser null si la
    /// tarjeta se coloca en un extremo de la lista.
    /// </summary>
    public static double Between(double? before, double? after)
    {
        if (before is null && after is null)
            return Step;

        if (before is null)
            return after!.Value - Step;

        if (after is null)
            return before.Value + Step;

        return (before.Value + after.Value) / 2d;
    }
}
