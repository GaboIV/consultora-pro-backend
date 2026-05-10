# ConsultoraPro - Documentacion completa del backend

Este documento describe el alcance actual del backend de ConsultoraPro: arquitectura, capas, entidades, endpoints, autenticacion, permisos, semillas, flujos de negocio, contratos y puntos pendientes.

## Vision General

El backend es una API REST construida en .NET 8 para administrar una consultora de software. Cubre:

- Autenticacion con JWT.
- Usuarios internos basados en ASP.NET Core Identity.
- Roles y permisos granulares por modulo.
- Clientes.
- Proyectos.
- Tipos de solucion.
- Asignacion de usuarios a proyectos como desarrolladores principales o de apoyo.
- Snapshot consolidado para el frontend.
- Catalogo inicial de credenciales protegido por permisos.
- Seed automatico de datos base para desarrollo.
- Migraciones automaticas con Entity Framework Core.

## Stack Tecnico

- .NET 8.
- ASP.NET Core Web API.
- ASP.NET Core Identity con usuarios y roles personalizados.
- JWT Bearer Authentication.
- Entity Framework Core.
- Pomelo.EntityFrameworkCore.MySql.
- MySQL 8.
- AutoMapper.
- FluentValidation.
- Swagger/OpenAPI.
- Middleware global de excepciones.

## Estructura de Proyectos

```text
backend/
├── ConsultoraPro.slnx
├── README.md
├── SISTEMA_BACKEND.md
└── src/
    ├── global.json
    ├── ConsultoraPro.Domain/
    ├── ConsultoraPro.Application/
    ├── ConsultoraPro.Infrastructure/
    └── ConsultoraPro.API/
```

## Arquitectura Por Capas

### Domain

Contiene el nucleo del dominio:

- Entidades persistentes.
- Enums de negocio.
- Interfaces de repositorio.
- Catalogo central de permisos.

Archivos clave:

- `Models/Cliente.cs`
- `Models/Proyecto.cs`
- `Models/TipoSolucion.cs`
- `Models/ApplicationUser.cs`
- `Models/ApplicationRole.cs`
- `Models/Permiso.cs`
- `Models/RolPermiso.cs`
- `Models/ProyectoMiembro.cs`
- `Enums/EtapaProyecto.cs`
- `Enums/EstadoProyecto.cs`
- `Enums/RolDesarrollador.cs`
- `Security/PermissionCatalog.cs`
- `Interfaces/IClienteRepository.cs`
- `Interfaces/IProyectoRepository.cs`
- `Interfaces/ITipoSolucionRepository.cs`

### Application

Contiene la logica de aplicacion:

- DTOs.
- Servicios.
- Interfaces de servicios.
- Validadores.
- Perfil de AutoMapper.
- Registro de dependencias de aplicacion.

Responsabilidades:

- Orquestar operaciones de clientes y proyectos.
- Mapear entidades a contratos de API.
- Generar el snapshot que consume el frontend.
- Validar comandos de creacion/edicion.
- Mantener reglas de negocio de proyectos, clientes y miembros.

### Infrastructure

Contiene persistencia e infraestructura:

- `AppDbContext`.
- Configuracion EF Core.
- Identity stores.
- Repositorios concretos.
- Migraciones.
- Seeds de datos y seguridad.
- Registro de DbContext, Identity y repositorios.

Responsabilidades:

- Conectar a MySQL.
- Ejecutar migraciones al inicio.
- Crear permisos, roles, usuario base, clientes, tipos de solucion, usuarios y proyectos de ejemplo.
- Implementar acceso a datos con EF Core.

### API

Contiene la capa HTTP:

- Controladores REST.
- Autenticacion JWT.
- Autorizacion por permisos.
- Swagger.
- CORS.
- Middleware global de errores.
- Servicio de autenticacion.

Responsabilidades:

- Exponer endpoints.
- Aplicar autorizacion por policy.
- Emitir tokens JWT.
- Responder con contratos normalizados.
- Iniciar base de datos y seeds al arrancar.

