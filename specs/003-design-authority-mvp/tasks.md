# Tasks: MVP Operativo de Design Authority

**Input**: Design documents from `specs/003-design-authority-mvp/`

**Prerequisites**: `plan.md`, `spec.md`, `research.md`, `data-model.md`,
`contracts/application-mvc.md`, and `quickstart.md`

**Tests**: Automated tests are included because each story defines independent
acceptance tests and the plan specifies unit, SQL Server integration, and MVC
verification.

**Organization**: Tasks are grouped by the four user stories in `spec.md`.

## Implementation progress

- Resumed after the reported power outage. `dotnet restore .\DesignAuthority.sln`
  succeeded; `dotnet test .\DesignAuthority.sln --no-restore` passed all 20
  currently discovered tests (9 Domain, 5 Application, 3 file-storage, 3 MVC).
- SQL Server is unavailable in this environment. No SQL Server repository
  integration tests are currently present, so database execution and the
  SQL-dependent acceptance scenarios remain unverified.
- Application use cases and Dapper persistence for the four modules are
  consolidated in `WorkTrackingService.cs` and `WorkTrackingRepository.cs`;
  completion markers below reflect implemented behavior, not the originally
  suggested per-feature file split.

## Format

Every task uses `- [ ] T### [P?] [US#?] Description with exact file paths`.
`[P]` is included only where work can proceed in parallel after its listed
prerequisites. User-story labels appear only in the corresponding story
phases.

## Phase 1: Setup

**Purpose**: Establish the solution and project structure in
`specs/003-design-authority-mvp/plan.md`.

- [x] T001 Create `DesignAuthority.sln` and the four .NET 10 projects `src/DesignAuthority.Web/DesignAuthority.Web.csproj`, `src/DesignAuthority.Application/DesignAuthority.Application.csproj`, `src/DesignAuthority.Domain/DesignAuthority.Domain.csproj`, and `src/DesignAuthority.Infrastructure/DesignAuthority.Infrastructure.csproj`, with project references directed inward as specified in `plan.md`.
- [x] T002 Create the Domain, Application, Infrastructure, Web, database, and test directory structures under `src/`, `database/scripts/`, and `tests/` as listed in `plan.md`, without adding feature behavior.
- [x] T003 Configure `src/DesignAuthority.Web/appsettings.json` with non-secret configuration keys for SQL connection and file-storage root, and add `src/DesignAuthority.Web/.gitignore` or the repository-level `.gitignore` rules that prevent local secret/config files from being committed; do not put credentials or a connection string value in source control.

---

## Phase 2: Foundational

**Purpose**: Build the approved integral initial schema and shared services.
The schema design is approved; no implementation may start until the plan and
tasks themselves are approved in accordance with the Constitution.
The user explicitly approved the plan and these tasks for implementation on
2026-10-09.

