# Data Model: MVP Operativo de Design Authority

## Scope and status

This is the consolidated logical/technical model for the MVP entities defined
in features 001–003. The user approved it as the initial MVP schema in the
clarification recorded in `spec.md`. The plan and implementation tasks have
also been explicitly approved; this model is the basis for the versioned
initial SQL script and production implementation.

This initial model does not define Informe Semanal or Excel functionality;
both are out of scope and will be incorporated through later migrations.
It also does not add a generic Subetapa relationship to Acuerdo or Reunión.

## Shared conventions

- Suggested primary keys use SQL Server `int` unless a formatted external
  identifier is required. Numeric Hallazgo, Acuerdo, and Reunión identifiers
  are allocated from transactionally updated counters, not `IDENTITY`.
- `CreatedAtUtc` and `CreatedByUserId` record creation; nullable
  `UpdatedAtUtc` and `UpdatedByUserId` record the latest modification.
- For logically deletable records, `IsActive`, `DeletedAtUtc`, and
  `DeletedByUserId` record the current soft-delete state and event. Delete
  operations also update the modification audit fields.
- User references point to the one configured user. No password, login flow,
  or authentication record is introduced.
- Status/reference values are versioned seed data. Functional rows reference
  them by key; display labels are not used as identifiers.
- Functional entity deletes are logical. Foreign-key history is not cascaded
  into physical deletion.

## Entities

### ConfiguredUser

One seeded row representing the configured Design Authority user.

| Field | Rule |
|---|---|
| `ConfiguredUserId` | Technical primary key; seeded stable value. |
| `DisplayName` | Name used in audit/activity display; source from the configured user data. |

There are no credentials or authentication attributes. The exact user profile
fields remain those defined by the configured-user setup; this feature does
not add identity-provider data.

### Tramite

| Field | Rule |
|---|---|
| `TramiteId` | Primary key. |
| `Name` | Required catalog name. |
| `Description` | Optional. |
| `IsDefault` | Indicates the manually managed default selection. |
| `IsActive` | Inactive Trámites are unavailable for new records. |
| Audit fields | Creation/modification metadata as applicable. |

Database integrity must allow zero or one active default and must not require
that any default exist. A filtered unique constraint over active defaults is
the proposed enforcement. Trámite is stored on each record and changing the
default never rewrites historical associations. No default Trámite name or
seed is assumed.

### Substage

| Field | Rule |
|---|---|
| `SubstageId` | Primary key. |
| `TramiteId` | Required parent Trámite. |
| `Name` | Required catalog name. |
| `Description` | Optional. |
| `IsActive` | State required by the catalog specification. |
| Audit fields | Creation/modification metadata as applicable. |

Expose a candidate key `(TramiteId, SubstageId)` for same-Trámite foreign
keys. No Subetapa-specific CRUD is added here.

### TeamTechnology

Reuses the unified catalog from feature 002.

| Field | Rule |
|---|---|
| `TeamTechnologyId` | Primary key. |
| `Code` | Stable unique reference code. |
| `Name` | Unique display name. |
| `Kind` | Required discriminator: `Technology` or `Team`. |

Initial rows:

| Name | Kind |
|---|---|
| Appian | Technology |
| Databricks | Technology |
| Microservicios | Technology |
| Power BI | Technology |
| Arquitectura | Team |
| Proyecto | Team |
| Célula | Team |
| QA | Team |

Expose `(TeamTechnologyId, Kind)` as a candidate key. The four functional
modules must reference `Kind = Technology`; a Decision's responsible
association must reference `Kind = Team`. Persona may reference either kind.

### Role

| Field | Rule |
|---|---|
| `RoleId` | Primary key. |
| `TeamTechnologyId` | Required parent catalog row. |
| `Code`, `Name` | Stable role code and display name, unique within its parent. |
| `IsLeadership` | Identifies roles with the specified zero-or-one active-person capacity. |