## Configuracion Principal

### Base de datos

La connection string se lee desde `appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Port=3306;Database=consultorapro_dev;User=root;Password=root;"
  }
}
```

### JWT

El JWT se configura en `Program.cs`:

- Valida issuer.
- Valida audience.
- Valida expiracion.
- Valida firma.
- Usa `userId` como `NameClaimType`.
- Usa `role` como `RoleClaimType`.
- Usa `ClockSkew` de 1 minuto.
- El token expira a las 8 horas.

Claims emitidos:

- `sub`
- `nameidentifier`
- `userId`
- `email`
- `nombres`
- `apellidos`
- `iniciales`
- `puesto`
- `role`
- `permisos`

### CORS

Permite el origen:

```text
http://localhost:4200
```

### Swagger

Disponible en desarrollo. Incluye definicion Bearer para probar endpoints protegidos.

## Modelo De Datos

### ApplicationUser

Usuario interno del sistema. Extiende Identity.

Cubre:

- Nombres.
- Apellidos.
- Telefono.
- Iniciales.
- Puesto.
- Estado activo/inactivo.
- Fecha de alta.
- Ultimo acceso.
- Proyectos asignados.

Notas:

- El email normalizado es unico.
- Los usuarios inactivos no pueden iniciar sesion.
- En creacion/edicion desde el modulo de usuarios, el puesto ya no se pide en el formulario; se deriva del rol seleccionado.

### ApplicationRole

Rol de Identity extendido.

Cubre:

- Nombre.
- Descripcion.
- Estado activo.
- Relacion con permisos concedidos.

### Permiso

Permiso granular del sistema.

Cubre:

- Id fijo.
- Clave tecnica, por ejemplo `clientes.ver`.
- Nombre legible.
- Modulo.
- Descripcion.

### RolPermiso

Relacion entre rol y permiso.

Cubre:

- RolId.
- PermisoId.
- Indicador `Concedido`.

La clave primaria es compuesta: `RolId + PermisoId`.

### Cliente

Cliente de la consultora.

Cubre:

- Nombre.
- Industria.
- Iniciales.
- Color visual.
- Estado activo.
- Fecha de alta.
- Total de proyectos.
- Relacion con proyectos.

La eliminacion de clientes es logica: se marca `Activo = false`.

### TipoSolucion

Catalogo de tipos de solucion ofrecidos.

Ejemplos sembrados:

- Portal de Proveedores.
- Facturacion Electronica.
- Host2Host.
- Guias de Remision.

### Proyecto

Proyecto asociado a un cliente y a un tipo de solucion.

Cubre:

- Nombre.
- Cliente.
- Tipo de solucion.
- Etapa.
- Estado.
- Progreso.
- Fecha inicio.
- Fecha fin.
- Total de miembros.
- Timestamps.
- Miembros asignados.

### ProyectoMiembro

Asignacion de usuarios a proyectos.

Cubre:

- Usuario.
- Proyecto.
- Rol dentro del proyecto: `Principal` o `Apoyo`.
- Fecha de asignacion.

Restriccion:

- Un usuario no puede estar duplicado dentro del mismo proyecto.

## Enums De Negocio

### EtapaProyecto

Representa la fase operativa del proyecto:

- `Analisis`
- `Diseno`
- `Desarrollo`
- `QA`
- `Deploy`
- `Soporte`

### EstadoProyecto

Representa el estado general:

- `Planificacion`
- `EnCurso`
- `Completado`
- `PorVencer`

### RolDesarrollador

Rol del usuario dentro de un proyecto:

- `Principal`
- `Apoyo`

## Seguridad Y Permisos

El sistema usa autorizacion por policies. Cada permiso del catalogo se registra como policy en `Program.cs`.

### Roles base

- `Gerencia`
- `Arquitecto`
- `LT`
- `Dev`

### Catalogo de permisos