- [x] T004 Confirm the global Decision identifier suffix rule provided by the product owner and record it in `specs/003-design-authority-mvp/spec.md`, `specs/003-design-authority-mvp/research.md`, `specs/003-design-authority-mvp/data-model.md`, and `specs/003-design-authority-mvp/plan.md`; the suffix does not reset by Trámite or Subetapa.
- [x] T005 Record the user's explicit approval of the consolidated initial schema in `specs/003-design-authority-mvp/data-model.md` and `specs/003-design-authority-mvp/plan.md`, including Trámite/Subetapa composite references, TeamTechnology kind restrictions, audit/soft-delete fields, Person and Role reuse, configured user, reference catalogs, ID counters, participant records, and single-current-Minuta storage. Informe Semanal and Excel are excluded from this initial schema and will be added through later migrations.
- [x] T006 After T004 and T005, create the approved, versioned SQL Server DDL and initial reference-data script in `database/scripts/001-initial-schema.sql`; include all confirmed entities and constraints in `data-model.md`, seed the fixed TeamTechnology and Role catalogs and exact functional enumerations from `spec.md`, seed each consecutive-ID counter at `0`, and do not seed an assumed default Trámite or invent catalog values.
- [x] T007 [P] Implement shared Domain entities/value types and audit metadata in `src/DesignAuthority.Domain/Entities/` and `src/DesignAuthority.Domain/ValueObjects/` for Trámite, Subetapa, TeamTechnology (`Kind` is exactly `Technology` or `Team`), Role, Persona, and the configured user; preserve one TeamTechnology and one valid Role per Persona and do not implement CRUD behavior for these prerequisite catalogs.
- [x] T008 [P] Define shared Application repository, transaction, configured-user, file-storage, and result contracts in `src/DesignAuthority.Application/Abstractions/`; include the repository responsibilities listed in `specs/003-design-authority-mvp/contracts/application-mvc.md` and expose no REST/API contracts.
- [x] T009 Implement Dapper SQL Server connection creation, parameterized command helpers, transaction coordination, counter reservation, and DI registration in `src/DesignAuthority.Infrastructure/Persistence/`, `src/DesignAuthority.Infrastructure/DependencyInjection/`, and `src/DesignAuthority.Web/Program.cs`; ensure counter reservation is committed atomically with entity insertion and rolled back on failure.
- [x] T010 Implement the configured-user context, external options binding, structured operational logging, and user-safe MVC error handling in `src/DesignAuthority.Infrastructure/Configuration/` and `src/DesignAuthority.Web/Program.cs` / `src/DesignAuthority.Web/Views/Shared/`; do not introduce authentication, authorization, or secrets in tracked configuration.
- [x] T011 Create shared MVC layout and Bootstrap asset integration in `src/DesignAuthority.Web/Views/Shared/_Layout.cshtml` and `src/DesignAuthority.Web/wwwroot/css/site.css`; provide navigation only for the four in-scope modules and do not add catalog CRUD pages.

**Checkpoint**: Shared model, approved and versioned schema, Dapper/DI,
configured-user auditing, and MVC shell are ready. Confirm no Entity Framework,
REST API, authentication, or authorization was introduced.

---

## Phase 3: User Story 1 - Registrar y dar seguimiento a Hallazgos (Priority: P1) 🎯 MVP

**Goal**: Create, consult, modify, follow up, and logically delete Hallazgos
with their Trámite, active same-Trámite Subetapa, Technology, and responsible
Persona.

**Independent Test**: With the required catalogs present, create and list a
Hallazgo; edit without changing its Id; append multiple independently
preserved Seguimientos; verify same-Trámite Subetapa validation, allowed
filter/sort/pagination behavior, soft deletion, and audit attribution.

### Tests for User Story 1

- [ ] T012 [P] [US1] Add Domain tests in `tests/DesignAuthority.Domain.Tests/Findings/FindingTests.cs` for required Hallazgo fields, initial status Abierto, immutable identifier behavior, and references to one Trámite, one Subetapa, one Technology, and one responsible Persona.
- [ ] T013 [P] [US1] Add Application tests in `tests/DesignAuthority.Application.Tests/Findings/FindingUseCasesTests.cs` proving invalid required fields, inactive/wrong-Trámite Subetapas, invalid Technology kinds, and invalid status references produce validation failures with no repository writes.
- [ ] T014 [P] [US1] Add SQL Server integration tests in `tests/DesignAuthority.Infrastructure.IntegrationTests/Findings/FindingRepositoryTests.cs` proving Hallazgo IDs start at 1 and increase consecutively, counter and insert roll back together, composite Trámite/Subetapa and Technology-kind constraints reject invalid associations, and soft delete preserves Seguimientos.
- [ ] T015 [P] [US1] Add MVC integration tests in `tests/DesignAuthority.Web.IntegrationTests/Findings/FindingsFlowTests.cs` for create, edit, list filters/sorting/pagination, preselection of the active default Trámite when one exists and manual active Trámite selection when none exists, Abierto initial status selection, user-safe validation feedback, and required visible Id/Technology/description columns.

### Implementation for User Story 1

