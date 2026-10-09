# Implementation Plan: Fundamentos de Dominio

**Branch**: `001-domain-foundation` | **Date**: 2026-10-09 | **Spec**: [spec.md](./spec.md)

**Input**: Feature specification at `specs/001-domain-foundation/spec.md`

## Summary

Establecer una base MVC en capas y un modelo relacional inicial para los
conceptos y reglas confirmados de Design Authority. La solución propuesta usa
cuatro proyectos (`DesignAuthority.Web`, `DesignAuthority.Application`,
`DesignAuthority.Domain` y `DesignAuthority.Infrastructure`), SQL Server,
Dapper, Repository Pattern y Dependency Injection. No implementa CRUD
funcional, APIs REST, autenticación, IA ni Excel.

El diseño de datos incluye las entidades nombradas por la especificación,
aplica únicamente las relaciones confirmadas, y deja explícitamente pendientes
los atributos funcionales y relaciones que corresponden a features
posteriores. Por ello, este plan no autoriza implementar un esquema final hasta
que se complete el diseño integral requerido por la constitución.

## Technical Context

**Language/Version**: C# / .NET 10

**Primary Dependencies**: ASP.NET Core MVC, Dapper, Bootstrap/CSS; inyección de
dependencias integrada de ASP.NET Core

**Storage**: SQL Server para datos relacionales; almacenamiento de Minuta detrás
de un contrato de infraestructura y con referencia relativa. Ubicación física
de archivos por ambiente queda pendiente de despliegue.

**Testing**: Pruebas unitarias de Domain y Application; pruebas de integración
de Infrastructure contra SQL Server; pruebas MVC/funcionales para flujos
incluidos. Framework de pruebas no seleccionado porque el repositorio no tiene
proyectos ni convenciones existentes.

**Target Platform**: Aplicación web ASP.NET Core; sistema operativo y
plataforma de alojamiento pendientes de decisión de despliegue.

**Project Type**: Aplicación web MVC monolítica en capas.

**Performance Goals**: No especificados en la feature; no se introducen metas.

**Constraints**: No Entity Framework; no APIs REST; no autenticación; secretos y
cadenas de conexión fuera del código fuente; scripts SQL versionados; ningún
CRUD funcional en esta feature. No persistir asociaciones no definidas ni
guardar versiones históricas de Minutas.

**Scale/Scope**: Un usuario configurado inicialmente. Volumen de datos,
concurrencia y objetivos de disponibilidad no especificados. Esta feature
establece conceptos e invariantes, no las pantallas funcionales futuras.

## Constitution Check

| Regla | Resultado | Aplicación en el plan |
|---|---|---|
| Spec-Driven Development | PASS | Este documento planifica únicamente la feature especificada; implementación requerirá tareas aprobadas. |
| Arquitectura en capas | PASS | MVC en Web; casos de uso en Application; reglas en Domain; Dapper, repositorios y archivos en Infrastructure. |
| Stack obligatorio | PASS | C#, .NET 10, ASP.NET Core MVC, SQL Server, Dapper y Bootstrap/CSS. |
| Sin Entity Framework ni REST interno | PASS | No se proponen EF ni endpoints REST. |
| Repository Pattern y DI | PASS | Los casos de uso dependen de contratos de repositorio; Infrastructure implementa y Web compone dependencias. |
| Secretos y validación | PASS | Configuración externa al código y validación en servidor para toda entrada que se agregue. |
| Soft delete y auditoría | PASS | Solo se aplican donde la especificación defina eliminación; auditoría de creación/modificación y eliminación cuando aplique. |
| SQL versionado y esquema inicial integral | CONDITIONAL | Se propone un inventario de esquema, pero las columnas funcionales diferidas impiden aprobar el esquema de producción hasta resolverlas. |
| Usuario configurado, sin autenticación | PASS | La identidad configurada se referencia para auditoría; no se incorpora login. |
| Trámite canónico y sin IA | PASS | No se introduce Proyecto/Proceso como conceptos ni funcionalidad de IA. |
| Excel mediante componentes reutilizables | PASS | Sin alcance de Excel en esta feature; no se crean componentes Excel. |
| Simplicidad y alcance explícito | PASS | No se agregan CRUD ni reglas no especificadas; la infraestructura de archivos queda limitada a Minuta vigente. |