Clientes:

- `clientes.ver`
- `clientes.crear`
- `clientes.editar`
- `clientes.eliminar`

Proyectos:

- `proyectos.ver`
- `proyectos.crear`
- `proyectos.editar`
- `proyectos.eliminar`

Ambientes:

- `ambientes.ver`
- `ambientes.crear`
- `ambientes.editar`

Credenciales:

- `credenciales.ver`
- `credenciales.revelar`
- `credenciales.crear`
- `credenciales.editar`

Despliegues:

- `despliegues.ver`
- `despliegues.ejecutar`
- `despliegues.historial`

Equipo:

- `equipo.ver`
- `equipo.crear`
- `equipo.editar`
- `equipo.eliminar`

Roles:

- `roles.ver`
- `roles.crear`
- `roles.editar`
- `roles.eliminar`
- `roles.asignar`

### Matriz base de roles

`Arquitecto`:

- Tiene todos los permisos.

`Gerencia`:

- Consulta clientes, proyectos, ambientes, despliegues, historial, equipo y roles.
- No tiene acceso a secretos ni operaciones criticas por defecto.

`LT`:

- Consulta clientes y proyectos.
- Edita proyectos.
- Consulta/crea/edita ambientes.
- Consulta y revela credenciales.
- Consulta y ejecuta despliegues.
- Consulta historial.
- Consulta equipo.

`Dev`:

- Consulta clientes, proyectos, ambientes, despliegues, historial y equipo.

## Autorizacion

`PermissionAuthorizationHandler` lee el claim `permisos`.

Soporta:

- Claim como valor simple.
- Claim como arreglo JSON serializado.

Si el permiso requerido esta dentro del claim, la policy se marca como satisfecha.

## Autenticacion

### Login

`POST /api/auth/login`

Valida:

- Usuario existente.
- Usuario activo.
- Password correcto.

Acciones:

- Actualiza `UltimoAcceso`.
- Devuelve token JWT.
- Devuelve datos del usuario y permisos efectivos.

### Register

`POST /api/auth/register`

Crea usuario con rol opcional. Este endpoint sigue existiendo para soporte de registro, aunque la administracion operativa actual se hace desde `UsuariosController`.

### Me

`POST /api/auth/me`

Requiere token. Devuelve el usuario actual con permisos efectivos.

### Change Password

`POST /api/auth/change-password`

Permite al usuario autenticado cambiar su propia contrasena usando password actual y nueva.

## Endpoints

Todas las rutas parten de `/api`.

### Auth

| Metodo | Ruta | Seguridad | Descripcion |
|---|---|---|---|
| POST | `/auth/register` | Publico | Registra usuario y emite token. |
| POST | `/auth/login` | Publico | Inicia sesion y emite token JWT. |
| POST | `/auth/me` | Autenticado | Devuelve usuario actual. |
| POST | `/auth/change-password` | Autenticado | Cambia contrasena propia. |

### Clientes

| Metodo | Ruta | Permiso | Descripcion |
|---|---|---|---|
| GET | `/clientes` | `clientes.ver` | Lista clientes. |
| GET | `/clientes/{id}` | `clientes.ver` | Obtiene un cliente. |
| POST | `/clientes` | `clientes.crear` | Crea cliente. |
| PUT | `/clientes/{id}` | `clientes.editar` | Actualiza cliente. |
| DELETE | `/clientes/{id}` | `clientes.eliminar` | Desactiva cliente. |

### Proyectos

| Metodo | Ruta | Permiso | Descripcion |
|---|---|---|---|
| GET | `/proyectos` | `proyectos.ver` | Lista proyectos. |
| GET | `/proyectos/{id}` | `proyectos.ver` | Obtiene un proyecto. |
| GET | `/proyectos/cliente/{clienteId}` | `proyectos.ver` | Lista proyectos por cliente. |
| POST | `/proyectos` | `proyectos.crear` | Crea proyecto. |
| PUT | `/proyectos/{id}` | `proyectos.editar` | Actualiza proyecto. |
| DELETE | `/proyectos/{id}` | `proyectos.eliminar` | Elimina proyecto. |