- [x] T016 [US1] Implement `Finding` and `FindingFollowUp` Domain entities and rules in `src/DesignAuthority.Domain/Entities/Finding.cs` and `src/DesignAuthority.Domain/Entities/FindingFollowUp.cs`; quote and enforce these model constraints: “`FindingId` Numeric, unique, immutable, consecutive from 1”; “`RegistrationDate` Required registration date”; “`TramiteId`, `SubstageId` Required; composite reference ensures Substage belongs to the same Trámite”; “`TeamTechnologyId`, `TeamTechnologyKind` Required Technology association; kind constrained to `Technology`”; “`Description` Required”; “`ResponsiblePersonId` Required Persona reference”; “`CommitmentDate` Required”; “`SolutionDate` Optional”; and “Abierto is the initial status selection; any listed status may be selected on create or edit.”
- [x] T017 [US1] Implement Application commands, queries, validation, list filters, allowed sorting, and pagination in `src/DesignAuthority.Application/Features/Findings/`; use only the exact Category, Status, Criticality, Priority, Registration phase, and Attention phase values in `specs/003-design-authority-mvp/data-model.md`; preselect the active default Trámite for new records when one exists and otherwise require manual selection of an active Trámite; offer only active Subetapas belonging to the selected Trámite and do not add a same-Technology rule for responsible Persona.
- [x] T018 [P] [US1] Implement parameterized Dapper Hallazgo and FindingFollowUp persistence in `src/DesignAuthority.Infrastructure/Repositories/FindingRepository.cs` and `src/DesignAuthority.Infrastructure/Repositories/FindingFollowUpRepository.cs`; create/update/soft-delete Hallazgos, append independent Seguimientos with date/configured user/observation and optional old/new status and commitment dates, exclude inactive Hallazgos from active lists, and never physically delete follow-up history.
- [x] T019 [P] [US1] Implement Hallazgo MVC controller, typed ViewModels, and create/edit/details views in `src/DesignAuthority.Web/Controllers/FindingsController.cs`, `src/DesignAuthority.Web/ViewModels/Findings/`, and `src/DesignAuthority.Web/Views/Findings/`; validate all inputs server-side, preselect the active default Trámite when available (otherwise require manual selection), preserve the immutable Id, default the form selection to Abierto while permitting any allowed status, and redisplay validation errors without partial writes.
- [x] T020 [P] [US1] Implement the Hallazgos list and Seguimientos UI in `src/DesignAuthority.Web/Views/Findings/Index.cshtml`, `src/DesignAuthority.Web/Views/Findings/FollowUps.cshtml`, and `src/DesignAuthority.Web/wwwroot/css/site.css`; support description search, Technology/status filters, Id/Technology/status/registration-date sorting, pagination, column customization with Id/Technology/description permanently visible, and append-only follow-up entry/history.

**Checkpoint**: US1 is independently usable; invalid writes leave no partial
data, and soft deletion preserves all existing Seguimientos.

---

## Phase 4: User Story 2 - Registrar y dar seguimiento a Acuerdos (Priority: P1)

**Goal**: Create, consult, modify, follow up, and logically delete Acuerdos
with a Trámite, Technology, responsible Persona, and preserved independent
Seguimientos. Acuerdos have no Subetapa association.

**Independent Test**: Create and edit an Acuerdo without changing its Id;
append multiple Seguimientos; verify permitted status values, list behavior,
audit fields, and soft deletion without losing Seguimientos.

### Tests for User Story 2

- [ ] T021 [P] [US2] Add Domain tests in `tests/DesignAuthority.Domain.Tests/Agreements/AgreementTests.cs` for required Acuerdo fields, Abierto initial selection, immutable consecutive identifier, and absence of a Subetapa relationship.
- [ ] T022 [P] [US2] Add Application tests in `tests/DesignAuthority.Application.Tests/Agreements/AgreementUseCasesTests.cs` proving required-field and reference validation, all allowed statuses on create/edit, no Subetapa input, and no writes after invalid requests.
- [ ] T023 [P] [US2] Add SQL Server integration tests in `tests/DesignAuthority.Infrastructure.IntegrationTests/Agreements/AgreementRepositoryTests.cs` for consecutive IDs, Technology-only FK, transactional counter rollback, independent follow-up rows, audit values, and soft deletion preserving follow-ups.
- [ ] T024 [P] [US2] Add MVC integration tests in `tests/DesignAuthority.Web.IntegrationTests/Agreements/AgreementsFlowTests.cs` for create/edit/list/filter/sort/pagination/follow-up flows, preselection of the active default Trámite when one exists and manual active Trámite selection when none exists, Abierto initial status selection, and the required always-visible Id/Technology/description columns.