The 15 role assignments and leadership classification are exactly those in
`specs/002-equipos-tecnologias/data-model.md`. A unique candidate key
`(TeamTechnologyId, RoleId)` supports Persona's composite reference. No
multiple-role assignment is modeled.

### Person

Reuses feature 002; there is no duplicate Persona catalog.

| Field | Rule |
|---|---|
| `PersonId` | Unique immutable primary key; `int IDENTITY(1,1)` is the existing technical proposal in feature 002. |
| `Name`, `PaternalSurname`, `AbbreviatedName` | Required. |
| `MaternalSurname` | Optional. |
| `TeamTechnologyId`, `RoleId` | Required, one of each; composite FK ensures Role belongs to the selected TeamTechnology. |
| `CreatedAtUtc`, `CreatedByUserId` | Creation audit; `CreatedAtUtc` is also the functional registration date. |
| `UpdatedAtUtc`, `UpdatedByUserId` | Latest modification audit. |
| `IsActive`, `DeletedAtUtc`, `DeletedByUserId` | Soft-delete metadata. |

Existing role-capacity behavior remains from feature 002: each active leadership
Role has zero or one active Persona; the other listed roles have zero or more.
The database write path must serialize leadership-role occupancy checks on
the relevant Role. Do not add multiple assignment tables.

### Functional reference values

Store the specification's fixed enumerations in versioned reference tables.
Do not add values or transition rules.

| Reference set | Values |
|---|---|
| Hallazgo category | Documental; Convenciones; Lineamientos y estándares; Reutilización; Diseños; Uso de Patrones; Buenas prácticas |
| Hallazgo status | Abierto; En progreso; Completado; Cancelado; Bloqueado; No entregó |
| Acuerdo status | Abierto; En progreso; Completado; Cancelado; Bloqueado; No entregó |
| Decision status | Abierto; En progreso; Cancelado; Cerrado |
| Meeting status | Realizada; Equipo no asistió a la reunión; Reprogramada; Cancelada |
| Criticality | No aplica; Crítico; Mayor; Menor; Sugerencia |
| Priority | Alta; Media; Baja |
| Registration phase | No aplica; Antes del desarrollo; Durante el desarrollo; Después del desarrollo; No entregó |
| Attention phase | Diseño; Construcción; Pruebas; Certificación; Soporte; Mantenimiento; En validación; No aplica |

Catalog tables have stable keys/codes and labels; they are seeded and not
managed by CRUD screens in this feature.

### ConsecutiveIdentifierCounter

Infrastructure table with a stable counter key per identifier family and the
last committed number. Seed each family with last committed value `0` so the
first reservation is `1`. Reserve/increment and insert the corresponding
functional record in one transaction. Lock only the relevant counter row
during allocation. The update must roll back if the record insert fails.

Families are Hallazgo, Acuerdo, Reunión, and the Decision suffix. The Decision
suffix is globally allocated across all Trámites and Subetapas, as confirmed
in the specification. Its counter is shared by every Decision.

### Finding (Hallazgo)

| Field | Rule |
|---|---|
| `FindingId` | Numeric, unique, immutable, consecutive from 1. |
| `RegistrationDate` | Required registration date. |
| `TramiteId`, `SubstageId` | Required; composite reference ensures Substage belongs to the same Trámite. |
| `TeamTechnologyId`, `TeamTechnologyKind` | Required Technology association; kind constrained to `Technology`. |
| `CategoryId`, `StatusId`, `CriticalityId`, `PriorityId`, `RegistrationPhaseId`, `AttentionPhaseId` | Required references to the allowed values above. Abierto is the initial status selection; any listed status may be selected on create or edit. |
| `Description` | Required. |
| `ResponsiblePersonId` | Required Persona reference. |
| `CommitmentDate` | Required. |
| `SolutionDate` | Optional. |
| Audit and soft-delete fields | Required as applicable. |

One Hallazgo has zero or more independent FindingFollowUp rows. The model
does not constrain the responsible Person to the same Technology or Role,
because that rule is not in the specification.