### Usuarios

| Metodo | Ruta | Permiso | Descripcion |
|---|---|---|---|
| GET | `/usuarios` | `roles.ver` | Lista usuarios del portal. |
| GET | `/usuarios/{id}` | `roles.ver` | Detalle de usuario y permisos. |
| POST | `/usuarios` | `roles.crear` | Crea usuario y asigna rol. |
| PUT | `/usuarios/{id}` | `roles.editar` | Edita usuario y rol. |
| PUT | `/usuarios/{id}/password` | `roles.editar` | Cambia password de un usuario. |
| PUT | `/usuarios/{id}/toggle` | `roles.editar` | Activa o desactiva usuario. |
| DELETE | `/usuarios/{id}` | `roles.eliminar` | Desactiva usuario. |

Reglas relevantes:

- El correo es unico.
- El rol debe existir y estar activo para crear/editar usuarios.
- La contrasena por defecto al crear usuario desde administracion puede ser el prefijo del correo si no se envia una.
- El sistema impide dejar sin usuario activo con rol `Arquitecto` en operaciones criticas.
- Al editar un usuario, el `Puesto` se deriva del rol.

### Roles

| Metodo | Ruta | Permiso | Descripcion |
|---|---|---|---|
| GET | `/roles` | `roles.ver` | Lista roles con permisos concedidos. |
| GET | `/roles/{id}` | `roles.ver` | Detalle del rol con permisos ids. |
| POST | `/roles` | `roles.crear` | Crea rol. |
| PUT | `/roles/{id}` | `roles.editar` | Edita nombre, descripcion y estado. |
| PUT | `/roles/{id}/permisos` | `roles.editar` | Actualiza permisos del rol. |
| DELETE | `/roles/{id}` | `roles.eliminar` | Elimina rol si no tiene usuarios. |

Reglas relevantes:

- No se puede renombrar el rol `Arquitecto`.
- No se puede eliminar el rol `Arquitecto`.
- No se puede eliminar un rol con usuarios asignados.
- La lista de permisos enviada a `/roles/{id}/permisos` se valida contra el catalogo real.

### Permisos

| Metodo | Ruta | Permiso | Descripcion |
|---|---|---|---|
| GET | `/permisos` | `roles.ver` | Lista permisos agrupados por modulo. |

### Management

| Metodo | Ruta | Seguridad | Descripcion |
|---|---|---|---|
| GET | `/management/snapshot` | Autenticado | Devuelve snapshot consolidado para dashboard, clientes, proyectos y catalogos auxiliares. |

El snapshot incluye:

- Fecha de generacion.
- Periodo.
- Metricas ejecutivas.
- Alertas.
- Clientes.
- Proyectos.
- Tipos de solucion.
- Usuarios disponibles para asignacion.
- Estructuras de infraestructura y equipo.

### Credenciales

| Metodo | Ruta | Permiso | Descripcion |
|---|---|---|---|
| GET | `/credenciales` | `credenciales.ver` | Endpoint protegido preparado para catalogo de credenciales. Actualmente devuelve lista vacia. |

## Contratos Principales

### ApiResponse

La mayoria de controladores responde:

```json
{
  "success": true,
  "data": {},
  "message": "",
  "errors": []
}
```

### AuthResponseDto

Incluye:

- `token`
- `expiresAt`
- `user`

El usuario incluye:

- Id.
- Nombres.
- Apellidos.
- Iniciales.
- Puesto.
- Rol.
- Permisos.

### ClienteDto

Representa clientes para API CRUD.

Incluye:

- Id.
- Nombre.
- Industria.
- Iniciales.
- ColorClass.
- TotalProyectos.
- Activo.
- FechaAlta.

### ProyectoDto

Representa proyectos para API CRUD.

Incluye:

