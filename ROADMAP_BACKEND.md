# ConsultoraPro · Roadmap Backend

> Stack: .NET 8 · ASP.NET Core Web API · EF Core + Pomelo · MySQL 8 · Identity · JWT · AutoMapper · FluentValidation
>
> Tachar con `~~tarea~~` o marcar con `[x]` al completar cada ítem.
> Orden: de más básico a más complejo, por grupo temático.

---

## GRUPO 1 — Estabilidad base y deuda técnica existente

> Marcado: `H` = se puede hacer ahora. `P` = pendiente; después de `:` se explica por qué no se puede cerrar todavía.

- [x] Revisar que `GlobalExceptionHandler` capture y loguee correctamente todas las excepciones no controladas (incluyendo `DbUpdateException`, `InvalidOperationException`)
- [x] Verificar que todos los endpoints protegidos retornen `ApiResponse` consistente al fallar autorización (no HTML de Identity)
- [x] Confirmar que las validaciones de `FluentValidation` en Clientes y Proyectos están registradas con `AddFluentValidationAutoValidation()`
- [x] Revisar que los timestamps `CreatedAt` / `UpdatedAt` se asignan automáticamente en EF Core (interceptor o override de `SaveChanges`)
- [x] Confirmar que los seeders son completamente idempotentes (si se ejecutan dos veces, no duplican datos)
- [x] Revisar que el soft delete de Cliente no rompe proyectos asociados (decidir: bloquear o permitir con advertencia)
- [x] Revisar que el recálculo de `TotalProyectos` en Cliente es correcto cuando se elimina un proyecto
- [x] Confirmar que `UltimoAcceso` se actualiza al hacer login exitoso
- [x] Agregar índice en MySQL para `Email` en `ApplicationUser` (si no fue creado por Identity automáticamente)
- [P] Revisar que CORS no permite `*` en producción — solo los orígenes explícitamente definidos: no se puede cerrar completamente sin definir primero el dominio real de producción del frontend; por ahora solo se puede dejar preparado por configuración.

---

## GRUPO 2 — Módulo de Credenciales (funcionalidad real)

El endpoint existe pero devuelve lista vacía. Este grupo lo convierte en funcional.

> Marcado: `H` = se puede hacer ahora. `P` = pendiente; después de `:` se explica por qué no se puede cerrar todavía.

- [x] Crear entidad `Credencial` en Domain con campos: Id, Nombre, Tipo, Servidor, ProyectoId, AmbienteId (nullable), ValorCifrado, FechaVencimiento, CreadoPor, FechaCreacion, Activo
- [x] Crear enum `TipoCredencial`: `BaseDatos`, `SSHKey`, `APIKey`, `ServiceAccount`, `CertificadoSSL`, `Otro`
- [P] Cerrar estrategia productiva de cifrado AES-256 para `IEncryptionService` / `EncryptionService` — la clave viene de configuración, nunca hardcodeada: el servicio ya queda funcional, pero no se puede cerrar de forma segura hasta definir dónde vivirá la clave real por ambiente (variables de entorno, user-secrets, Key Vault u otro mecanismo); usar una clave de desarrollo no cierra el requisito de seguridad.
- [x] Crear migración para tabla `Credenciales`
- [x] Crear `ICredencialRepository` en Domain
- [x] Implementar `CredencialRepository` en Infrastructure
- [x] Crear DTOs: `CredencialListDto`, `CredencialDetalleDto`, `CreateCredencialDto`, `UpdateCredencialDto`
- [x] En `CredencialListDto` el valor **nunca** se incluye — solo metadatos
- [P] Aprobar `CredencialService` para uso productivo con lógica de cifrar al guardar y descifrar solo en endpoint de revelar: el servicio ya queda funcional, pero depende de cerrar primero la estrategia de clave de cifrado; sin clave segura por ambiente, no queda aprobado para uso real.
- [x] Crear `CredencialesController` con endpoints:
  - `GET /credenciales` → lista sin revelar valores (requiere `credenciales.ver`)
  - `GET /credenciales/{id}/revelar` → devuelve el valor descifrado, registra auditoría (requiere `credenciales.revelar`)
  - `POST /credenciales` → crear (requiere `credenciales.crear`)
  - `PUT /credenciales/{id}` → editar metadatos (requiere `credenciales.editar`)
  - `PUT /credenciales/{id}/valor` → actualizar el valor cifrado (requiere `credenciales.editar`)
  - `DELETE /credenciales/{id}` → soft delete (requiere `credenciales.editar`)
