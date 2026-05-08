# ConsultoraPro — Backend API

API REST en .NET 8 con arquitectura en capas para la gestión de proyectos de una consultora de software.  
MySQL + Entity Framework Core + ASP.NET Core Identity + JWT.

---

## Stack

| Tecnología | Versión |
|---|---|
| .NET | 8.0 |
| Entity Framework Core | 8.0 |
| Pomelo.EntityFrameworkCore.MySql | 8.0 |
| ASP.NET Core Identity | 8.0 |
| JWT Bearer | 8.0 |
| AutoMapper | 12.0 |
| FluentValidation | 11.8 |
| Swashbuckle (Swagger) | 6.5 |

---

## Estructura

```
ConsultoraPro.Backend/
├── ConsultoraPro.slnx
├── src/
│   ├── global.json
│   ├── ConsultoraPro.Domain/          # Entidades, enums, interfaces de repositorio
│   ├── ConsultoraPro.Application/     # DTOs, servicios, validadores, perfiles AutoMapper
│   ├── ConsultoraPro.Infrastructure/  # EF Core DbContext, repositorios, Identity, migraciones, seed
│   └── ConsultoraPro.API/            # Controladores, middleware, auth JWT, Swagger, CORS
└── README.md
```

### Capas

- **Domain** — `Cliente`, `Proyecto`, `EstadoProyecto`, `EtapaProyecto`, `IClienteRepository`, `IProyectoRepository`
- **Application** — `ClienteDto`, `ProyectoDto`, `CreateClienteDto`, `UpdateClienteDto`, `CreateProyectoDto`, `UpdateProyectoDto`, `ApiResponse<T>`, DTOs de Auth, validadores FluentValidation, perfil AutoMapper, servicios de aplicación, DependencyInjection
- **Infrastructure** — `AppDbContext` (hereda de `IdentityDbContext`), repositorios con EF Core, migración inicial, `DataSeeder` con 3 clientes + 4 proyectos, DependencyInjection
- **API** — `ClientesController`, `ProyectosController`, `AuthController`, `GlobalExceptionHandler`, `AuthService` (JWT), `Program.cs` con CORS, Swagger, JWT config, FluentValidation, AutoMapper

---

## Requisitos

- .NET 8 SDK
- MySQL 8 corriendo en `localhost:3306`

---

## Configuración

La connection string está en `src/ConsultoraPro.API/appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Port=3306;Database=consultorapro_dev;User=root;Password=root;"
  }
}
```

Si tu MySQL usa otro user/password, edítalo ahí.

---

## Endpoints

### Clientes
| Método | Ruta | Descripción |
|---|---|---|
| GET | `/api/clientes` | Lista todos los activos |
| GET | `/api/clientes/{id}` | Detalle + proyectos del cliente |
| POST | `/api/clientes` | Crear cliente |
| PUT | `/api/clientes/{id}` | Editar cliente |
| DELETE | `/api/clientes/{id}` | Soft delete (Activo = false) |

### Proyectos
| Método | Ruta | Descripción |
|---|---|---|
| GET | `/api/proyectos` | Lista todos con nombre del cliente |
| GET | `/api/proyectos/{id}` | Detalle del proyecto |
| GET | `/api/proyectos/cliente/{clienteId}` | Proyectos de un cliente |
| POST | `/api/proyectos` | Crear proyecto |
| PUT | `/api/proyectos/{id}` | Editar proyecto |
| DELETE | `/api/proyectos/{id}` | Eliminar proyecto |

### Auth
| Método | Ruta | Descripción |
|---|---|---|
| POST | `/api/auth/register` | Registrar usuario (con rol) |
| POST | `/api/auth/login` | Login retorna JWT |

> Los endpoints de Clientes y Proyectos son públicos (sin `[Authorize]`) para facilitar el desarrollo.

---

## Ejecutar

```bash
# 1. Restaurar paquetes
dotnet restore

# 2. Crear la base de datos
dotnet ef database update --project src/ConsultoraPro.Infrastructure --startup-project src/ConsultoraPro.API

# 3. Correr
dotnet run --project src/ConsultoraPro.API --launch-profile https
```

Swagger disponible en `https://localhost:7001/swagger`.

---

## Seed data

Al iniciar por primera vez, la BD se puebla automáticamente con:

**Clientes:** Repsol (azul), Telefónica (púrpura), BBVA (verde)  
**Proyectos:** Plataforma Digital Upstream, App Mi Movistar, Banca Digital Empresas, CRM Comercial Repsol

---

## Roles (Identity)

| Rol | Descripción |
|---|---|
| Gerencia | Visibilidad total sin edición técnica |
| Arquitecto | Control técnico completo |
| LT | Gestión de proyectos asignados |
| Dev | Acceso operativo limitado |

Los roles se crean automáticamente al registrar un usuario con `POST /api/auth/register`.

---

## Respuesta API

Todas las respuestas usan `ApiResponse<T>`:

```json
{
  "success": true,
  "data": { ... },
  "message": "OK",
  "errors": []
}
```

Errores controlados retornan el mismo formato con `success: false`.

---

## CORS

Habilitado para `http://localhost:4200` (Angular dev). Editar en `Program.cs` si se necesita otro origen.