### Implementation for User Story 2

- [x] T025 [US2] Implement `Agreement` and `AgreementFollowUp` Domain entities in `src/DesignAuthority.Domain/Entities/Agreement.cs` and `src/DesignAuthority.Domain/Entities/AgreementFollowUp.cs`; quote and enforce these model constraints: “`AgreementId` Numeric, unique, immutable, consecutive from 1”; “`RegistrationDate` Required registration date”; “`TramiteId` Required”; “`TeamTechnologyId`, `TeamTechnologyKind` Required Technology association; kind constrained to `Technology`”; “`Description` Required”; “`ResponsiblePersonId` Required Persona reference”; “`CommitmentDate` Required”; “`SolutionDate` Optional”; and “Abierto is the initial status selection; any listed status may be selected on create or edit”; do not add a Subetapa field.
- [x] T026 [US2] Implement Application commands, queries, validation, allowed Acuerdo statuses, initial list restriction, approved filters/sort keys, and pagination in `src/DesignAuthority.Application/Features/Agreements/`; preselect the active default Trámite for new records when one exists and otherwise require manual selection of an active Trámite; require Technology, description, responsible Persona, registration date, status, and commitment date, with optional solution date, and reject unlisted statuses.
- [x] T027 [P] [US2] Implement parameterized Dapper Acuerdo and AgreementFollowUp repositories in `src/DesignAuthority.Infrastructure/Repositories/AgreementRepository.cs` and `src/DesignAuthority.Infrastructure/Repositories/AgreementFollowUpRepository.cs`; allocate consecutive IDs transactionally, append follow-ups independently, soft-delete Acuerdos, filter inactive rows from active lists, and preserve follow-up records.
- [x] T028 [P] [US2] Implement Acuerdo MVC controller, ViewModels, and create/edit/details/list/follow-up views in `src/DesignAuthority.Web/Controllers/AgreementsController.cs`, `src/DesignAuthority.Web/ViewModels/Agreements/`, and `src/DesignAuthority.Web/Views/Agreements/`; preselect the active default Trámite when available (otherwise require manual selection), provide description search, Technology/status filters, Id/Technology/status/registration-date sorting, pagination, required visible columns, Abierto initial status selection, and server-side validation.

**Checkpoint**: US2 works independently of US1 behavior; Acuerdo records do
not expose or persist a Subetapa association.

---

## Phase 5: User Story 3 - Registrar Decisiones de Arquitectura (Priority: P2)

**Goal**: Create, consult, modify, and logically delete Decisions with a
Technology, responsible Team, and a Subetapa belonging to the same Trámite.

**Independent Test**: Create and list Decisions with the specified global
consecutive identifier and required fields; verify the active default Trámite
is preselected when present and an active Trámite can be selected manually
when absent; edit without changing the Id; verify the same-Trámite constraint,
Technology-versus-Team classification, filters, ordering, and soft deletion.

### Tests for User Story 3

- [ ] T029 [P] [US3] Add Domain tests in `tests/DesignAuthority.Domain.Tests/Decisions/ArchitectureDecisionTests.cs` for required fields, immutable formatted Id, same-Trámite Subetapa association, Technology association, and Team responsible association.
- [ ] T030 [P] [US3] Add Application tests in `tests/DesignAuthority.Application.Tests/Decisions/DecisionUseCasesTests.cs` for required-field/reference validation, the approved status values, same-Trámite validation, immutable Id, and invalid-request no-write behavior.
- [ ] T031 [P] [US3] Add SQL Server integration tests in `tests/DesignAuthority.Infrastructure.IntegrationTests/Decisions/DecisionRepositoryTests.cs` for approved suffix sequence allocation and rollback, composite Trámite/Subetapa integrity, rejection of Team as Technology and Technology as responsible Team, and soft deletion.
- [ ] T032 [P] [US3] Add MVC integration tests in `tests/DesignAuthority.Web.IntegrationTests/Decisions/DecisionsFlowTests.cs` for create/edit/list/search/filter/sort/pagination, preselection of the active default Trámite when one exists and manual active Trámite selection when none exists, required visible Id/Technology/problem-description columns, and validation feedback.