### FindingFollowUp (Seguimiento de Hallazgo)

One independent row per follow-up:

| Field | Rule |
|---|---|
| `FindingFollowUpId` | Primary key. |
| `FindingId` | Required parent Hallazgo. |
| `FollowUpDate`, `UserId`, `Observation` | Required follow-up date, configured user, and observation. |
| `OldStatusId`, `NewStatusId` | Optional old/new status pair when status changed. |
| `OldCommitmentDate`, `NewCommitmentDate` | Optional old/new date pair when commitment date changed. |

Create each follow-up as a separate record; parent updates never rewrite
follow-up data. Follow-up date/user fields preserve the event details required
by the specification. This feature does not define separate follow-up
edit/delete operations or impose an additional immutability rule.

### Agreement (Acuerdo)

| Field | Rule |
|---|---|
| `AgreementId` | Numeric, unique, immutable, consecutive from 1. |
| `RegistrationDate` | Required registration date. |
| `TramiteId` | Required. |
| `TeamTechnologyId`, `TeamTechnologyKind` | Required Technology association; kind constrained to `Technology`. |
| `Description` | Required. |
| `ResponsiblePersonId` | Required Persona reference. |
| `StatusId` | Required reference; Abierto is the initial status selection; any listed status may be selected on create or edit. |
| `CommitmentDate` | Required. |
| `SolutionDate` | Optional. |
| Audit and soft-delete fields | Required as applicable. |

An Acuerdo has zero or more independent AgreementFollowUp rows. There is no
Subetapa field or relationship.

### AgreementFollowUp (Seguimiento de Acuerdo)

Same event shape as FindingFollowUp: independent primary key, required
Agreement FK, follow-up date, configured user, observation, and optional
old/new status and commitment-date pairs. No follow-up overwrites an earlier
row.

### ArchitectureDecision (Decisión de Arquitectura)

| Field | Rule |
|---|---|
| `DecisionId` | Unique, immutable formatted identifier. Proposed form uses stable `TramiteId` and `SubstageId` identifiers plus a globally consecutive suffix of at least three digits. |
| `TramiteId`, `SubstageId` | Required; composite FK enforces same-Trámite membership. |
| `TeamTechnologyId`, `TeamTechnologyKind` | Required Technology association; kind constrained to `Technology`. |
| `ResponsibleTeamTechnologyId`, `ResponsibleKind` | Required responsible Team; kind constrained to `Team`. |
| `ProblemDescription`, `SolutionApproach`, `DecisionText` | Required values per feature specification. |
| `RegistrationDate` | Required registration date. |
| `StatusId` | Required Decision status reference. Any listed status may be selected; no transition sequence is introduced. |
| Audit and soft-delete fields | Required as applicable. |

The spec requires the Subetapa to belong to the same Trámite but does not say
that it must be active; do not add an active-only rule without clarification.
The displayed prefix uses stable keys rather than mutable catalog names so an
immutable identifier cannot change when a label is edited.

### Meeting (Reunión)

| Field | Rule |
|---|---|
| `MeetingId` | Numeric, unique, immutable, consecutive from 1. |
| `TramiteId` | Required. |
| `TeamTechnologyId`, `TeamTechnologyKind` | Required Technology association; kind constrained to `Technology`. |
| `MeetingDate`, `MeetingTime` | Required date and time. |
| `StatusId` | Required Meeting status reference. |
| `AgreementsSummary`, `CommitmentsSummary`, `Subject` | Required. |
| `Comments` | Optional. |
| Audit and soft-delete fields | Required as applicable. |

There is intentionally no Subetapa field. A Meeting has one or more meeting
participants; at least one must have `Attendance = Attended`.

### MeetingParticipant (asistentes y ausentes)

| Field | Rule |
|---|---|
| `MeetingId`, `PersonId` | Required composite primary key and foreign keys. |
| `Attendance` | Required value distinguishing attended from absent. |