- [x] Crear entidad `AuditoriaCredencial`: quién, cuándo, qué credencial fue revelada
- [x] Implementar registro automático de auditoría en cada llamada a `revelar`
- [x] Agregar endpoint `GET /credenciales/{id}/auditoria` para ver historial de accesos (requiere `credenciales.ver`)
- [x] Agregar validaciones FluentValidation para `CreateCredencialDto` y `UpdateCredencialDto`
- [x] Incluir credenciales próximas a vencer (< 7 días) en el snapshot de management como alertas
- [x] Documentar en Swagger que el endpoint `/revelar` registra auditoría

---

## GRUPO 3 — Módulo de Ambientes (nuevo)

Los permisos ya existen en el catálogo. El CRUD completo queda funcional.

- [H] Crear entidad `Ambiente` en Domain: Id, Nombre, Tipo (enum), Url, ProyectoId, Tecnologia, Estado (enum), UptimePorcentaje, Activo, FechaCreacion
- [H] Crear enum `TipoAmbiente`: `Produccion`, `Staging`, `Desarrollo`, `QA`
- [H] Crear enum `EstadoAmbiente`: `Online`, `Offline`, `Alerta`, `Configurando`
- [H] Crear migración para tabla `Ambientes`
- [H] Crear `IAmbienteRepository` en Domain
- [H] Implementar `AmbienteRepository` en Infrastructure
- [H] Crear DTOs: `AmbienteDto`, `CreateAmbienteDto`, `UpdateAmbienteDto`
- [H] Crear `AmbienteService` en Application
- [H] Crear `AmbientesController` con endpoints:
  - `GET /ambientes` → lista todos (requiere `ambientes.ver`)
  - `GET /ambientes/proyecto/{proyectoId}` → filtrado por proyecto (requiere `ambientes.ver`)
  - `POST /ambientes` → crear (requiere `ambientes.crear`)
  - `PUT /ambientes/{id}` → editar (requiere `ambientes.editar`)
  - `PUT /ambientes/{id}/estado` → cambiar estado sin editar todo (requiere `ambientes.editar`)
  - `DELETE /ambientes/{id}` → soft delete (requiere `ambientes.editar`)
- [H] Agregar validaciones FluentValidation
- [H] Incluir resumen de ambientes (total, online, con alerta) en el snapshot de management
- [H] Agregar seed de ambientes de ejemplo asociados a los proyectos existentes

---

## GRUPO 4 — Módulo de Repositorios (nuevo)

- [ ] Crear entidad `Repositorio` en Domain: Id, Nombre, ProyectoId, Proveedor (enum), RamaPrincipal, Url, EstadoPipeline (enum), Activo, FechaCreacion
- [ ] Crear enum `ProveedorRepositorio`: `GitHub`, `GitLab`, `AzureDevOps`, `Bitbucket`, `Otro`
- [ ] Crear enum `EstadoPipeline`: `Passing`, `Failed`, `Desconocido`, `EnEjecucion`
- [ ] Crear migración para tabla `Repositorios`
- [ ] Crear `IRepositorioRepository` en Domain
- [ ] Implementar `RepositorioRepository` en Infrastructure
- [ ] Crear DTOs: `RepositorioDto`, `CreateRepositorioDto`, `UpdateRepositorioDto`
- [ ] Crear `RepositoriosController` con endpoints:
  - `GET /repositorios` → lista todos (requiere `proyectos.ver`)
  - `GET /repositorios/proyecto/{proyectoId}` → filtrado por proyecto (requiere `proyectos.ver`)
  - `POST /repositorios` → crear (requiere `proyectos.editar`)
  - `PUT /repositorios/{id}` → editar (requiere `proyectos.editar`)
  - `DELETE /repositorios/{id}` → eliminar (requiere `proyectos.editar`)
