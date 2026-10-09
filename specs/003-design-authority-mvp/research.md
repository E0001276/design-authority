# Research: MVP Operativo de Design Authority

## Purpose

Record technical decisions used by the approved plan and implementation.
Functional rules and enumerated values remain sourced from [spec.md](./spec.md);
this document does not amend them.

## Decisions

### Consecutive functional identifiers

- **Decision**: Use a database counter row for each numeric identifier
  family (Hallazgo, Acuerdo, Reunión) and a separate counter for the numeric
  suffix of Decisiones de Arquitectura. Reserve the next value and insert the
  record in the same SQL Server transaction. Do not use `IDENTITY` or
  `SEQUENCE` for these business identifiers.
- **Rationale**: The feature requires unique, immutable identifiers starting
  at 1 and increasing consecutively by 1. SQL Server documents that identity
  values can have gaps following rollback, failed inserts, and restart/cache
  behavior. A counter update participating in the same transaction as its
  record insert is the simpler design that can roll back the reservation with
  the record. Serialize allocation on the relevant counter row so concurrent
  transactions cannot receive the same value or commit out of order.
- **Alternatives considered**:
  - `IDENTITY`: rejected for these IDs because SQL Server does not guarantee
    gap-free values.
  - `SEQUENCE`: rejected because it is not transactionally rolled back with
    the entity insert and can cache values.
  - Locking the full entity table and calculating `MAX(Id) + 1`: rejected as
    more contention-prone and less explicit than a dedicated counter row.
- **Limit**: The invariant is for writes made through the application and its
  supported transaction path. Manual data edits, partial database restores,
  or future physical purges are not supported allocation operations.

### Decision identifier suffix scope

- **Decision**: The user confirmed that the suffix is global for all Decisions
  in the application and does not reset by Trámite or Subetapa. Use one global,
  gap-free suffix counter, formatted to at least three digits, and prefix it
  with the stable Trámite and Subetapa identifiers. Keep the resulting
  Decision Id immutable.
- **Rationale**: This is the smallest technical design that generates unique
  identifiers in the specified `[Trámite]-[Subetapa]-XXX` shape without
  inventing a separate counter per Trámite/Subetapa pair or relying on mutable
  display names.
- **Alternatives considered**: A counter per Trámite/Subetapa pair would imply
  a grouping/reset rule that is not stated. A sequence per pair would add
  unnecessary schema/administration complexity.
- **Confirmation**: The global scope is a confirmed functional rule recorded
  in `spec.md`; a counter per Trámite/Subetapa is not permitted.

### Enforcing same-Trámite Subetapa relationships

- **Decision**: Store both Trámite and Subetapa keys on Hallazgo and Decisión;
  make the pair reference the corresponding unique pair on Subetapa.
- **Rationale**: A composite foreign key ensures the stored Subetapa belongs
  to the same Trámite even when an insert bypasses the MVC workflow.
- **Alternatives considered**: Application-only validation was rejected
  because it cannot protect data written outside the expected service path.
  A generic `FunctionalRecord -> Subetapa` relationship was rejected because
  the specification explicitly excludes it for Acuerdo and Reunión.

### Restricting Technology and Team associations

- **Decision**: Reuse `TeamTechnology` and its `Kind` (`Technology`/`Team`)
  from feature 002. Functional Technology associations carry a constrained
  Technology discriminator and reference `(TeamTechnologyId, Kind)`; the
  Decision responsible association uses the corresponding Team discriminator.
- **Rationale**: This allows one catalog for Persona assignments while
  database constraints reject a Team where a Technology is required (and vice
  versa) without creating duplicate labels or identifiers.
- **Alternatives considered**: Form-only filtering and application-only
  checks were rejected because neither protects referential integrity.
  Separate duplicate Technology/Team catalogs were rejected as unnecessary.

### Catalog values and status

- **Decision**: Represent the finite enumerations in small reference tables
  with stable codes and display labels, seeded by versioned SQL scripts.
  Keep the exact values from `spec.md`; do not add transitions or other
  statuses. There is no catalog CRUD in this feature.
- **Rationale**: Foreign keys provide database-level enforcement and the
  tables support filters and display names without duplicating labels in
  functional records.