- Id.
- Nombre.
- ClienteId.
- ClienteNombre.
- TipoSolucionId.
- TipoSolucionNombre.
- Etapa.
- Estado.
- Progreso.
- Fechas.
- TotalMiembros.
- Miembros.

### UsuarioListDto

Incluye:

- Id.
- Nombres.
- Apellidos.
- Correo.
- Telefono.
- Iniciales.
- Puesto.
- RolId.
- Rol.
- Activo.
- FechaAlta.
- UltimoAcceso.

### RolListDto

Incluye:

- Id.
- Nombre.
- Descripcion.
- Estado activo.
- Conteo de usuarios.
- Permisos agrupados por modulo.

## Flujos De Negocio

### Crear cliente

1. El frontend envia nombre, industria, iniciales y color.
2. `ClientesController` exige `clientes.crear`.
3. `ClienteService` genera Id y fecha de alta.
4. El repositorio persiste en MySQL.
5. La API devuelve el cliente creado.

### Editar cliente

1. Se valida permiso `clientes.editar`.
2. Se busca el cliente.
3. Se mapean los cambios.
4. Se actualiza en base de datos.

### Eliminar cliente

1. Se valida permiso `clientes.eliminar`.
2. Se busca el cliente.
3. Se marca `Activo = false`.

### Crear proyecto

1. Se valida permiso `proyectos.crear`.
2. Se valida que exista cliente.
3. Se valida que exista tipo de solucion.
4. Se crea proyecto con:
   - Id nuevo.
   - Progreso inicial 0.
   - Fecha inicio actual.
   - Fecha fin estimada a 3 meses.
   - Miembros asignados.
5. Se valida cada usuario asignado.
6. Se actualiza contador de proyectos del cliente.

### Editar proyecto

1. Se valida permiso `proyectos.editar`.
2. Se busca proyecto.
3. Se valida cliente y tipo de solucion.
4. Se actualizan datos.
5. Se limpia y reconstruye la lista de miembros.
6. Se actualiza contador del cliente anterior y del nuevo si cambio.

### Eliminar proyecto

1. Se valida permiso `proyectos.eliminar`.
2. Se busca proyecto.
3. Se elimina.
4. Se recalcula total de proyectos del cliente.

### Crear usuario

1. Se valida permiso `roles.crear`.
2. Se valida nombre, apellido y correo.
3. Se verifica que el correo no exista.
4. Se valida rol activo.
5. Se crea usuario Identity.
6. Se asigna rol unico.
7. Si no se envia password, se usa el prefijo del correo.

### Editar usuario

1. Se valida permiso `roles.editar`.
2. Se valida existencia del usuario.
3. Se valida correo unico.
4. Se valida rol activo.
5. Si se intenta quitar el ultimo arquitecto activo, se bloquea.
6. Se actualizan datos.
7. Se remueven roles previos.
8. Se asigna rol seleccionado.

### Cambiar password desde administracion

1. Se valida permiso `roles.editar`.
2. Se busca usuario.
3. Se genera token de reset.
4. Se cambia password.

### Activar/desactivar usuario

1. Se valida permiso `roles.editar`.
2. Se bloquea desactivar al ultimo arquitecto activo.
3. Se invierte `Activo`.

### Eliminar usuario

1. Se valida permiso `roles.eliminar`.
2. Se bloquea eliminar al ultimo arquitecto activo.
3. Se desactiva usuario.

### Administrar permisos de rol

1. Se valida permiso `roles.editar`.
2. Se valida rol.
3. Se valida que los permisos enviados existan.
4. Se actualizan o crean registros `RolPermiso`.

## Seed Inicial

Al iniciar, `InitializeDatabaseAsync` ejecuta:

1. Migraciones.
2. Seed de permisos.
3. Seed de roles base.
4. Seed de permisos por rol.
5. Seed de usuario por defecto.
6. Seed de datos funcionales si no existen clientes.

Datos funcionales sembrados:

- Clientes:
  - Repsol.
  - Telefonica.
  - BBVA.
- Tipos de solucion:
  - Portal de Proveedores.
  - Facturacion Electronica.
  - Host2Host.
  - Guias de Remision.
- Usuarios de ejemplo:
  - Desarrolladores.
  - Lead Technical.
  - Arquitecto.
- Proyectos:
  - Plataforma Digital Upstream.
  - App Mi Movistar.
  - Banca Digital Empresas.
  - CRM Comercial Repsol.

Importante:

- Las credenciales de desarrollo sembradas deben rotarse o eliminarse antes de produccion.

## Validaciones

### Clientes

Usan validadores en:

- `CreateClienteValidator.cs`
- `UpdateClienteValidator.cs`

### Proyectos

Usan validadores en:

- `CreateProyectoValidator.cs`
- `UpdateProyectoValidator.cs`

### Usuarios y roles

Combinan validaciones manuales en controladores con validaciones propias de Identity:

- Email unico.
- Usuario activo.
- Rol valido.
- No eliminar ultimo arquitecto activo.
- No eliminar roles con usuarios.
- No renombrar/eliminar `Arquitecto`.

## Repositorios

### ClienteRepository

Cubre:

- Listar.
- Obtener por Id.
- Crear.
- Actualizar.
- Soft delete indirecto via servicio.

### ProyectoRepository

Cubre:

- Listar proyectos con relaciones.
- Obtener por Id.
- Listar por cliente.
- Crear.
- Actualizar.
- Eliminar.

### TipoSolucionRepository

Cubre:

- Listar tipos.
- Obtener tipo por Id.

## Mapeos AutoMapper

El perfil `AutoMapperProfile` cubre:

- Cliente a DTO.
- DTOs de cliente a entidad.
- Usuario a snapshot.
- ProyectoMiembro a DTO.
- Proyecto a DTO CRUD.
- Proyecto a DTO de management.
- Cliente a DTO de management.

Tambien calcula etiquetas visuales para frontend:

- Tono del cliente.
- Label de etapa.
- Tono de etapa.
- Tono de progreso.
- Label de estado.
- Tono de estado.
- Resumen de lead basado en miembros principales y de apoyo.

## Middleware De Errores

`GlobalExceptionHandler` centraliza excepciones.

Objetivo:

- Evitar respuestas inconsistentes.
- Convertir errores conocidos a respuestas API.
- Mantener un formato uniforme para fallos.

## Estado Actual De Modulos

### Implementado y operativo

- Auth/login.
- Usuarios.
- Roles.
- Permisos.
- Clientes.
- Proyectos.
- Tipos de solucion.
- Miembros de proyecto.
- Snapshot de management.
- Proteccion por JWT.
- Proteccion por permisos.

### Preparado o parcial

- Credenciales: endpoint protegido creado, respuesta vacia.
- Ambientes: permisos definidos, aun sin CRUD real.
- Despliegues: permisos definidos, aun sin CRUD real.
- Infraestructura tecnica: representada principalmente en contratos/frontend, pendiente de persistencia real.

## Consideraciones De Produccion

Antes de produccion revisar:

- Secretos JWT.
- Connection string.
- CORS.
- Passwords sembrados.
- Politicas de password.
- HTTPS.
- Logs estructurados.
- Rotacion de credenciales.
- Backups de MySQL.
- Migraciones controladas por pipeline.
- Endpoints publicos de registro si no se quieren permitir altas abiertas.

## Comandos Utiles

Restaurar:

```bash
dotnet restore
```

Compilar:

```bash
dotnet build
```

Ejecutar API:

```bash
dotnet run --project src/ConsultoraPro.API --launch-profile https
```

Aplicar migraciones manualmente:

```bash
dotnet ef database update --project src/ConsultoraPro.Infrastructure --startup-project src/ConsultoraPro.API
```

Swagger local:

```text
https://localhost:7001/swagger
```