### Implementation for User Story 3

- [x] T033 [US3] Implement `ArchitectureDecision` in `src/DesignAuthority.Domain/Entities/ArchitectureDecision.cs`; quote and enforce these model constraints: “`DecisionId` Unique, immutable formatted identifier”; “`TramiteId`, `SubstageId` Required; composite FK enforces same-Trámite membership”; “`TeamTechnologyId`, `TeamTechnologyKind` Required Technology association; kind constrained to `Technology`”; “`ResponsibleTeamTechnologyId`, `ResponsibleKind` Required responsible Team; kind constrained to `Team`”; “`ProblemDescription`, `SolutionApproach`, `DecisionText` Required”; and “`RegistrationDate` Required registration date”; do not impose an active-only Subetapa requirement.
- [x] T034 [US3] Implement Decision Application commands, queries, validation, and identifier formatting in `src/DesignAuthority.Application/Features/Decisions/`; allocate the globally consecutive suffix across all Trámites and Subetapas, preselect the active default Trámite for new records when one exists and otherwise require manual selection of an active Trámite, preserve the stored Id on edits, permit the specified statuses, and filter/sort/paginate exactly as defined for Decisiones.
- [x] T035 [P] [US3] Implement parameterized Dapper Decision repository and transactional identifier allocation in `src/DesignAuthority.Infrastructure/Repositories/ArchitectureDecisionRepository.cs`; persist the composite same-Trámite association, fixed Technology/Team discriminators, audit fields, and soft-delete state without imposing Subetapa activity rules.
- [x] T036 [P] [US3] Implement Decision MVC controller, ViewModels, and create/edit/details/list views in `src/DesignAuthority.Web/Controllers/DecisionsController.cs`, `src/DesignAuthority.Web/ViewModels/Decisions/`, and `src/DesignAuthority.Web/Views/Decisions/`; preselect the active default Trámite when available (otherwise require manual selection), show Technology choices separately from Team choices, require the responsible Team, keep Id immutable, and support problem-description search, Technology/status filters, registration-date sorting, pagination, and mandatory visible columns.

**Checkpoint**: US3 is independently usable after the approved schema and
foundation are implemented; its identifier generation follows the global
suffix rule in the specification.

---

## Phase 6: User Story 4 - Registrar Reuniones y su Minuta vigente (Priority: P2)

**Goal**: Create, consult, modify, and logically delete Reuniones with
participants and one current Minuta, without a Subetapa relationship or
historical document versions.

**Independent Test**: Save a Reunión with at least one attendee and optionally
no absentees; verify required fields, list behavior, audit/soft delete, upload
validation, relative file reference, replacement of the current Minuta, and
absence of retained version history.

### Tests for User Story 4

- [ ] T037 [P] [US4] Add Domain tests in `tests/DesignAuthority.Domain.Tests/Meetings/MeetingTests.cs` for required Reunión fields, immutable consecutive numeric Id, Technology association, absence of Subetapa, and at least one attendee.
- [ ] T038 [P] [US4] Add Application tests in `tests/DesignAuthority.Application.Tests/Meetings/MeetingUseCasesTests.cs` for required-field validation, empty absentees, at least one attendee, unique attendance per Persona, file extension/size checks, relative-path metadata, and no partial writes on invalid requests.
- [ ] T039 [P] [US4] Add SQL Server integration tests in `tests/DesignAuthority.Infrastructure.IntegrationTests/Meetings/MeetingRepositoryTests.cs` for consecutive Meeting IDs, parent/participant atomicity, one current Minuta row per Meeting, audit fields, and soft delete preserving related traceability data.
- [ ] T040 [P] [US4] Add MVC/storage integration tests in `tests/DesignAuthority.Web.IntegrationTests/Meetings/MeetingsFlowTests.cs` for create/edit/list/participants/Minuta upload and replacement, preselection of the active default Trámite when one exists and manual active Trámite selection when none exists, required visible columns, safe errors, and `.docx` size boundary behavior.

