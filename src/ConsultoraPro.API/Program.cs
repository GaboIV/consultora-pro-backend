using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using ConsultoraPro.API.Authorization;
using ConsultoraPro.API.Interfaces;
using ConsultoraPro.API.Middleware;
using ConsultoraPro.API.Services;
using ConsultoraPro.Application;
using ConsultoraPro.Application.Configuration;
using ConsultoraPro.Application.DTOs.Common;
using ConsultoraPro.Domain.Security;
using ConsultoraPro.Infrastructure;
using FluentValidation;
using FluentValidation.AspNetCore;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssembly(
    typeof(ConsultoraPro.Application.DependencyInjection).Assembly);

builder.Services.AddMemoryCache();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "ConsultoraPro API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddAutoMapper(typeof(ConsultoraPro.Application.Profiles.AutoMapperProfile));
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddScoped<IAuthService, AuthService>();

// Opciones de autenticación: sección "Auth" + fallback a las env vars planas
// (GOOGLE_CLIENT_ID / GOOGLE_CLIENT_SECRET / ALLOWED_DOMAINS) para reutilizar la misma
// configuración que ya existe en Outline. Se resuelve aquí una sola instancia porque el
// registro de los esquemas OAuth necesita los valores ya materializados al arrancar.
var authOptions = builder.Configuration.GetSection(AuthOptions.SectionName).Get<AuthOptions>() ?? new AuthOptions();
ApplyGoogleEnvFallback(authOptions, builder.Configuration);
builder.Services.AddSingleton(Options.Create(authOptions));

var authBuilder = builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            NameClaimType = "userId",
            RoleClaimType = "role",
            ClockSkew = TimeSpan.FromMinutes(1),
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
        };
        options.Events = new JwtBearerEvents
        {
            OnChallenge = async context =>
            {
                context.HandleResponse();
                await WriteAuthErrorResponseAsync(
                    context.Response,
                    StatusCodes.Status401Unauthorized,
                    "No autenticado o token inválido");
            },
            OnForbidden = async context =>
            {
                await WriteAuthErrorResponseAsync(
                    context.Response,
                    StatusCodes.Status403Forbidden,
                    "No tienes permiso para realizar esta acción");
            }
        };
    });

// Esquema OAuth de Google: solo se registra si está habilitado y configurado. El handler
// deposita los claims externos en la cookie temporal "External"; el AuthController los
// intercambia por el JWT propio de la app (mismo flujo de claims/permisos que el login local).
// CallbackPath va bajo /api para que el proxy Nginx del frontend lo reenvíe sin tocar su config.
if (authOptions.Google.Enabled && !string.IsNullOrWhiteSpace(authOptions.Google.ClientId))
{
    authBuilder
        .AddCookie(AuthSchemes.External, options =>
        {
            options.Cookie.Name = "ConsultoraPro.External";
            options.Cookie.SameSite = SameSiteMode.Lax;
            options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
            options.ExpireTimeSpan = TimeSpan.FromMinutes(10);
        })
        .AddGoogle(GoogleDefaults.AuthenticationScheme, options =>
        {
            options.ClientId = authOptions.Google.ClientId;
            options.ClientSecret = authOptions.Google.ClientSecret;
            options.SignInScheme = AuthSchemes.External;
            options.CallbackPath = "/api/auth/google/signin";
            options.SaveTokens = false;
            // Google no mapea la foto por defecto: la extraemos del payload y la añadimos como claim.
            options.Events.OnCreatingTicket = context =>
            {
                if (context.User.TryGetProperty("picture", out var picture)
                    && picture.ValueKind == JsonValueKind.String)
                {
                    context.Identity?.AddClaim(new Claim("urn:google:picture", picture.GetString()!));
                }
                return Task.CompletedTask;
            };
        });
}

builder.Services.AddAuthorization(options =>
{
    options.DefaultPolicy = new AuthorizationPolicyBuilder(JwtBearerDefaults.AuthenticationScheme)
        .RequireAuthenticatedUser()
        .Build();

    foreach (var permission in PermissionCatalog.All)
    {
        options.AddPolicy(permission.Clave, policy =>
        {
            policy.AuthenticationSchemes.Add(JwtBearerDefaults.AuthenticationScheme);
            policy.RequireAuthenticatedUser()
                  .AddRequirements(new PermissionRequirement(permission.Clave));
        });
    }
});
builder.Services.AddSingleton<IAuthorizationHandler, PermissionAuthorizationHandler>();

// Forwarded Headers: detras del Nginx del contenedor frontend y del Nginx del host,
// el backend recibe trafico HTTP. Estas cabeceras (X-Forwarded-For / -Proto) permiten
// que la app conozca la IP real del cliente y que el esquema sea https.
// Se limpian KnownNetworks/KnownProxies porque el backend NO se expone: solo es
// alcanzable a traves de la red interna de Docker (proxy de confianza), cuya IP no es
// fija. Sin esto, el middleware ignoraria las cabeceras de un proxy "desconocido".
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

var app = builder.Build();

// Debe ir PRIMERO en el pipeline, antes de CORS/Auth, para que el resto del middleware
// vea ya el esquema (https) y la IP del cliente corregidos.
app.UseForwardedHeaders();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();

// Solo el proveedor Local sirve archivos estáticos en /uploads. Con Azure Blob los bytes viven
// en el contenedor privado y se acceden vía URLs SAS firmadas, por lo que este middleware no aplica.
var storageOptions = app.Services.GetRequiredService<
    Microsoft.Extensions.Options.IOptions<ConsultoraPro.Application.Configuration.StorageOptions>>().Value;
if (string.Equals(storageOptions.Provider, "Local", StringComparison.OrdinalIgnoreCase))
{
    var uploadsDir = string.IsNullOrWhiteSpace(storageOptions.Local.RootPath)
        ? Path.Combine(Directory.GetCurrentDirectory(), "uploads")
        : storageOptions.Local.RootPath;
    if (!Directory.Exists(uploadsDir))
    {
        Directory.CreateDirectory(uploadsDir);
    }
    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(uploadsDir),
        RequestPath = "/uploads"
    });
}

app.UseMiddleware<GlobalExceptionHandler>();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

await app.Services.InitializeDatabaseAsync();
app.Run();

static async Task WriteAuthErrorResponseAsync(HttpResponse response, int statusCode, string message)
{
    if (response.HasStarted)
        return;

    response.ContentType = "application/json";
    response.StatusCode = statusCode;

    var payload = new ApiResponse<object>
    {
        Success = false,
        Message = message
    };

    await response.WriteAsync(JsonSerializer.Serialize(payload, new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    }));
}

// Si la sección "Auth:Google" no trae credenciales, las toma de las env vars planas que
// ya usas en Outline (GOOGLE_CLIENT_ID / GOOGLE_CLIENT_SECRET / ALLOWED_DOMAINS).
static void ApplyGoogleEnvFallback(AuthOptions options, IConfiguration config)
{
    if (string.IsNullOrWhiteSpace(options.Google.ClientId))
        options.Google.ClientId = config["GOOGLE_CLIENT_ID"] ?? string.Empty;

    if (string.IsNullOrWhiteSpace(options.Google.ClientSecret))
        options.Google.ClientSecret = config["GOOGLE_CLIENT_SECRET"] ?? string.Empty;

    if (string.IsNullOrWhiteSpace(options.Google.AllowedDomains))
        options.Google.AllowedDomains = config["ALLOWED_DOMAINS"] ?? string.Empty;
}