- [ ] Agregar validaciones FluentValidation (URL válida, proveedor válido)
- [ ] Incluir repositorios en el snapshot de management agrupados por proyecto
- [ ] Agregar seed de repositorios de ejemplo

---

## GRUPO 5 — Módulo de Despliegues (nuevo)

- [ ] Crear entidad `Despliegue` en Domain: Id, ProyectoId, AmbienteId, Version, EjecutadoPorId, FechaHora, Estado (enum), DuracionSegundos, Notas
- [ ] Crear enum `EstadoDespliegue`: `Exitoso`, `Fallido`, `EnCurso`, `Cancelado`
- [ ] Crear migración para tabla `Despliegues`
- [ ] Crear `IDespliegueRepository` en Domain
- [ ] Implementar `DespliegueRepository` en Infrastructure
- [ ] Crear DTOs: `DespliegueDto`, `DespliegueListDto`, `CreateDespliegueDto`
- [ ] Crear `DesplieguesController` con endpoints:
  - `GET /despliegues` → historial completo paginado (requiere `despliegues.historial`)
  - `GET /despliegues/proyecto/{proyectoId}` → historial por proyecto (requiere `despliegues.historial`)
  - `GET /despliegues/recientes` → últimos 10 para dashboard (requiere `despliegues.ver`)
  - `POST /despliegues` → registrar despliegue (requiere `despliegues.ejecutar`)
  - `PUT /despliegues/{id}/estado` → actualizar estado de un despliegue en curso (requiere `despliegues.ejecutar`)
- [ ] El campo `EjecutadoPorId` se toma del claim `userId` del JWT, nunca del body
- [ ] Calcular tasa de éxito del mes y exponerla en el snapshot de management
- [ ] Agregar los últimos 5 despliegues en el snapshot de management para el dashboard
- [ ] Agregar seed de despliegues de ejemplo

---

## GRUPO 6 — Perfil de usuario (cuenta propia)

- [ ] Crear endpoint `GET /auth/me` si no está funcionando completamente (verificar que retorna datos frescos de BD, no solo del JWT)
- [ ] Crear endpoint `PUT /auth/perfil` → editar datos propios (nombres, apellidos, teléfono, iniciales) sin requerir permiso de admin
- [ ] Crear endpoint `PUT /auth/cambiar-password` → cambio de contraseña propio con validación de contraseña actual
- [ ] Validar que el cambio de contraseña propio no puede ser usado para cambiar la contraseña de otro usuario
- [ ] Al editar perfil propio, devolver un nuevo token con los datos actualizados (iniciales, nombre) para que el frontend lo refresque

---

## GRUPO 7 — Screenshots de proyectos

- [ ] Crear entidad `Screenshot` en Domain: Id, ProyectoId, Nombre, Version, Url (blob), Descripcion, SubidoPorId, FechaSubida
- [ ] Crear migración para tabla `Screenshots`
- [ ] Configurar Azure Blob Storage o almacenamiento local para desarrollo (carpeta `/uploads`)
- [ ] Crear `IStorageService` / `StorageService` para abstraer el upload de archivos
- [ ] Crear `ScreenshotsController` con endpoints:
  - `GET /screenshots/proyecto/{proyectoId}` → lista de screenshots del proyecto
  - `POST /screenshots` → subir imagen (multipart/form-data, max 5MB, PNG/JPG)
  - `DELETE /screenshots/{id}` → eliminar screenshot