### Implementation for User Story 4

- [x] T041 [US4] Implement `Meeting`, `MeetingParticipant`, and current `MeetingMinute` Domain entities in `src/DesignAuthority.Domain/Entities/Meeting.cs`, `src/DesignAuthority.Domain/Entities/MeetingParticipant.cs`, and `src/DesignAuthority.Domain/Entities/MeetingMinute.cs`; quote and enforce these model constraints: “`MeetingId` Numeric, unique, immutable, consecutive from 1”; “`TramiteId` Required”; “`TeamTechnologyId`, `TeamTechnologyKind` Required Technology association; kind constrained to `Technology`”; “`MeetingDate`, `MeetingTime` Required date and time”; “`StatusId` Required”; “`AgreementsSummary`, `CommitmentsSummary`, `Subject` Required”; “`Comments` Optional”; “at least one attendee”; and “zero or one current Minuta”.
- [x] T042 [US4] Implement Meeting Application commands, queries, participant validation, list filters/sorts, pagination, and soft-delete behavior in `src/DesignAuthority.Application/Features/Meetings/`; preselect the active default Trámite for new records when one exists and otherwise require manual selection of an active Trámite; require one or more attendees, allow an empty absence list, ensure a Persona is not both attended and absent for one Meeting, and do not associate a Subetapa.
- [x] T043 [P] [US4] Implement parameterized Dapper Meeting, MeetingParticipant, and MeetingMinute repositories in `src/DesignAuthority.Infrastructure/Repositories/MeetingRepository.cs`, `src/DesignAuthority.Infrastructure/Repositories/MeetingParticipantRepository.cs`, and `src/DesignAuthority.Infrastructure/Repositories/MeetingMinuteRepository.cs`; allocate Meeting IDs transactionally, persist Meeting and participant rows atomically, enforce one current Minuta row, store relative references only, and soft-delete without physically deleting traceability rows.
- [x] T044 [P] [US4] Implement configured-root file storage and Minuta staging/replace/cleanup in `src/DesignAuthority.Infrastructure/FileStorage/LocalFileStorage.cs` and `src/DesignAuthority.Infrastructure/FileStorage/MinuteCleanupWorker.cs`; accept only `.docx` files up to `5 * 1024 * 1024` bytes, store the original filename plus generated storage filename and relative path, never persist an absolute path, and delete the previous document after the current database reference is committed without retaining a user-visible version history.
- [x] T045 [P] [US4] Implement Meeting MVC controller, typed ViewModels, and create/edit/details/list/participant/Minuta views in `src/DesignAuthority.Web/Controllers/MeetingsController.cs`, `src/DesignAuthority.Web/ViewModels/Meetings/`, and `src/DesignAuthority.Web/Views/Meetings/`; preselect the active default Trámite when available (otherwise require manual selection), support subject search, Id/Technology/subject/date sorting, pagination, column customization with required columns visible, and server-side attendee/file validation.

**Checkpoint**: US4 is independently usable; a failed upload or reference
update is not reported as successful and no previous Minuta is exposed as a
version.

---

## Phase 7: Polish & Cross-Cutting Concerns

**Purpose**: Validate integration and consistency across the four stories.

- [ ] T046 [P] Align shared audit fields, configured-user attribution, active-list filters, and operational logging across `src/DesignAuthority.Application/`, `src/DesignAuthority.Infrastructure/`, and `src/DesignAuthority.Web/`; confirm all write failures are surfaced and user messages do not disclose SQL, secrets, stack traces, or absolute file paths.
- [x] T047 [P] Complete the four module navigation links and verify the shared Bootstrap/CSS layout in `src/DesignAuthority.Web/Views/Shared/_Layout.cshtml` and `src/DesignAuthority.Web/wwwroot/css/site.css`; confirm each list keeps its Id, Technology, and required descriptive column visible.
- [x] T048 Review the approved SQL script and repository queries in `database/scripts/001-initial-schema.sql` and `src/DesignAuthority.Infrastructure/Repositories/` for parameterized values, allowlisted sort identifiers, composite FK consistency, versioned reference data, and absence of Entity Framework.
- [x] T049 Run the solution restore/build and all Domain, Application, SQL Server integration, and MVC integration tests using `dotnet restore .\DesignAuthority.sln` and `dotnet test .\DesignAuthority.sln`; record the actual results in the implementation review and do not claim success for unavailable SQL Server-dependent tests.
- [ ] T050 Execute the acceptance scenarios in `specs/003-design-authority-mvp/quickstart.md`, update that guide only for validated implementation commands, and confirm the out-of-scope items remain absent from `src/` and `database/scripts/`.

