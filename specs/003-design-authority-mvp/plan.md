# Implementation Plan: MVP Operativo de Design Authority

**Feature branch**: `003-design-authority-mvp` | **Current worktree branch**: `main` | **Date**: 2026-10-10 | **Spec**: [spec.md](./spec.md)

**Input**: Feature specification from `specs/003-design-authority-mvp/spec.md` and the approved domain decisions in `specs/001-domain-foundation/` and `specs/002-equipos-tecnologias/`.

## Summary

Plan the MVC workflows for Hallazgos and their Seguimientos, Acuerdos and
their Seguimientos, Decisiones de Arquitectura, Reuniones, and a single
current Minuta per Reunión. The design reuses the Trámite, Subetapa,
TeamTechnology, Role, Persona, and configured-user concepts from features 001
and 002.

The consolidated relational model uses SQL Server, Dapper, versioned SQL
scripts, Repository Pattern, and Dependency Injection. It enforces
same-Trámite Subetapa associations, Technology-only functional associations,
immutable consecutive identifiers, independent follow-up records, soft
deletion, audit metadata, and a single current Minuta file reference. It does
not add REST endpoints, authentication, authorization, Excel, AI, weekly
reports, or CRUD screens for manually maintained Trámite/Subetapa catalogs.

The user explicitly approved this plan and its tasks for implementation on
2026-10-09. The approved model may be implemented as versioned SQL and
production code within the stated scope.

## Technical Context

**Language/Version**: C# / .NET 10

**Primary Dependencies**: ASP.NET Core MVC, Dapper 2.1.66, Microsoft.Data.SqlClient
6.1.3, Bootstrap/CSS; dependency injection built into ASP.NET Core. Automated
tests use xUnit in the approved test projects.

**Storage**: SQL Server for relational data and a configured file-storage
root for Minutas. Database configuration and storage root are supplied outside
source-controlled secrets.

**Testing**: Domain and Application unit tests; SQL Server integration tests
for constraints, transactions, repositories, and concurrent ID allocation;
MVC flow tests for server-side validation and rendered workflows.

**Target Platform**: ASP.NET Core MVC web application on .NET 10. Deployment
hosting and environment-specific SQL Server provisioning are not specified by
this feature.

**Project Type**: Layered server-rendered web application.

**Performance Goals**: No numeric service-level or throughput goals are
specified. Lists must support the filtering, ordering, and pagination defined
in the specification.

**Constraints**:
- No Entity Framework, REST API, authentication, authorization, AI, or Excel
  functionality.
- Web must not access SQL Server directly. All persistence goes through
  Application repository contracts and Infrastructure Dapper implementations.
- Every write must validate on the server and execute all related database
  writes atomically.
- Runtime secrets and connection strings must be external to source control.
- Versioned SQL scripts must be reviewed as part of the initial schema.
- File storage and SQL Server cannot share an atomic transaction; replacement
  must use explicit staging and cleanup behavior.
- Do not define attributes or relationships for Informe Semanal in this
  feature.

**Scale/Scope**: Personal application, one configured user, the functional
modules and catalogs defined by features 001–003; no high-concurrency or
availability target is specified.

## Constitution Check

### Before design

| Principle | Gate |
|---|---|
| Spec-Driven Development | PASS — this work plans the clarified feature and does not implement it. |
| Layered architecture | PASS — responsibilities are assigned to Web, Application, Domain, and Infrastructure. |
| Required stack / no Entity Framework | PASS — C#/.NET 10, ASP.NET Core MVC, SQL Server, Dapper, Bootstrap/CSS. |
| No internal REST APIs | PASS — contracts below are internal Application/MVC contracts, not HTTP APIs. |
| Repository Pattern and DI | PASS — repository interfaces are in Application and implementations in Infrastructure. |
| Secrets and server validation | PASS — external configuration and server-side validation are planned. |
| Soft delete and auditing | PASS — applied to the functional records and Persona that support logical deletion. |
| Versioned SQL and integral initial schema | PASS — the user approved the consolidated initial schema in `data-model.md`; scripts must be versioned and implementation remains subject to plan/task approval. |
| Configured user; no authentication | PASS — one configured identity supplies audit and activity attribution. |
| Canonical domain / no invented rules | PASS — “Trámite” remains canonical; relationships follow the feature specification. |
| Simplicity and explicit scope | PASS — no weekly-report model, Excel, AI, authentication, authorization, or extra user features are introduced. |

### After design

**PASS.** The design covers the fully specified MVP entities across features
001–003. The user approved the consolidated schema in `data-model.md` as the
initial MVP schema; that approval is recorded in the clarification dated
2026-10-09. The user explicitly approved this plan and its tasks for
implementation on 2026-10-09. Any SQL scripts must be versioned. Informe
Semanal and Excel remain out of scope for this schema and will be introduced
through later migrations.

The agreed requirement that IDs be consecutive is addressed technically by
transactional counters rather than `IDENTITY` or a SQL Server sequence. The
Decision identifier uses a global suffix counter across all Trámites and
Subetapas, as confirmed by the user. Its visible Trámite/Subetapa components
must be generated from stable catalog identifiers and must not be changed
when a display name changes.