- [ ] Validar tipo de archivo y tamaño en el endpoint de upload
- [ ] Incluir screenshots recientes por proyecto en el snapshot de management

---

## GRUPO 8 — Paginación y filtros

Actualmente todos los listados devuelven todos los registros. Cuando la data crezca, esto será un problema.

- [ ] Implementar paginación genérica `PagedResult<T>` con campos: `data`, `totalCount`, `page`, `pageSize`, `totalPages`
- [ ] Aplicar paginación en `GET /usuarios` (pageSize default: 20)
- [ ] Aplicar paginación en `GET /clientes` (pageSize default: 20)
- [ ] Aplicar paginación en `GET /proyectos` (pageSize default: 20)
- [ ] Aplicar paginación en `GET /despliegues` (pageSize default: 50)
- [ ] Aplicar paginación en `GET /credenciales` (pageSize default: 20)
- [ ] Implementar filtro por estado en `GET /proyectos?estado=EnCurso`
- [ ] Implementar filtro por cliente en `GET /proyectos?clienteId=xxx`
- [ ] Implementar filtro por rol en `GET /usuarios?rol=LT`
- [ ] Implementar búsqueda por nombre en `GET /clientes?search=Repsol`

---

## GRUPO 9 — Notificaciones y alertas del sistema

- [ ] Crear servicio `IAlertaService` que calcule alertas activas del sistema:
  - Credenciales que vencen en menos de 7 días
  - Proyectos cuya fecha fin es en menos de 14 días y no están completados
  - Ambientes en estado `Alerta`
- [ ] Exponer alertas en el snapshot de management (ya hay estructura, conectar con datos reales)
- [ ] Crear endpoint `GET /alertas` para que el frontend pueda hacer polling independiente
- [ ] Evaluar implementación de SignalR hub para notificaciones en tiempo real (fase futura)

---

## GRUPO 10 — Reportes y exportación

- [ ] Crear endpoint `GET /reportes/proyectos/csv` → exportar tabla de proyectos como CSV (requiere `proyectos.ver`)
- [ ] Crear endpoint `GET /reportes/usuarios/csv` → exportar tabla de usuarios como CSV (requiere `roles.ver`)
- [ ] Crear endpoint `GET /reportes/proyecto/{id}/pdf` → generar PDF del estado del proyecto con datos básicos, etapa, progreso, miembros y ambientes
- [ ] Crear endpoint `GET /reportes/ejecutivo/pdf` → resumen ejecutivo con métricas globales para Gerencia
- [ ] Evaluar librería: `QuestPDF` (recomendada, MIT) o `iTextSharp`

---

## GRUPO 11 — Mejoras de seguridad

- [ ] Revisar y endurecer la política de contraseñas en Identity (mínimo 8 caracteres, al menos una mayúscula, un número — ajustar según política de la consultora)
- [ ] Implementar bloqueo de cuenta por intentos fallidos (`MaxFailedAccessAttempts` en Identity)
- [ ] Implementar refresh token para evitar que la sesión expire en mitad de una jornada:
  - Entidad `RefreshToken`: userId, token, expira, usado
  - Endpoint `POST /auth/refresh` → valida refresh token, emite nuevo JWT
  - Endpoint `POST /auth/revoke` → invalida refresh token activo (logout)
- [ ] Agregar rate limiting en el endpoint `/auth/login` (máx N intentos por minuto por IP)
- [ ] Auditar que ningún endpoint devuelve el hash de contraseña en ningún DTO
- [ ] Revisar que el claim `permisos` en el JWT no incluye permisos de roles inactivos

---

## GRUPO 12 — Logs estructurados y observabilidad