- **Alternatives considered**: Free-form text was rejected because it permits
  values outside the approved lists. Domain-only enums were rejected as the
  sole source because SQL Server would not enforce the same catalog.
- **Status behavior**: Hallazgo, Acuerdo, and Decisión may be set to any
  defined status on create or edit; no transition graph is introduced.
  Abierto is the initial selection for new Hallazgos and Acuerdos, not a
  restriction on the status the user may select when creating them.

### Minuta file replacement

- **Decision**: Persist one current `MeetingMinute` row per Reunión. Validate
  extension and size on the server; generate the physical name internally;
  persist only a relative reference. Stage the new file, commit the new
  reference, then remove the old physical file. Record failed deletions in an
  infrastructure-only retry queue and log operational errors.
- **Rationale**: SQL Server and a file system do not share an atomic
  transaction. Staging preserves the current file until the new file is
  available; a durable cleanup action handles a post-commit deletion failure.
  The queue is not a user-visible history and removes the old file after
  successful cleanup.
- **Alternatives considered**: Delete the old file before updating SQL was
  rejected because a later database failure would leave the Reunión without
  its current file. Keep all files indefinitely was rejected because it
  conflicts with the no-version-history requirement.
- **Limit**: Interpret 5 MB as `5 * 1024 * 1024` bytes in the implementation.
  No absolute server path is stored in SQL.

### List customization, filters, and sorting

- **Decision**: Keep column preferences in browser/presentation state for the
  single-user MVP; do not add a profile/preference table. Enforce the required
  always-visible columns in the MVC view. Use an allowlist for sort columns
  and direction, parameterized Dapper values, and a stable identifier
  secondary order for pagination.
- **Rationale**: The specification requires column customization but does
  not require its persistence; browser state avoids introducing a new
  profile feature. SQL parameters protect values, while a sort allowlist is
  needed because sort identifiers cannot be parameterized as values.
- **Alternatives considered**: A persistent user-preference subsystem was
  rejected as unnecessary for the configured-user MVP. Arbitrary sort-column
  interpolation was rejected as unsafe.

### Layering and test approach

- **Decision**: Keep Domain independent; place use cases and repository
  abstractions in Application; put Dapper and file storage in Infrastructure;
  use MVC controllers/views in Web. Use unit tests for Domain/Application and
  SQL Server/MVC integration tests for the relevant boundary behavior.
- **Rationale**: This follows the Constitution and prevents Web from bypassing
  repository contracts.
- **Alternatives considered**: Direct Dapper calls from Controllers and an
  ORM-generated model were rejected by the required architecture and the
  prohibition on Entity Framework.
- **Test framework**: Use xUnit in the test projects defined by the approved
  tasks.

## Sources

- Microsoft Learn, [IDENTITY (Property) (Transact-SQL)](https://learn.microsoft.com/en-us/sql/t-sql/statements/create-table-transact-sql-identity-property?view=sql-server-ver17) — identity uniqueness requires a key/constraint, and gaps can result from failures, rollbacks, or server restarts.
- Microsoft Learn, [CREATE SEQUENCE (Transact-SQL)](https://learn.microsoft.com/en-us/sql/t-sql/statements/create-sequence-transact-sql?view=sql-server-ver17) — sequences are independent of tables and values may be requested before an insert.
- Microsoft Learn, [Transactions (Transact-SQL)](https://learn.microsoft.com/en-us/sql/t-sql/language-elements/transactions-transact-sql?view=sql-server-ver17) — transaction changes commit together or roll back together.
- Microsoft Learn, [Table Hints (Transact-SQL)](https://learn.microsoft.com/en-us/sql/t-sql/queries/hints-transact-sql-table?view=sql-server-ver17) — locking hints exist; the optimizer should normally be preferred, and hints require careful use.
- Microsoft Learn, [Model validation in ASP.NET Core MVC](https://learn.microsoft.com/en-us/aspnet/core/mvc/models/validation?view=aspnetcore-10.0) — model binding and validation occur before MVC actions; the application must handle invalid ModelState.
- Dapper project documentation, [Dapper](https://github.com/DapperLib/Dapper) — Dapper extends ADO.NET connections with parameterized query/command APIs.
