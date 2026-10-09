# Quickstart: MVP Operativo de Design Authority

## Purpose and current status

This guide documents how to build and validate the current MVP implementation.
The solution, MVC workflows, application/domain layers, Dapper persistence,
and initial versioned schema script are present. SQL Server-backed acceptance
validation still requires a provisioned disposable test database.

## Prerequisites for implementation

- .NET 10 SDK.
- SQL Server instance and a dedicated test database.
- Dapper and Microsoft.Data.SqlClient restored by the solution.
- Connection string and file-storage root supplied through environment- or
  deployment-specific configuration, not committed to source control.
- The approved schema and reference-data scripts applied to the test database.
- A seeded configured user and the approved initial TeamTechnology, Role,
  Trámite/Subetapa, and functional reference values.

## Setup and test commands

From the repository root:

```powershell
dotnet restore .\DesignAuthority.sln
dotnet build .\DesignAuthority.sln --no-restore
dotnet test .\DesignAuthority.sln --no-build
```

Set `ConnectionStrings__DesignAuthority`, `ConfiguredUser__Id`, and
`FileStorage__RootPath` in the process or deployment environment before
running the application. The connection string and database credentials must
not be committed to source control.

The current automated tests cover domain invariants, application validation,
and local Minuta file storage. SQL Server repository and complete MVC flow
acceptance scenarios remain unverified until an isolated SQL Server test
database is available. Do not report those scenarios as passed based only on
a build or unit-test run.

## Acceptance validation scenarios

1. **Trámite and Subetapa integrity**
   - Create records with an active default Trámite and verify it is initially
     selected; change it and verify existing records retain their stored
     Trámite.
   - With no active default, verify the user can select an active Trámite.
   - Save a Hallazgo with a Subetapa from a different Trámite and verify the
     server rejects it without writing partial data.
   - Verify the same-Trámite constraint for Decisiones. Do not test or impose
     an active-only Decision Subetapa rule until specified.

2. **Technology and Team classification**
   - Verify Hallazgo, Acuerdo, Decisión, and Reunión accept only one of
     Appian, Databricks, Microservicios, or Power BI.
   - Attempt to persist Arquitectura, Proyecto, Célula, or QA as a Technology
     and verify database integrity rejects it.
   - Verify a Decision's responsible assignment is a Team, not a Technology.

3. **Consecutive identifiers**
   - Create multiple Hallazgos, Acuerdos, and Reuniones and verify each
     numeric series starts at 1 and advances by exactly one.
   - Create Decisiones and verify the approved formatted identifier and
     suffix-allocation scope.
   - Run concurrent create requests and a forced rollback; verify there are
     no duplicate or skipped committed values.

4. **Follow-ups, audit, and soft delete**
   - Add multiple follow-ups to one Hallazgo and Acuerdo; verify each row
     retains its own date, configured user, observation, and old/new values
     when applicable.
   - Update or soft-delete a parent and verify prior follow-ups remain intact.
   - Verify creation, modification, and deletion audit attribution uses the
     configured user and invalid writes change neither entity nor audit
     metadata.

5. **Statuses and lists**
   - Verify Abierto is initially selected for new Hallazgos and Acuerdos and
     that every specified status can be selected on create/edit without a
     transition graph.
   - Verify initial Hallazgo and Acuerdo lists show only Abierto and En
     progreso; test their description search, Technology/status filters,
     allowed sorts, stable pagination, and always-visible columns.
   - Verify Decision and Meeting list fields, filters, sorts, pagination, and
     their always-visible columns against FR-031 and FR-032.

6. **Meeting participants**
   - Save a Reunión with at least one attendee and an empty absence list.
   - Attempt a Reunión with no attendees and verify no partial Meeting or
     participant rows are committed.
   - Verify a Person cannot appear as both attended and absent in one Meeting.

7. **Minuta**
   - Verify `.docx` Minutas up to the approved 5 MB byte limit are accepted;
     other extensions and larger files are rejected server-side.
   - Verify the stored relative reference, original name, generated physical
     name, one current metadata row, and successful physical replacement.
   - Simulate a SQL failure during replacement and a physical deletion
     failure after commit; verify the current reference remains consistent
     and the old file is retried for cleanup without exposing version history.

8. **Operational errors**
   - Force a repository/storage failure and verify the MVC response does not
     expose connection strings, SQL, physical paths, or stack traces.
   - Verify relevant diagnostic details are logged and the operation is not
     reported as successful.

## Related design

- Entity fields and constraints: [data-model.md](./data-model.md)
- Internal use-case and MVC boundaries: [contracts/application-mvc.md](./contracts/application-mvc.md)
- Technical decisions and sources: [research.md](./research.md)