- [ ] Configurar `Serilog` con sink a consola (dev) y archivo rotativo (producción)
- [ ] Loguear cada request con: método, ruta, userId, duración, status code
- [ ] Loguear cada acceso a credencial revelada (ya existe en auditoría, agregar también al log)
- [ ] Loguear fallos de autenticación con IP (sin loguear contraseñas)
- [ ] Loguear errores no controlados con stack trace completo
- [ ] Configurar nivel de log por ambiente: Debug en dev, Warning en producción
- [ ] Evaluar integración con Application Insights o similar para producción

---

## GRUPO 13 — Pruebas

- [ ] Configurar proyecto de pruebas unitarias `ConsultoraPro.Tests`
- [ ] Escribir pruebas unitarias para `ClienteService` (crear, editar, soft delete)
- [ ] Escribir pruebas unitarias para `ProyectoService` (crear con miembros, editar, eliminar, recálculo de totales)
- [ ] Escribir pruebas unitarias para `EncryptionService` (cifrar y descifrar correctamente)
- [ ] Escribir pruebas unitarias para `PermissionAuthorizationHandler` (claim presente, claim ausente, claim como array JSON)
- [ ] Escribir pruebas de integración para `GET /clientes` y `POST /clientes` con base de datos en memoria (SQLite para tests)
- [ ] Escribir pruebas de integración para el flujo de login y generación de JWT
- [ ] Escribir pruebas de integración para el endpoint `/credenciales/{id}/revelar` verificando que registra auditoría

---

## GRUPO 14 — Preparación para producción

- [ ] Mover todos los secretos (JWT key, connection string, encryption key) a variables de entorno o Azure Key Vault
- [ ] Confirmar que `appsettings.Development.json` no se sube al repositorio (está en `.gitignore`)
- [ ] Revisar y ajustar las contraseñas sembradas para producción (o deshabilitar seed de usuarios de ejemplo en producción)
- [ ] Configurar HTTPS obligatorio en producción (`UseHttpsRedirection`, `HSTS`)
- [ ] Configurar CORS para solo aceptar el dominio real del frontend en producción
- [ ] Configurar política de contraseñas más estricta para producción
- [ ] Agregar health check endpoint `GET /health` para monitoreo
- [ ] Definir estrategia de migraciones para producción (no auto-migrate al arrancar, sino pipeline controlado)
- [ ] Crear Dockerfile para el backend
- [ ] Definir y documentar pipeline CI/CD (GitHub Actions o Azure DevOps): restore → build → test → publish → deploy
- [ ] Configurar backups automáticos de MySQL
- [ ] Documentar procedimiento de rotación de secretos JWT y encryption key

---

## Resumen de estado por módulo

| Módulo | Estado actual |
|---|---|
| Auth / JWT | ✅ Operativo |
| Usuarios | ✅ Operativo |
| Roles | ✅ Operativo |
| Permisos | ✅ Operativo |
| Clientes | ✅ Operativo |
| Proyectos | ✅ Operativo |
| Tipos de solución | ✅ Operativo |
| Miembros de proyecto | ✅ Operativo |
| Snapshot de management | ✅ Operativo |
| Credenciales | 🟡 CRUD funcional y auditoría lista; clave de cifrado productiva pendiente |
| Ambientes | ✅ CRUD funcional, snapshot y seed listos |
| Repositorios | 🔴 Pendiente |
| Despliegues | 🔴 Solo permisos definidos |
| Screenshots | 🔴 Pendiente |
| Perfil propio | 🟡 `/auth/me` existe, edición pendiente |
| Paginación / filtros | 🔴 Pendiente |
| Notificaciones / alertas | 🟡 Snapshot con alertas reales de credenciales y ambientes; proyectos pendiente |
| Refresh token | 🔴 Pendiente |
| Reportes / exportación | 🔴 Pendiente |
| Logs estructurados | 🔴 Pendiente |
| Pruebas unitarias | 🔴 Pendiente |
| Pruebas de integración | 🔴 Pendiente |
| Seguridad producción | 🔴 Pendiente |
| CI/CD y Docker | 🔴 Pendiente |