---

## Dependencies & Execution Order

### Phase dependencies

- **Setup (Phase 1)**: creates the solution and baseline configuration.
- **Foundational (Phase 2)**: begins after setup; T004 and T005 are complete.
  T006 and shared persistence depend on the approved schema. User-story
  implementation depends on the versioned schema and shared foundation.
- **User Stories (Phases 3–6)**: depend on the foundational model, database
  schema, Dapper infrastructure, and MVC shell. The stories may be assigned in
  parallel after Phase 2, subject to identifier-dependent behavior in US3.
- **Polish (Phase 7)**: depends on all stories selected for the MVP.

### User-story dependencies

- **US1 (P1)**: depends only on Phase 2; independent of other story behavior.
- **US2 (P1)**: depends only on Phase 2; independent of US1 behavior.
- **US3 (P2)**: depends on Phase 2; its suffix allocation follows the global
  scope already clarified in the specification.
- **US4 (P2)**: depends only on Phase 2; independent of US1–US3 behavior.

### Parallel opportunities identified

- Setup tasks T002 and T003 can be performed in parallel after T001.
- After plan and task approval, T007 and T008 can be developed in parallel; T009 and
  T010 follow their contracts and can be divided by project after those
  contracts are stable.
- Within each story, its Domain/Application/SQL/MVC test files (T012–T015,
  T021–T024, T029–T032, T037–T040) are separate and can be prepared in
  parallel. The tests should run before implementation and fail for missing
  behavior.
- After each story's Application behavior is defined, its Dapper repository
  and MVC surface can be developed in parallel in separate files/projects.
- After Phase 2, US1, US2, and US4 can be implemented in parallel; US3 can
  join when the shared foundation is ready.

### Parallel example: User Story 1

```text
After T008 establishes the contracts, prepare the tests in parallel:
  T012 Finding Domain tests
  T013 Finding Application validation tests
  T014 Finding SQL Server repository/constraint tests
  T015 Finding MVC flow tests

After T016–T017 define the Domain and Application behavior:
  T018 Finding Dapper repositories
  T019 Finding create/edit/details MVC flow
  T020 Finding list and follow-up MVC views
```

## Implementation Strategy

### MVP first (User Story 1)

1. Complete Phase 1 setup.
2. With T004 and T005 complete, implement the approved, versioned schema and
   shared foundation before any dependent user-story code.
3. Complete US1 tests and implementation (Phase 3).
4. Validate the US1 independent test criteria and the Hallazgo scenarios in
   `quickstart.md`.
5. Continue with US2, then US3/US4 as capacity permits.

### Incremental delivery

1. Phase 1 + Phase 2 → approved, usable shared foundation.
2. US1 → Hallazgo and Seguimiento end-to-end.
3. US2 → Acuerdo and Seguimiento end-to-end.
4. US3 → Decision end-to-end using the globally consecutive suffix confirmed
   in the specification.
5. US4 → Reunión, participants, and single-current-Minuta end-to-end.
6. Phase 7 → cross-story validation against `quickstart.md`.

## Notes

- `[P]` marks tasks that operate on independent files after their stated
  prerequisites; `[US#]` maps each story task to `spec.md`.
- No task implements Informe Semanal, Excel, AI, authentication, authorization,
  REST APIs, or CRUD for manually maintained Trámite/Subetapa catalogs.
- Use the confirmed global Decision suffix scope. Do not infer an active-Person
  assignment rule or an active-Subetapa rule for Decisions.
- The initial schema is approved; the SQL script and dependent implementation
  remain subject to approval of this plan and these tasks, now recorded in
  `plan.md` and this task file.
- Do not commit credentials, connection strings, or secrets.