**Pre-Phase Gate**: PASS para el diseño de conceptos y arquitectura. El esquema
SQL integral y su implementación quedan bloqueados hasta definir los atributos
de las entidades que se completarán en futuras features. Esta condición evita
inferir reglas y cumple el principio de diseñar el esquema completo antes de
implementar features dependientes.

## Design Decisions

- Separar cuatro ensamblados por responsabilidad; Domain no referencia
  Application, Infrastructure ni Web.
- Definir interfaces de repositorio y servicios requeridos por casos de uso en
  Application; Domain conserva entidades e invariantes que están definidas.
- Usar MVC con Controllers, Views y ViewModels en Web. No publicar contratos
  HTTP REST.
- Acceder a SQL Server exclusivamente desde Infrastructure mediante Dapper y
  parámetros SQL; todas las consultas de escritura y lectura se ejecutan a
  través de repositorios.
- Versionar scripts SQL en el repositorio. El mecanismo de migración/ejecución
  de scripts no se selecciona porque no hay tooling establecido en el repo.
- Aislar el guardado de la Minuta mediante una abstracción de almacenamiento
  que reciba el archivo y devuelva identificador/nombre físico y referencia
  relativa. El reemplazo conserva solo el documento más reciente; no se
  especifica aún el comportamiento ante fallos parciales de base de datos y
  almacenamiento.
- No crear contratos externos: esta aplicación interna no expone APIs en esta
  feature. Los contratos de repositorios son internos y pertenecen a
  Application.

## Project Structure

### Documentation (this feature)

```text
specs/001-domain-foundation/
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
├── checklists/
│   └── requirements.md
└── spec.md
```

No se genera carpeta `contracts/`: no existen interfaces públicas ni APIs
externas definidas para esta feature.

### Source Code (proposed; not created by planning)

```text
src/
├── DesignAuthority.Web/
│   ├── Controllers/
│   ├── Views/
│   ├── ViewModels/
│   └── Program.cs
├── DesignAuthority.Application/
│   ├── UseCases/
│   ├── Services/
│   └── Repositories/       # Interfaces required by use cases
├── DesignAuthority.Domain/
│   ├── Entities/
│   └── Rules/
└── DesignAuthority.Infrastructure/
    ├── Data/
    ├── Repositories/
    └── FileStorage/

database/
└── scripts/

tests/
├── DesignAuthority.Domain.Tests/
├── DesignAuthority.Application.Tests/
├── DesignAuthority.Infrastructure.IntegrationTests/
└── DesignAuthority.Web.Tests/
```

**Structure Decision**: Solución web MVC en cuatro proyectos que coinciden con
la arquitectura requerida por la solicitud y las capas de la constitución.
Repositories se ubican como contratos en Application e implementaciones en
Infrastructure. Este árbol es una propuesta de planificación; el repositorio
no contiene todavía código fuente ni proyectos .NET.

## Constitution Check (Post-Design)

| Gate | Resultado | Evidencia |
|---|---|---|
| Dependencias y capas | PASS | Referencias dirigidas hacia Domain/Application; persistencia y almacenamiento quedan en Infrastructure. |
| Acceso y persistencia | PASS | Dapper y repositorios; sin EF ni acceso directo desde Web/Domain. |
| Seguridad y usuario | PASS | Sin autenticación; secretos externalizados; auditoría ligada al usuario configurado. |
| Alcance | PASS | No REST, IA, Excel ni pantallas CRUD funcionales. |
| Integridad del dominio | PASS | Solo relaciones confirmadas; no se relacionan Persona/Equipo/Rol entre sí ni se agrega Subetapa a Acuerdo/Reunión. |
| Minuta | PASS | Una Minuta actual por Reunión; no se conserva historial; referencia relativa. |
| Esquema integral previo a implementación | BLOCKED | Atributos funcionales de varias entidades y detalles de almacenamiento aún no están definidos en la especificación. Deben resolverse antes de aprobar scripts SQL de producción o implementar consumidores dependientes. |

**Post-Design Gate**: Diseño arquitectónico aprobado para planificación;
implementación de esquema y código bloqueada por los elementos de datos
pendientes indicados en [data-model.md](./data-model.md).

## Complexity Tracking

No se registran excepciones a la constitución. La separación en cuatro proyectos
es explícita en la arquitectura solicitada y alinea responsabilidades ya
requeridas.
