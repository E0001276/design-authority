# Internal Application and MVC Contracts

## Scope

These are design-level contracts between MVC and Application layers. They are
not REST endpoints, public web APIs, or permission/authentication contracts.
Controllers call Application use cases; Application depends on abstractions
implemented by Infrastructure. Web does not call Dapper or SQL Server.

The field rules and enumerated values come from
[the feature specification](../spec.md). This document does not authorize
implementation before the plan and tasks are approved.

## Request and result conventions

Every write use case:

- accepts a typed request that does not allow mutation of an existing record's
  identifier;
- validates required input and cross-entity rules on the server;
- obtains the configured user identity through an Application abstraction;
- returns a typed success result or validation/not-found/conflict/operational
  failure; and
- must not convert infrastructure failures into success-shaped fallback
  results.

MVC maps validation failures to form-level or field-level feedback and
redisplays submitted values. User-facing operational errors omit SQL, stack
traces, secrets, and absolute file paths; Infrastructure logs diagnostic
context.

## Application use cases

| Use-case contract | Expected behavior |
|---|---|
| `GetFindingList` | Search by description; filter by Technology/status; sort by Id, Technology, status, or registration date; paginate; initial view restricts status to Abierto/En progreso; preserve required visible columns. |
| `CreateFinding` / `UpdateFinding` | Require the defined fields; use a Technology; validate that Subetapa is active and belongs to the selected Trámite; allocate immutable consecutive Id on create; default the status selection to Abierto while allowing any listed status on create/edit; write audit metadata. |
| `AddFindingFollowUp` | Insert an independent follow-up with date, configured user, observation, and optional old/new status/commitment values. Never overwrite a previous follow-up. |
| `SoftDeleteFinding` | Set inactive/deletion audit values; do not physically delete follow-ups. |
| `GetAgreementList` | Search by description; filter by Technology/status; sort by Id, Technology, status, or registration date; paginate; initial view restricts status to Abierto/En progreso. No Subetapa filter or field. |
| `CreateAgreement` / `UpdateAgreement` | Require defined fields; use a Technology; allocate immutable consecutive Id on create; default the status selection to Abierto while allowing any listed status on create/edit; write audit values. No Subetapa association. |
| `AddAgreementFollowUp` | Insert an independent follow-up row; preserve prior rows. |
| `SoftDeleteAgreement` | Set inactive/deletion audit values; preserve follow-ups. |
| `GetDecisionList` | Search by problem description, filter by Technology/status, sort by registration date, paginate, and enforce visible columns. |
| `CreateDecision` / `UpdateDecision` | Require a Technology, responsible Team, Trámite and same-Trámite Subetapa; allocate/retain the approved immutable identifier; allow any listed status. Do not enforce Subetapa active-only unless clarified. |
| `SoftDeleteDecision` | Set inactive/deletion audit values. |
| `GetMeetingList` | Search by subject; sort by Id, Technology, subject, or date; paginate; enforce required visible columns. |
| `CreateMeeting` / `UpdateMeeting` | Require a Technology and Trámite; collect participants as a single attendance-status set; require at least one attendee before persistence; save parent and participant rows atomically. Do not associate Subetapa. |
| `SoftDeleteMeeting` | Set inactive/deletion audit values; preserve meeting participant and current-Minuta metadata for traceability. |
| `ReplaceMeetingMinute` | Validate `.docx` and size server-side, use generated physical name and relative path, maintain at most one current row, and coordinate storage cleanup without version history. |
| `GetTramiteOptions` / `GetSubstageOptions` | Return active Trámites for record creation and active Subetapas for a selected Hallazgo Trámite. Preselect the active default if present; do not rewrite historical Trámite values. |

Pagination bounds and exact page size are implementation configuration, not a
new functional rule. Search/sort inputs must not allow arbitrary SQL
identifiers or expressions.

## Repository abstractions in Application

The final interface names may follow repository conventions when the solution
is created. The following responsibilities are required:

- **`ITramiteRepository` / `ISubstageRepository`**: fetch active options,
  default Trámite, and same-Trámite Subetapas; no CRUD screen contract.
- **`ITeamTechnologyRepository` / `IRoleRepository` / `IPersonRepository`**:
  fetch typed Technology/Team options, People and required references; reuse
  entities from feature 002.
- **`IFindingRepository` / `IAgreementRepository` /
  `IArchitectureDecisionRepository` / `IMeetingRepository`**: create/update,
  lists, details, and soft delete, including transactional identifier
  allocation for creates.
- **`IFindingFollowUpRepository` / `IAgreementFollowUpRepository`**: append
  independent event rows and query their history.
- **`IMeetingParticipantRepository`**: persist and read the participant set
  within the parent operation's database transaction.
- **`IMeetingMinuteRepository`**: read/upsert/remove the single current
  metadata row and enqueue physical cleanup in the same SQL transaction.
- **`IConfiguredUserContext`**: provide the configured audit/activity user;
  it is not an authentication abstraction.
- **`IFileStorage`**: stage, promote, read, and delete files using generated
  names and relative references; never accept absolute paths from a ViewModel.

All SQL execution and concrete `IDbConnection` usage remain in
Infrastructure. Application owns validation and transaction-level use-case
coordination; repositories own data access and persistence-level integrity.

## MVC surfaces

The feature's MVC views consist of:

- Hallazgos list/create/edit/details and follow-up entry/history;
- Acuerdos list/create/edit/details and follow-up entry/history;
- Decisiones de Arquitectura list/create/edit/details;
- Reuniones list/create/edit/details, participant selection, and current
  Minuta upload/replace.

Each list provides only the search, filters, sort choices, pagination, and
column customization specified for that module. Required identifier,
Technology, and primary descriptive columns stay visible. Trámite/Subetapa
catalog CRUD, People/TeamTechnology CRUD, and role administration belong to
other features and are not duplicated here.

## Persistence guarantees

- Repository writes use parameterized Dapper commands.
- Create operations reserve counters and insert the entity in the same SQL
  transaction.
- Hallazgo and Decision references include both Trámite and Subetapa keys.
- Functional Technology references include the Technology discriminator;
  Decision responsible-team references include the Team discriminator.
- Status/reference IDs must resolve to the approved seeded catalog.
- Soft delete updates the record and audit metadata without physical delete.
- Meeting creation/update and participant writes share one SQL transaction.
- Minuta metadata replacement and a cleanup-queue record share a SQL
  transaction; filesystem operations use staging and retryable cleanup.
