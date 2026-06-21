using Microsoft.AspNetCore.Authorization;

namespace ConsultoraPro.API.Authorization;

public sealed class ModuloOperationRequirement : IAuthorizationRequirement
{
    public ModuloOperationRequirement(string modulo, string clave)
    {
        Modulo = modulo;
        Clave = clave;
    }

    public string Modulo { get; }
    public string Clave { get; }
}
