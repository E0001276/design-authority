# Implementation Plan: Equipos y Tecnologías

**Branch**: `main` (feature: `002-equipos-tecnologias`) | **Date**: 2026-10-09 | **Spec**: [spec.md](./spec.md)

**Input**: Feature specification at `specs/002-equipos-tecnologias/spec.md`

## Summary

Definir la administración MVC de personas asociadas a exactamente un Equipo o
Tecnología y un Rol o puesto válido para ese contexto. El diseño usa cuatro
proyectos (`DesignAuthority.Web`, `DesignAuthority.Application`,
`DesignAuthority.Domain` y `DesignAuthority.Infrastructure`), SQL Server,
Dapper, Repository Pattern, DI y Bootstrap/CSS. El Id técnico se propone como
clave entera `IDENTITY` de SQL Server: es simple, único mediante clave primaria
y no editable desde casos de uso. La fecha funcional de alta se representa con
la fecha de creación de auditoría, sin duplicar el mismo evento.

Los ocho Equipos o Tecnologías y sus quince Roles iniciales son datos de
referencia versionados. `TeamTechnology` conserva un clasificador explícito
`Kind` (`Technology` o `Team`): Persona acepta cualquiera de las ocho opciones,
mientras las features de Hallazgos, Acuerdos, Decisiones de Arquitectura y
Reuniones pueden restringir sus asociaciones a `Technology`. La integridad de
Rol/Equipo se protege con una clave foránea compuesta. La ocupación máxima de
una persona activa en Roles de liderazgo se comprueba de forma transaccional
en el repositorio, serializando las operaciones por RoleId; el soft delete
libera esa ocupación. No se implementa autenticación, autorización, roles de
seguridad, IA, Excel ni APIs REST. Este documento planifica; no autoriza todavía
scripts o código hasta aprobar las tareas y el esquema inicial integral de la
aplicación.

## Technical Context

**Language/Version**: C# / .NET 10

**Primary Dependencies**: ASP.NET Core MVC, Dapper, Bootstrap/CSS e inyección
de dependencias integrada de ASP.NET Core

**Storage**: SQL Server. Datos de Persona y catálogos relacionales versionados
en scripts SQL; ninguna credencial ni cadena de conexión en el código fuente.

**Testing**: Pruebas unitarias de Domain/Application y pruebas de integración
de Infrastructure contra SQL Server, más pruebas de flujo MVC. El repositorio
no contiene solución ni proyectos de pruebas; el framework y la provisión de
SQL Server se definirán al crear la solución.

**Target Platform**: Aplicación web ASP.NET Core; alojamiento y sistema
operativo no definidos por esta feature.

**Project Type**: Aplicación web MVC monolítica en capas.

**Performance Goals**: No definidos por la especificación; no se introducen
umbrales de rendimiento.

**Constraints**: Sin Entity Framework, APIs REST, autenticación, autorización,
roles de seguridad, IA ni Excel. Acceso a datos solo en Infrastructure por
repositorios Dapper. El Id funcional es único e inmutable; el mecanismo
técnico recomendado se decide aquí. Auditoría con el usuario configurado de
`001-domain-foundation`. El soft delete aplica a Persona; los catálogos
iniciales son referencias y no tienen operaciones de eliminación en esta
feature.

**Scale/Scope**: Un usuario configurado; ocho valores iniciales de Equipo o
Tecnología y quince Roles asociados. El volumen de Personas, concurrencia y
disponibilidad no están definidos.

## Constitution Check

| Regla | Resultado | Aplicación en el plan |
|---|---|---|
| Spec-Driven Development | PASS | El plan deriva de `spec.md`; tareas aprobadas preceden a la implementación. |
| Arquitectura en capas | PASS | Web MVC; casos de uso y contratos en Application; invariantes en Domain; Dapper y repositorios en Infrastructure. |
| Stack obligatorio | PASS | C#, .NET 10, ASP.NET Core MVC, SQL Server, Dapper y Bootstrap/CSS. |
| Sin Entity Framework ni REST interno | PASS | No se propone EF ni API REST. |
| Repository Pattern y DI | PASS | Application declara interfaces; Infrastructure las implementa; Web compone por DI. |
| Protección de secretos y validación | PASS | Configuración externa y validación en servidor tanto sintáctica como de dominio. |
| Soft delete y auditoría | PASS | Persona conserva estado y datos de eliminación; creación/modificación/eliminación se asocian al usuario configurado. |
| Scripts SQL versionados y esquema inicial integral | CONDITIONAL | El modelo de esta feature se define; los scripts ejecutables se bloquean hasta integrar y aprobar el esquema inicial completo requerido por la Constitución. |
| Sin autenticación | PASS | Se reutiliza el contexto de usuario configurado; no se agregan login ni roles de seguridad. |
| Simplicidad y alcance | PASS | Catálogos acotados a datos iniciales; una sola asignación por persona; no se agregan relaciones extra. |

**Pre-Phase Gate**: PASS para el diseño. No se ha aprobado aún la
implementación de SQL ni código: debe completarse el esquema integral de la
aplicación y aprobarse el plan y las tareas.

## Design Decisions

- Una tabla `TeamTechnology` unifica las cuatro Tecnologías y los cuatro
  Equipos, con un campo obligatorio `Kind` (`Technology` o `Team`) que
  conserva explícitamente su diferencia semántica. Persona puede elegir
  exactamente una de las ocho opciones. Las features posteriores que modelen
  Hallazgos, Acuerdos, Decisiones de Arquitectura o Reuniones deben poder
  limitar la asociación a filas con `Kind = Technology`.