One row per Person per Meeting means a Person cannot appear in both lists for
the same Meeting. Application validation enforces at least one attendee in
the complete submitted set before saving; the parent and participant rows are
committed atomically. The specification does not define a stricter active
Persona selection rule.

### MeetingMinute (Minuta vigente)

One row at most per Meeting, with `MeetingId` as primary key:

| Field | Rule |
|---|---|
| `MeetingId` | Required FK to Meeting and primary key, enforcing at most one current Minuta. |
| `OriginalFileName` | Required original upload name. |
| `StorageFileName` | Required generated physical name or internal identifier. |
| `RelativePath` | Required path relative to configured storage root; never an absolute server path. |
| Audit fields | Creation and latest replacement user/time, as applicable. |

Replacing a Minuta updates this row and removes the previous physical file;
it does not create a previous-version row. Store only `.docx` files up to
`5 * 1024 * 1024` bytes. The infrastructure cleanup queue described in the
plan is operational metadata only, not a user-visible version history.

### Informe Semanal

No table is designed in this feature. It is a domain concept recognized by
feature 001 but is expressly out of scope here; its fields, relations,
retention, and behavior must be specified before a future schema change.

## Relationship and constraint summary

| Relationship | Constraint |
|---|---|
| Trámite → Substage | One Trámite to zero or more Subetapas; each Subetapa has one Trámite. |
| Finding → Trámite/Substage | Both required; composite FK enforces the same Trámite. |
| Decision → Trámite/Substage | Both required; composite FK enforces the same Trámite. |
| Agreement → Trámite | Required; no Subetapa relationship. |
| Meeting → Trámite | Required; no Subetapa relationship. |
| Finding, Agreement, Decision, Meeting → TeamTechnology | Required Technology kind; reject `Team` rows. |
| Decision → responsible TeamTechnology | Required Team kind; reject `Technology` rows. |
| Person → TeamTechnology/Role | Exactly one of each; composite FK ensures the Role belongs to the selected catalog item. |
| Finding → FindingFollowUp | Zero or more independent rows. |
| Agreement → AgreementFollowUp | Zero or more independent rows. |
| Meeting → MeetingParticipant | One or more participants in a valid saved Meeting, including at least one attendee. |
| Meeting → MeetingMinute | Zero or one current row. |
| Audited records → ConfiguredUser | Required user for create; nullable update/delete user fields until the event applies. |

Soft deletion applies to Hallazgos, Acuerdos, Decisiones, Reuniones, and
Personas as specified. Active list queries exclude those rows. Soft deleting
a parent does not physically delete its follow-ups, participants, or Minuta
metadata. Catalog records are manually maintained; their active status and
foreign keys preserve historical context.

## Validation and transaction boundaries

1. MVC validates bound input and redisplays invalid forms; Application
   repeats all business validation before calling a repository.
2. Application checks allowed references and the same-Trámite relationship;
   composite SQL foreign keys backstop the critical reference constraints.
3. Each counter allocation and corresponding record insert runs in one SQL
   transaction. A failed validation performs no write; a failed insert rolls
   back the counter reservation.
4. Parent changes and any submitted MeetingParticipant rows are committed
   together; at least one attendee is validated before the transaction.
5. Follow-up insertion is a distinct insert and does not update/delete prior
   follow-up rows.
6. Soft-delete and audit values are written together. No physical delete is
   exposed in the functional repositories.
7. Minuta file staging, current-reference update, and previous-file cleanup
   follow the process described in `plan.md`; operational failures are logged
   and never represented as a successful replacement if the current
   reference was not saved.

## Schema-review gate

The model is approved by the user as the consolidated initial MVP schema for
features 001–003. Its global Decision suffix is confirmed to continue across
all Trámites and Subetapas. Informe Semanal and Excel are excluded from this
initial schema and will require later migrations. Implementation still
requires approved plan and tasks and must create versioned SQL scripts in
accordance with the Constitution.