The spec does not define whether inactive Personas may be newly assigned to
records, nor whether a Decision's Subetapa must be active. The plan does not
invent those eligibility rules. Tasks must keep those choices out of code
unless the feature specification is clarified first. Referential integrity
and the explicitly stated same-Trámite rule remain required.

## Design decisions

1. **Consecutive identifiers**: use a SQL Server counter row per numeric
   functional entity type; increment and insert the entity in one transaction.
   Use a separate global Decision suffix counter to create the required
   `[Trámite]-[Subetapa]-XXX` form. Do not use `IDENTITY` or `SEQUENCE` for
   these business identifiers because gaps on failed/rolled-back inserts or
   server restart are incompatible with the stated consecutiveness requirement.
2. **Relationship integrity**: use composite foreign keys for
   `(TramiteId, SubstageId)` and `(TeamTechnologyId, Kind)`. Store the
   corresponding fixed kind discriminator on functional records, constrained
   to `Technology`; constrain a Decision's responsible Team discriminator to
   `Team`. This preserves the unified catalog without accepting a Team where
   a Technology is required.
3. **Controlled values**: represent the finite functional lists in seeded,
   versioned reference tables, not free-form text. The values and labels must
   match `spec.md`; there is no catalog-management screen in this feature.
4. **Follow-ups**: store each follow-up in its own row with its functional
   date, configured user, observation, and optional old/new status and
   commitment-date values. Updating a parent never edits prior follow-up rows.
   Individual follow-up edit/delete behavior is not defined by this feature.
5. **Minuta replacement**: store at most one `MeetingMinute` row per Meeting.
   Write and validate a new file in staging, update the current metadata in a
   database transaction, then delete the previous physical file. Record
   failed file deletions in an infrastructure cleanup queue and retry them;
   the queue is not a user-visible version history and retains no document
   after successful deletion.
6. **List queries**: expose only specification-approved filter and sort
   options. Dapper values are parameters; dynamic sort expressions come from
   a fixed allowlist. Use a stable secondary ordering by identifier for
   pagination.
7. **Column preferences**: persist no user profile or server-side preference.
   Keep list customization as presentation/browser state for this single-user
   MVP; the required identifier, Technology, and primary descriptive column
   cannot be hidden.

## Architecture and responsibilities

| Project | Responsibilities |
|---|---|
| `DesignAuthority.Web` | ASP.NET Core MVC controllers, Views, ViewModels, server-side binding/validation feedback, list filters and display, composition root. No SQL or direct Infrastructure calls. |
| `DesignAuthority.Application` | Use cases, request/result models, validation coordination, configured-user abstraction, repository and file-storage contracts, transaction orchestration. |
| `DesignAuthority.Domain` | Entities/value concepts, allowed state/value rules, same-Trámite association rules, and domain invariants. No infrastructure dependency. |
| `DesignAuthority.Infrastructure` | Dapper repositories, SQL transaction/counter implementation, SQL connection factory, versioned SQL scripts, configured file storage, cleanup retry implementation and operational logging. |

Application repository interfaces are implemented in Infrastructure and
registered through Web's dependency-injection composition root. Domain must not
depend on Application or Infrastructure. Application must not depend on Web.

## Project Structure

### Documentation (this feature)

```text
specs/003-design-authority-mvp/
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
└── contracts/
    └── application-mvc.md
```

`tasks.md` is created by `/speckit-tasks`, not by this plan.

### Implemented source structure

```text
src/
├── DesignAuthority.Web/
│   ├── Controllers/
│   ├── ViewModels/
│   ├── Views/
│   └── wwwroot/
├── DesignAuthority.Application/
│   ├── Abstractions/
│   └── Features/
├── DesignAuthority.Domain/
│   ├── Entities/
│   └── Rules/
└── DesignAuthority.Infrastructure/
    ├── Persistence/
    ├── Repositories/
    ├── FileStorage/
    └── DependencyInjection/
database/
└── scripts/                 # versioned SQL scripts, added only after schema approval
tests/
├── DesignAuthority.Domain.Tests/
├── DesignAuthority.Application.Tests/
├── DesignAuthority.Infrastructure.IntegrationTests/
└── DesignAuthority.Web.IntegrationTests/
```

**Structure Decision**: use the four-project layered ASP.NET Core MVC
structure required by the Constitution. The solution, project scaffolding,
and initial feature implementation are present; SQL Server-dependent
acceptance validation remains pending a test database.

## Verification strategy

- Verify Domain invariants and allowed-value lists with unit tests.
- Verify Application validation and that invalid requests make no repository
  writes.
- Verify same-Trámite Subetapa references, Technology-only / Team-only
  associations, audit fields, soft delete, and catalog references against a
  real SQL Server test database.
- Verify concurrent creation produces unique, strictly consecutive numeric
  identifiers and that rollback does not consume an identifier.
- Verify follow-ups are independent rows and survive parent soft deletion.
- Verify MVC input validation, approved list filters/sorts, pagination,
  mandatory visible columns, attendee minimum, and user-safe error display.
- Verify `.docx` and 5 MB Minuta validation, relative paths, single current
  metadata row, physical replacement, and cleanup retry behavior.

## Complexity Tracking

No Constitution violations or additional architectural projects are proposed.