- Para preservar esa restricción en relaciones futuras, `TeamTechnology`
  expone una clave candidata `(TeamTechnologyId, Kind)`. Una entidad futura
  que solo acepte Tecnologías puede referenciar esa pareja con `Kind` fijado y
  restringido a `Technology`; así no depende solo de una validación visual ni
  acepta por error equipos como Arquitectura o QA.
- Una tabla `Role` mantiene cada Rol asociado a un solo `TeamTechnology`. La
  semilla contiene quince pares iniciales y un indicador técnico de liderazgo
  que expresa su capacidad funcional (cero o una Persona activa, o cero o más).
- `Person` mantiene las dos FK `TeamTechnologyId` y `RoleId`; la FK compuesta
  a `Role(TeamTechnologyId, RoleId)` impide seleccionar un Rol de otra opción.
  Solo se modela una asignación por fila; no hay tablas de asociación N:M.
- `PersonId int IDENTITY(1,1)` y PK es la estrategia recomendada para el Id
  técnico, con `OUTPUT INSERTED.PersonId` al crear. La PK garantiza unicidad;
  las operaciones de actualización no aceptan el Id como dato editable. No se
  promete numeración continua ni significado funcional del valor.
- `CreatedAtUtc datetime2` es la única marca de tiempo del evento de alta y
  satisface Fecha de registro. `UpdatedAtUtc` se establece al modificar;
  la baja lógica también actualiza `UpdatedAtUtc` y `UpdatedByUserId`, además
  de `IsActive`, `DeletedAtUtc` y `DeletedByUserId`.
- La exclusividad de liderazgo requiere coordinación concurrente. En una
  transacción, el repositorio bloquea la fila estable del Role elegido con
  semántica de actualización/rango, verifica que no exista otra Persona activa
  y escribe. Todas las altas, reasignaciones y eliminaciones siguen el mismo
  protocolo. La restricción se valida además en Application/Domain para
  producir un error funcional claro. No se aplica un índice único a todos los
  RoleId activos porque impediría las múltiples asignaciones válidas; un índice
  filtrado no puede decidir su filtro consultando otra tabla.
- El usuario auditor se obtiene del servicio/contexto de usuario configurado en
  Application y se persiste según el modelo de usuario común aprobado para la
  base inicial. No se crea autenticación ni una identidad de usuario separada
  para esta feature.
- Las operaciones visibles son el directorio, alta, modificación y baja
  lógica. La tabla de referencia inicial no se edita desde la pantalla de
  Personas.

## Project Structure

### Documentation (this feature)

```text
specs/002-equipos-tecnologias/
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
├── checklists/
│   └── requirements.md
└── spec.md
```

No se genera `contracts/`: la feature no expone APIs ni contratos a sistemas
externos. Los contratos internos de repositorio pertenecen a Application y se
detallarán como trabajo de implementación en `tasks.md`.

### Source Code (proposed; not created by planning)

```text
src/
├── DesignAuthority.Web/
│   ├── Controllers/
│   ├── Views/People/
│   ├── ViewModels/People/
│   └── Program.cs
├── DesignAuthority.Application/
│   ├── People/
│   │   ├── UseCases/
│   │   ├── Contracts/
│   │   └── Repositories/
│   └── Common/             # Usuario configurado/contexto de auditoría
├── DesignAuthority.Domain/
│   └── People/
│       ├── Entities/
│       └── Rules/
└── DesignAuthority.Infrastructure/
    ├── Data/
    ├── People/
    │   └── Repositories/
    └── DependencyInjection/

database/
└── scripts/

tests/
├── DesignAuthority.Domain.Tests/
├── DesignAuthority.Application.Tests/
├── DesignAuthority.Infrastructure.IntegrationTests/
└── DesignAuthority.Web.Tests/
```

**Structure Decision**: Monolito MVC de cuatro proyectos, consistente con la
arquitectura obligatoria. Application define casos de uso y contratos de
repositorio; Infrastructure implementa consultas Dapper y transacciones; Web
no accede directamente a SQL Server. La estructura es propuesta: no hay aún
`.sln`, `.csproj` ni fuentes en el repositorio.

## Constitution Check (Post-Design)

| Gate | Resultado | Evidencia |
|---|---|---|
| Separación de capas / DI | PASS | Solo Infrastructure accede a SQL Server; Web usa casos de uso de Application. |
| Persistencia | PASS | Dapper y Repository Pattern; claves y FK refuerzan integridad sin EF. |
| Clasificación y asignaciones | PASS | `TeamTechnology.Kind` separa Technology de Team; Persona acepta ambos y las asociaciones futuras pueden restringir Technology mediante FK compuesta. |
| Exclusividad de liderazgo | PASS | Operaciones transaccionales serializadas por RoleId; roles múltiples no quedan sujetos a exclusividad. |
| Identificador / fecha de registro | PASS | Id técnico propuesto como `int IDENTITY`, inmutable desde casos de uso; `CreatedAtUtc` sirve como fecha de alta y auditoría. |
| Auditoría y baja lógica | PASS | Columnas de auditoría, usuario configurado y eliminación lógica definidos para Persona. |
| Alcance | PASS | Sin login, autorización, seguridad, IA, Excel, APIs REST ni edición dinámica de catálogos. |
| Esquema integral | BLOCKED | No ejecutar scripts ni implementar persistencia hasta aprobar el esquema inicial integral y el modelo común de usuario. |

**Post-Design Gate**: Aprobable para generar tareas. La implementación queda
bloqueada hasta completar el diseño y aprobación del esquema integral exigido
por la Constitución. Ver [data-model.md](./data-model.md) para el inventario
propuesto y [quickstart.md](./quickstart.md) para la validación posterior.

## Complexity Tracking

No se registran excepciones a la Constitución. La exclusividad de líderes usa
transacción y bloqueo porque debe conservarse bajo concurrencia; no se agrega
una relación N:M ni una tabla de asignación independiente.
