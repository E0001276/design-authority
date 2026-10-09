SET XACT_ABORT ON;
BEGIN TRANSACTION;

CREATE TABLE dbo.ConfiguredUser
(
    ConfiguredUserId int NOT NULL CONSTRAINT PK_ConfiguredUser PRIMARY KEY,
    DisplayName nvarchar(200) NOT NULL
);

INSERT dbo.ConfiguredUser (ConfiguredUserId, DisplayName)
VALUES (1, N'Usuario configurado');

CREATE TABLE dbo.Tramite
(
    TramiteId int IDENTITY(1,1) NOT NULL CONSTRAINT PK_Tramite PRIMARY KEY,
    Name nvarchar(200) NOT NULL,
    Description nvarchar(1000) NULL,
    IsDefault bit NOT NULL CONSTRAINT DF_Tramite_IsDefault DEFAULT (0),
    IsActive bit NOT NULL CONSTRAINT DF_Tramite_IsActive DEFAULT (1),
    CreatedAtUtc datetime2(0) NOT NULL CONSTRAINT DF_Tramite_CreatedAtUtc DEFAULT SYSUTCDATETIME(),
    CreatedByUserId int NOT NULL,
    UpdatedAtUtc datetime2(0) NULL,
    UpdatedByUserId int NULL,
    CONSTRAINT FK_Tramite_CreatedBy FOREIGN KEY (CreatedByUserId) REFERENCES dbo.ConfiguredUser (ConfiguredUserId),
    CONSTRAINT FK_Tramite_UpdatedBy FOREIGN KEY (UpdatedByUserId) REFERENCES dbo.ConfiguredUser (ConfiguredUserId)
);
CREATE UNIQUE INDEX UX_Tramite_ActiveDefault
    ON dbo.Tramite (IsDefault) WHERE IsDefault = 1 AND IsActive = 1;

CREATE TABLE dbo.Substage
(
    SubstageId int IDENTITY(1,1) NOT NULL CONSTRAINT PK_Substage PRIMARY KEY,
    TramiteId int NOT NULL,
    Name nvarchar(200) NOT NULL,
    Description nvarchar(1000) NULL,
    IsActive bit NOT NULL CONSTRAINT DF_Substage_IsActive DEFAULT (1),
    CreatedAtUtc datetime2(0) NOT NULL CONSTRAINT DF_Substage_CreatedAtUtc DEFAULT SYSUTCDATETIME(),
    CreatedByUserId int NOT NULL,
    UpdatedAtUtc datetime2(0) NULL,
    UpdatedByUserId int NULL,
    CONSTRAINT UQ_Substage_Tramite_Substage UNIQUE (TramiteId, SubstageId),
    CONSTRAINT FK_Substage_Tramite FOREIGN KEY (TramiteId) REFERENCES dbo.Tramite (TramiteId),
    CONSTRAINT FK_Substage_CreatedBy FOREIGN KEY (CreatedByUserId) REFERENCES dbo.ConfiguredUser (ConfiguredUserId),
    CONSTRAINT FK_Substage_UpdatedBy FOREIGN KEY (UpdatedByUserId) REFERENCES dbo.ConfiguredUser (ConfiguredUserId)
);

CREATE TABLE dbo.TeamTechnology
(
    TeamTechnologyId int IDENTITY(1,1) NOT NULL CONSTRAINT PK_TeamTechnology PRIMARY KEY,
    Code varchar(40) NOT NULL CONSTRAINT UQ_TeamTechnology_Code UNIQUE,
    Name nvarchar(100) NOT NULL CONSTRAINT UQ_TeamTechnology_Name UNIQUE,
    Kind varchar(10) NOT NULL,
    CONSTRAINT UQ_TeamTechnology_Id_Kind UNIQUE (TeamTechnologyId, Kind),
    CONSTRAINT CK_TeamTechnology_Kind CHECK (Kind IN ('Technology', 'Team'))
);

SET IDENTITY_INSERT dbo.TeamTechnology ON;
INSERT dbo.TeamTechnology (TeamTechnologyId, Code, Name, Kind) VALUES
    (1, 'APPIAN', N'Appian', 'Technology'),
    (2, 'DATABRICKS', N'Databricks', 'Technology'),
    (3, 'MICROSERVICIOS', N'Microservicios', 'Technology'),
    (4, 'POWER_BI', N'Power BI', 'Technology'),
    (5, 'ARQUITECTURA', N'Arquitectura', 'Team'),
    (6, 'PROYECTO', N'Proyecto', 'Team'),
    (7, 'CELULA', N'Célula', 'Team'),
    (8, 'QA', N'QA', 'Team');
SET IDENTITY_INSERT dbo.TeamTechnology OFF;

CREATE TABLE dbo.[Role]
(
    RoleId int IDENTITY(1,1) NOT NULL CONSTRAINT PK_Role PRIMARY KEY,
    TeamTechnologyId int NOT NULL,
    Code varchar(60) NOT NULL,
    Name nvarchar(100) NOT NULL,
    IsLeadership bit NOT NULL,
    CONSTRAINT UQ_Role_TeamTechnology_Code UNIQUE (TeamTechnologyId, Code),
    CONSTRAINT UQ_Role_TeamTechnology_Name UNIQUE (TeamTechnologyId, Name),
    CONSTRAINT UQ_Role_Team_Role_Leadership UNIQUE (TeamTechnologyId, RoleId, IsLeadership),
    CONSTRAINT FK_Role_TeamTechnology FOREIGN KEY (TeamTechnologyId) REFERENCES dbo.TeamTechnology (TeamTechnologyId)
);

SET IDENTITY_INSERT dbo.Role ON;
INSERT dbo.[Role] (RoleId, TeamTechnologyId, Code, Name, IsLeadership) VALUES
    (1, 1, 'TECHNICAL_LEAD', N'Líder técnico', 1), (2, 1, 'DEVELOPER', N'Desarrollador', 0),
    (3, 2, 'TECHNICAL_LEAD', N'Líder técnico', 1), (4, 2, 'DEVELOPER', N'Desarrollador', 0),
    (5, 3, 'TECHNICAL_LEAD', N'Líder técnico', 1), (6, 3, 'DEVELOPER', N'Desarrollador', 0),
    (7, 4, 'TECHNICAL_LEAD', N'Líder técnico', 1), (8, 4, 'DEVELOPER', N'Desarrollador', 0),
    (9, 5, 'ARCHITECTURE_LEAD', N'Líder de arquitectura', 1), (10, 5, 'ARCHITECT', N'Arquitecto', 0),
    (11, 6, 'PROJECT_LEAD', N'Líder de proyecto', 1), (12, 6, 'RELEASE_MANAGER', N'Release manager', 0),
    (13, 7, 'CELL_LEAD', N'Líder de célula', 1),
    (14, 8, 'QA_LEAD', N'Líder de QA', 1), (15, 8, 'TESTER', N'Tester', 0);
SET IDENTITY_INSERT dbo.Role OFF;

CREATE TABLE dbo.Person
(
    PersonId int IDENTITY(1,1) NOT NULL CONSTRAINT PK_Person PRIMARY KEY,
    Name nvarchar(100) NOT NULL,
    PaternalSurname nvarchar(100) NOT NULL,
    MaternalSurname nvarchar(100) NULL,
    AbbreviatedName nvarchar(100) NOT NULL,
    TeamTechnologyId int NOT NULL,
    RoleId int NOT NULL,
    IsLeadership bit NOT NULL,
    CreatedAtUtc datetime2(0) NOT NULL CONSTRAINT DF_Person_CreatedAtUtc DEFAULT SYSUTCDATETIME(),
    CreatedByUserId int NOT NULL,
    UpdatedAtUtc datetime2(0) NULL,
    UpdatedByUserId int NULL,
    IsActive bit NOT NULL CONSTRAINT DF_Person_IsActive DEFAULT (1),
    DeletedAtUtc datetime2(0) NULL,
    DeletedByUserId int NULL,
    CONSTRAINT FK_Person_Role FOREIGN KEY (TeamTechnologyId, RoleId, IsLeadership)
        REFERENCES dbo.[Role] (TeamTechnologyId, RoleId, IsLeadership),
    CONSTRAINT FK_Person_CreatedBy FOREIGN KEY (CreatedByUserId) REFERENCES dbo.ConfiguredUser (ConfiguredUserId),
    CONSTRAINT FK_Person_UpdatedBy FOREIGN KEY (UpdatedByUserId) REFERENCES dbo.ConfiguredUser (ConfiguredUserId),
    CONSTRAINT FK_Person_DeletedBy FOREIGN KEY (DeletedByUserId) REFERENCES dbo.ConfiguredUser (ConfiguredUserId),
    CONSTRAINT CK_Person_DeleteAudit CHECK
        ((IsActive = 1 AND DeletedAtUtc IS NULL AND DeletedByUserId IS NULL) OR
         (IsActive = 0 AND DeletedAtUtc IS NOT NULL AND DeletedByUserId IS NOT NULL))
);
CREATE UNIQUE INDEX UX_Person_ActiveLeadership
    ON dbo.Person (TeamTechnologyId, RoleId) WHERE IsLeadership = 1 AND IsActive = 1;

CREATE TABLE dbo.ConsecutiveIdentifierCounter
(
    CounterKey varchar(40) NOT NULL CONSTRAINT PK_ConsecutiveIdentifierCounter PRIMARY KEY,
    LastValue int NOT NULL,
    CONSTRAINT CK_ConsecutiveIdentifierCounter_LastValue CHECK (LastValue >= 0)
);
INSERT dbo.ConsecutiveIdentifierCounter (CounterKey, LastValue) VALUES
    ('Finding', 0), ('Agreement', 0), ('ArchitectureDecisionSuffix', 0), ('Meeting', 0);

CREATE TABLE dbo.FindingCategory
(
    CategoryId int IDENTITY(1,1) NOT NULL CONSTRAINT PK_FindingCategory PRIMARY KEY,
    Code varchar(60) NOT NULL CONSTRAINT UQ_FindingCategory_Code UNIQUE,
    Name nvarchar(100) NOT NULL CONSTRAINT UQ_FindingCategory_Name UNIQUE
);
INSERT dbo.FindingCategory (Code, Name) VALUES
    ('DOCUMENTAL', N'Documental'), ('CONVENCIONES', N'Convenciones'),
    ('LINEAMIENTOS_ESTANDARES', N'Lineamientos y estándares'),
    ('REUTILIZACION', N'Reutilización'), ('DISENOS', N'Diseños'),
    ('USO_PATRONES', N'Uso de Patrones'), ('BUENAS_PRACTICAS', N'Buenas prácticas');

CREATE TABLE dbo.FindingStatus
(
    StatusId int IDENTITY(1,1) NOT NULL CONSTRAINT PK_FindingStatus PRIMARY KEY,
    Code varchar(40) NOT NULL CONSTRAINT UQ_FindingStatus_Code UNIQUE,
    Name nvarchar(100) NOT NULL CONSTRAINT UQ_FindingStatus_Name UNIQUE
);
INSERT dbo.FindingStatus (Code, Name) VALUES
    ('OPEN', N'Abierto'), ('IN_PROGRESS', N'En progreso'), ('COMPLETED', N'Completado'),
    ('CANCELLED', N'Cancelado'), ('BLOCKED', N'Bloqueado'), ('NOT_DELIVERED', N'No entregó');

CREATE TABLE dbo.AgreementStatus
(
    StatusId int IDENTITY(1,1) NOT NULL CONSTRAINT PK_AgreementStatus PRIMARY KEY,
    Code varchar(40) NOT NULL CONSTRAINT UQ_AgreementStatus_Code UNIQUE,
    Name nvarchar(100) NOT NULL CONSTRAINT UQ_AgreementStatus_Name UNIQUE
);
INSERT dbo.AgreementStatus (Code, Name) VALUES
    ('OPEN', N'Abierto'), ('IN_PROGRESS', N'En progreso'), ('COMPLETED', N'Completado'),
    ('CANCELLED', N'Cancelado'), ('BLOCKED', N'Bloqueado'), ('NOT_DELIVERED', N'No entregó');

CREATE TABLE dbo.DecisionStatus
(
    StatusId int IDENTITY(1,1) NOT NULL CONSTRAINT PK_DecisionStatus PRIMARY KEY,
    Code varchar(40) NOT NULL CONSTRAINT UQ_DecisionStatus_Code UNIQUE,
    Name nvarchar(100) NOT NULL CONSTRAINT UQ_DecisionStatus_Name UNIQUE
);
INSERT dbo.DecisionStatus (Code, Name) VALUES
    ('OPEN', N'Abierto'), ('IN_PROGRESS', N'En progreso'), ('CANCELLED', N'Cancelado'), ('CLOSED', N'Cerrado');

CREATE TABLE dbo.MeetingStatus
(
    StatusId int IDENTITY(1,1) NOT NULL CONSTRAINT PK_MeetingStatus PRIMARY KEY,
    Code varchar(40) NOT NULL CONSTRAINT UQ_MeetingStatus_Code UNIQUE,
    Name nvarchar(100) NOT NULL CONSTRAINT UQ_MeetingStatus_Name UNIQUE
);
INSERT dbo.MeetingStatus (Code, Name) VALUES
    ('HELD', N'Realizada'), ('TEAM_ABSENT', N'Equipo no asistió a la reunión'),
    ('RESCHEDULED', N'Reprogramada'), ('CANCELLED', N'Cancelada');

CREATE TABLE dbo.Criticality
(
    CriticalityId int IDENTITY(1,1) NOT NULL CONSTRAINT PK_Criticality PRIMARY KEY,
    Code varchar(40) NOT NULL CONSTRAINT UQ_Criticality_Code UNIQUE,
    Name nvarchar(100) NOT NULL CONSTRAINT UQ_Criticality_Name UNIQUE
);
INSERT dbo.Criticality (Code, Name) VALUES
    ('NOT_APPLICABLE', N'No aplica'), ('CRITICAL', N'Crítico'), ('MAJOR', N'Mayor'),
    ('MINOR', N'Menor'), ('SUGGESTION', N'Sugerencia');

CREATE TABLE dbo.Priority
(
    PriorityId int IDENTITY(1,1) NOT NULL CONSTRAINT PK_Priority PRIMARY KEY,
    Code varchar(40) NOT NULL CONSTRAINT UQ_Priority_Code UNIQUE,
    Name nvarchar(100) NOT NULL CONSTRAINT UQ_Priority_Name UNIQUE
);
INSERT dbo.Priority (Code, Name) VALUES
    ('HIGH', N'Alta'), ('MEDIUM', N'Media'), ('LOW', N'Baja');

CREATE TABLE dbo.RegistrationPhase
(
    RegistrationPhaseId int IDENTITY(1,1) NOT NULL CONSTRAINT PK_RegistrationPhase PRIMARY KEY,
    Code varchar(40) NOT NULL CONSTRAINT UQ_RegistrationPhase_Code UNIQUE,
    Name nvarchar(100) NOT NULL CONSTRAINT UQ_RegistrationPhase_Name UNIQUE
);
INSERT dbo.RegistrationPhase (Code, Name) VALUES
    ('NOT_APPLICABLE', N'No aplica'), ('BEFORE_DEVELOPMENT', N'Antes del desarrollo'),
    ('DURING_DEVELOPMENT', N'Durante el desarrollo'), ('AFTER_DEVELOPMENT', N'Después del desarrollo'),
    ('NOT_DELIVERED', N'No entregó');

CREATE TABLE dbo.AttentionPhase
(
    AttentionPhaseId int IDENTITY(1,1) NOT NULL CONSTRAINT PK_AttentionPhase PRIMARY KEY,
    Code varchar(40) NOT NULL CONSTRAINT UQ_AttentionPhase_Code UNIQUE,
    Name nvarchar(100) NOT NULL CONSTRAINT UQ_AttentionPhase_Name UNIQUE
);
INSERT dbo.AttentionPhase (Code, Name) VALUES
    ('DESIGN', N'Diseño'), ('CONSTRUCTION', N'Construcción'), ('TESTING', N'Pruebas'),
    ('CERTIFICATION', N'Certificación'), ('SUPPORT', N'Soporte'), ('MAINTENANCE', N'Mantenimiento'),
    ('IN_VALIDATION', N'En validación'), ('NOT_APPLICABLE', N'No aplica');

CREATE TABLE dbo.Finding
(
    FindingId int NOT NULL CONSTRAINT PK_Finding PRIMARY KEY,
    RegistrationDate date NOT NULL,
    TramiteId int NOT NULL,
    SubstageId int NOT NULL,
    TeamTechnologyId int NOT NULL,
    TeamTechnologyKind varchar(10) NOT NULL CONSTRAINT DF_Finding_TechnologyKind DEFAULT ('Technology'),
    CategoryId int NOT NULL,
    Description nvarchar(max) NOT NULL,
    ResponsiblePersonId int NOT NULL,
    StatusId int NOT NULL,
    CriticalityId int NOT NULL,
    PriorityId int NOT NULL,
    RegistrationPhaseId int NOT NULL,
    AttentionPhaseId int NOT NULL,
    CommitmentDate date NOT NULL,
    SolutionDate date NULL,
    CreatedAtUtc datetime2(0) NOT NULL CONSTRAINT DF_Finding_CreatedAtUtc DEFAULT SYSUTCDATETIME(),
    CreatedByUserId int NOT NULL,
    UpdatedAtUtc datetime2(0) NULL,
    UpdatedByUserId int NULL,
    IsActive bit NOT NULL CONSTRAINT DF_Finding_IsActive DEFAULT (1),
    DeletedAtUtc datetime2(0) NULL,
    DeletedByUserId int NULL,
    CONSTRAINT FK_Finding_Substage FOREIGN KEY (TramiteId, SubstageId) REFERENCES dbo.Substage (TramiteId, SubstageId),
    CONSTRAINT FK_Finding_Technology FOREIGN KEY (TeamTechnologyId, TeamTechnologyKind) REFERENCES dbo.TeamTechnology (TeamTechnologyId, Kind),
    CONSTRAINT CK_Finding_TechnologyKind CHECK (TeamTechnologyKind = 'Technology'),
    CONSTRAINT FK_Finding_Category FOREIGN KEY (CategoryId) REFERENCES dbo.FindingCategory (CategoryId),
    CONSTRAINT FK_Finding_Status FOREIGN KEY (StatusId) REFERENCES dbo.FindingStatus (StatusId),
    CONSTRAINT FK_Finding_Criticality FOREIGN KEY (CriticalityId) REFERENCES dbo.Criticality (CriticalityId),
    CONSTRAINT FK_Finding_Priority FOREIGN KEY (PriorityId) REFERENCES dbo.Priority (PriorityId),
    CONSTRAINT FK_Finding_RegistrationPhase FOREIGN KEY (RegistrationPhaseId) REFERENCES dbo.RegistrationPhase (RegistrationPhaseId),
    CONSTRAINT FK_Finding_AttentionPhase FOREIGN KEY (AttentionPhaseId) REFERENCES dbo.AttentionPhase (AttentionPhaseId),
    CONSTRAINT FK_Finding_Person FOREIGN KEY (ResponsiblePersonId) REFERENCES dbo.Person (PersonId),
    CONSTRAINT FK_Finding_CreatedBy FOREIGN KEY (CreatedByUserId) REFERENCES dbo.ConfiguredUser (ConfiguredUserId),
    CONSTRAINT FK_Finding_UpdatedBy FOREIGN KEY (UpdatedByUserId) REFERENCES dbo.ConfiguredUser (ConfiguredUserId),
    CONSTRAINT FK_Finding_DeletedBy FOREIGN KEY (DeletedByUserId) REFERENCES dbo.ConfiguredUser (ConfiguredUserId),
    CONSTRAINT CK_Finding_DeleteAudit CHECK
        ((IsActive = 1 AND DeletedAtUtc IS NULL AND DeletedByUserId IS NULL) OR
         (IsActive = 0 AND DeletedAtUtc IS NOT NULL AND DeletedByUserId IS NOT NULL))
);

CREATE TABLE dbo.FindingFollowUp
(
    FindingFollowUpId int IDENTITY(1,1) NOT NULL CONSTRAINT PK_FindingFollowUp PRIMARY KEY,
    FindingId int NOT NULL,
    FollowUpDate date NOT NULL,
    UserId int NOT NULL,
    Observation nvarchar(max) NOT NULL,
    OldStatusId int NULL,
    NewStatusId int NULL,
    OldCommitmentDate date NULL,
    NewCommitmentDate date NULL,
    CreatedAtUtc datetime2(0) NOT NULL CONSTRAINT DF_FindingFollowUp_CreatedAtUtc DEFAULT SYSUTCDATETIME(),
    CreatedByUserId int NOT NULL,
    CONSTRAINT FK_FindingFollowUp_Finding FOREIGN KEY (FindingId) REFERENCES dbo.Finding (FindingId),
    CONSTRAINT FK_FindingFollowUp_User FOREIGN KEY (UserId) REFERENCES dbo.ConfiguredUser (ConfiguredUserId),
    CONSTRAINT FK_FindingFollowUp_OldStatus FOREIGN KEY (OldStatusId) REFERENCES dbo.FindingStatus (StatusId),
    CONSTRAINT FK_FindingFollowUp_NewStatus FOREIGN KEY (NewStatusId) REFERENCES dbo.FindingStatus (StatusId),
    CONSTRAINT FK_FindingFollowUp_CreatedBy FOREIGN KEY (CreatedByUserId) REFERENCES dbo.ConfiguredUser (ConfiguredUserId),
    CONSTRAINT CK_FindingFollowUp_StatusPair CHECK
        ((OldStatusId IS NULL AND NewStatusId IS NULL) OR (OldStatusId IS NOT NULL AND NewStatusId IS NOT NULL)),
    CONSTRAINT CK_FindingFollowUp_CommitmentPair CHECK
        ((OldCommitmentDate IS NULL AND NewCommitmentDate IS NULL) OR (OldCommitmentDate IS NOT NULL AND NewCommitmentDate IS NOT NULL))
);

CREATE TABLE dbo.Agreement
(
    AgreementId int NOT NULL CONSTRAINT PK_Agreement PRIMARY KEY,
    RegistrationDate date NOT NULL,
    TramiteId int NOT NULL,
    TeamTechnologyId int NOT NULL,
    TeamTechnologyKind varchar(10) NOT NULL CONSTRAINT DF_Agreement_TechnologyKind DEFAULT ('Technology'),
    Description nvarchar(max) NOT NULL,
    ResponsiblePersonId int NOT NULL,
    StatusId int NOT NULL,
    CommitmentDate date NOT NULL,
    SolutionDate date NULL,
    CreatedAtUtc datetime2(0) NOT NULL CONSTRAINT DF_Agreement_CreatedAtUtc DEFAULT SYSUTCDATETIME(),
    CreatedByUserId int NOT NULL,
    UpdatedAtUtc datetime2(0) NULL,
    UpdatedByUserId int NULL,
    IsActive bit NOT NULL CONSTRAINT DF_Agreement_IsActive DEFAULT (1),
    DeletedAtUtc datetime2(0) NULL,
    DeletedByUserId int NULL,
    CONSTRAINT FK_Agreement_Tramite FOREIGN KEY (TramiteId) REFERENCES dbo.Tramite (TramiteId),
    CONSTRAINT FK_Agreement_Technology FOREIGN KEY (TeamTechnologyId, TeamTechnologyKind) REFERENCES dbo.TeamTechnology (TeamTechnologyId, Kind),
    CONSTRAINT CK_Agreement_TechnologyKind CHECK (TeamTechnologyKind = 'Technology'),
    CONSTRAINT FK_Agreement_Person FOREIGN KEY (ResponsiblePersonId) REFERENCES dbo.Person (PersonId),
    CONSTRAINT FK_Agreement_Status FOREIGN KEY (StatusId) REFERENCES dbo.AgreementStatus (StatusId),
    CONSTRAINT FK_Agreement_CreatedBy FOREIGN KEY (CreatedByUserId) REFERENCES dbo.ConfiguredUser (ConfiguredUserId),
    CONSTRAINT FK_Agreement_UpdatedBy FOREIGN KEY (UpdatedByUserId) REFERENCES dbo.ConfiguredUser (ConfiguredUserId),
    CONSTRAINT FK_Agreement_DeletedBy FOREIGN KEY (DeletedByUserId) REFERENCES dbo.ConfiguredUser (ConfiguredUserId),
    CONSTRAINT CK_Agreement_DeleteAudit CHECK
        ((IsActive = 1 AND DeletedAtUtc IS NULL AND DeletedByUserId IS NULL) OR
         (IsActive = 0 AND DeletedAtUtc IS NOT NULL AND DeletedByUserId IS NOT NULL))
);

CREATE TABLE dbo.AgreementFollowUp
(
    AgreementFollowUpId int IDENTITY(1,1) NOT NULL CONSTRAINT PK_AgreementFollowUp PRIMARY KEY,
    AgreementId int NOT NULL,
    FollowUpDate date NOT NULL,
    UserId int NOT NULL,
    Observation nvarchar(max) NOT NULL,
    OldStatusId int NULL,
    NewStatusId int NULL,
    OldCommitmentDate date NULL,
    NewCommitmentDate date NULL,
    CreatedAtUtc datetime2(0) NOT NULL CONSTRAINT DF_AgreementFollowUp_CreatedAtUtc DEFAULT SYSUTCDATETIME(),
    CreatedByUserId int NOT NULL,
    CONSTRAINT FK_AgreementFollowUp_Agreement FOREIGN KEY (AgreementId) REFERENCES dbo.Agreement (AgreementId),
    CONSTRAINT FK_AgreementFollowUp_User FOREIGN KEY (UserId) REFERENCES dbo.ConfiguredUser (ConfiguredUserId),
    CONSTRAINT FK_AgreementFollowUp_OldStatus FOREIGN KEY (OldStatusId) REFERENCES dbo.AgreementStatus (StatusId),
    CONSTRAINT FK_AgreementFollowUp_NewStatus FOREIGN KEY (NewStatusId) REFERENCES dbo.AgreementStatus (StatusId),
    CONSTRAINT FK_AgreementFollowUp_CreatedBy FOREIGN KEY (CreatedByUserId) REFERENCES dbo.ConfiguredUser (ConfiguredUserId),
    CONSTRAINT CK_AgreementFollowUp_StatusPair CHECK
        ((OldStatusId IS NULL AND NewStatusId IS NULL) OR (OldStatusId IS NOT NULL AND NewStatusId IS NOT NULL)),
    CONSTRAINT CK_AgreementFollowUp_CommitmentPair CHECK
        ((OldCommitmentDate IS NULL AND NewCommitmentDate IS NULL) OR (OldCommitmentDate IS NOT NULL AND NewCommitmentDate IS NOT NULL))
);

CREATE TABLE dbo.ArchitectureDecision
(
    DecisionId nvarchar(100) NOT NULL CONSTRAINT PK_ArchitectureDecision PRIMARY KEY,
    TramiteId int NOT NULL,
    SubstageId int NOT NULL,
    TeamTechnologyId int NOT NULL,
    TeamTechnologyKind varchar(10) NOT NULL CONSTRAINT DF_Decision_TechnologyKind DEFAULT ('Technology'),
    ResponsibleTeamTechnologyId int NOT NULL,
    ResponsibleKind varchar(10) NOT NULL CONSTRAINT DF_Decision_ResponsibleKind DEFAULT ('Team'),
    ProblemDescription nvarchar(max) NOT NULL,
    SolutionApproach nvarchar(max) NOT NULL,
    DecisionText nvarchar(max) NOT NULL,
    RegistrationDate date NOT NULL,
    StatusId int NOT NULL,
    CreatedAtUtc datetime2(0) NOT NULL CONSTRAINT DF_Decision_CreatedAtUtc DEFAULT SYSUTCDATETIME(),
    CreatedByUserId int NOT NULL,
    UpdatedAtUtc datetime2(0) NULL,
    UpdatedByUserId int NULL,
    IsActive bit NOT NULL CONSTRAINT DF_Decision_IsActive DEFAULT (1),
    DeletedAtUtc datetime2(0) NULL,
    DeletedByUserId int NULL,
    CONSTRAINT FK_Decision_Substage FOREIGN KEY (TramiteId, SubstageId) REFERENCES dbo.Substage (TramiteId, SubstageId),
    CONSTRAINT FK_Decision_Technology FOREIGN KEY (TeamTechnologyId, TeamTechnologyKind) REFERENCES dbo.TeamTechnology (TeamTechnologyId, Kind),
    CONSTRAINT CK_Decision_TechnologyKind CHECK (TeamTechnologyKind = 'Technology'),
    CONSTRAINT FK_Decision_ResponsibleTeam FOREIGN KEY (ResponsibleTeamTechnologyId, ResponsibleKind) REFERENCES dbo.TeamTechnology (TeamTechnologyId, Kind),
    CONSTRAINT CK_Decision_ResponsibleKind CHECK (ResponsibleKind = 'Team'),
    CONSTRAINT FK_Decision_Status FOREIGN KEY (StatusId) REFERENCES dbo.DecisionStatus (StatusId),
    CONSTRAINT FK_Decision_CreatedBy FOREIGN KEY (CreatedByUserId) REFERENCES dbo.ConfiguredUser (ConfiguredUserId),
    CONSTRAINT FK_Decision_UpdatedBy FOREIGN KEY (UpdatedByUserId) REFERENCES dbo.ConfiguredUser (ConfiguredUserId),
    CONSTRAINT FK_Decision_DeletedBy FOREIGN KEY (DeletedByUserId) REFERENCES dbo.ConfiguredUser (ConfiguredUserId),
    CONSTRAINT CK_Decision_DeleteAudit CHECK
        ((IsActive = 1 AND DeletedAtUtc IS NULL AND DeletedByUserId IS NULL) OR
         (IsActive = 0 AND DeletedAtUtc IS NOT NULL AND DeletedByUserId IS NOT NULL))
);

CREATE TABLE dbo.Meeting
(
    MeetingId int NOT NULL CONSTRAINT PK_Meeting PRIMARY KEY,
    TramiteId int NOT NULL,
    TeamTechnologyId int NOT NULL,
    TeamTechnologyKind varchar(10) NOT NULL CONSTRAINT DF_Meeting_TechnologyKind DEFAULT ('Technology'),
    MeetingDate date NOT NULL,
    MeetingTime time(0) NOT NULL,
    StatusId int NOT NULL,
    AgreementsSummary nvarchar(max) NOT NULL,
    CommitmentsSummary nvarchar(max) NOT NULL,
    Subject nvarchar(500) NOT NULL,
    Comments nvarchar(max) NULL,
    CreatedAtUtc datetime2(0) NOT NULL CONSTRAINT DF_Meeting_CreatedAtUtc DEFAULT SYSUTCDATETIME(),
    CreatedByUserId int NOT NULL,
    UpdatedAtUtc datetime2(0) NULL,
    UpdatedByUserId int NULL,
    IsActive bit NOT NULL CONSTRAINT DF_Meeting_IsActive DEFAULT (1),
    DeletedAtUtc datetime2(0) NULL,
    DeletedByUserId int NULL,
    CONSTRAINT FK_Meeting_Tramite FOREIGN KEY (TramiteId) REFERENCES dbo.Tramite (TramiteId),
    CONSTRAINT FK_Meeting_Technology FOREIGN KEY (TeamTechnologyId, TeamTechnologyKind) REFERENCES dbo.TeamTechnology (TeamTechnologyId, Kind),
    CONSTRAINT CK_Meeting_TechnologyKind CHECK (TeamTechnologyKind = 'Technology'),
    CONSTRAINT FK_Meeting_Status FOREIGN KEY (StatusId) REFERENCES dbo.MeetingStatus (StatusId),
    CONSTRAINT FK_Meeting_CreatedBy FOREIGN KEY (CreatedByUserId) REFERENCES dbo.ConfiguredUser (ConfiguredUserId),
    CONSTRAINT FK_Meeting_UpdatedBy FOREIGN KEY (UpdatedByUserId) REFERENCES dbo.ConfiguredUser (ConfiguredUserId),
    CONSTRAINT FK_Meeting_DeletedBy FOREIGN KEY (DeletedByUserId) REFERENCES dbo.ConfiguredUser (ConfiguredUserId),
    CONSTRAINT CK_Meeting_DeleteAudit CHECK
        ((IsActive = 1 AND DeletedAtUtc IS NULL AND DeletedByUserId IS NULL) OR
         (IsActive = 0 AND DeletedAtUtc IS NOT NULL AND DeletedByUserId IS NOT NULL))
);

CREATE TABLE dbo.MeetingParticipant
(
    MeetingId int NOT NULL,
    PersonId int NOT NULL,
    Attendance varchar(10) NOT NULL,
    CreatedAtUtc datetime2(0) NOT NULL CONSTRAINT DF_MeetingParticipant_CreatedAtUtc DEFAULT SYSUTCDATETIME(),
    CreatedByUserId int NOT NULL,
    CONSTRAINT PK_MeetingParticipant PRIMARY KEY (MeetingId, PersonId),
    CONSTRAINT FK_MeetingParticipant_Meeting FOREIGN KEY (MeetingId) REFERENCES dbo.Meeting (MeetingId),
    CONSTRAINT FK_MeetingParticipant_Person FOREIGN KEY (PersonId) REFERENCES dbo.Person (PersonId),
    CONSTRAINT FK_MeetingParticipant_CreatedBy FOREIGN KEY (CreatedByUserId) REFERENCES dbo.ConfiguredUser (ConfiguredUserId),
    CONSTRAINT CK_MeetingParticipant_Attendance CHECK (Attendance IN ('Attended', 'Absent'))
);

CREATE TABLE dbo.MeetingMinute
(
    MeetingId int NOT NULL CONSTRAINT PK_MeetingMinute PRIMARY KEY,
    OriginalFileName nvarchar(255) NOT NULL,
    StorageFileName nvarchar(255) NOT NULL,
    RelativePath nvarchar(1000) NOT NULL,
    CreatedAtUtc datetime2(0) NOT NULL CONSTRAINT DF_MeetingMinute_CreatedAtUtc DEFAULT SYSUTCDATETIME(),
    CreatedByUserId int NOT NULL,
    UpdatedAtUtc datetime2(0) NULL,
    UpdatedByUserId int NULL,
    CONSTRAINT FK_MeetingMinute_Meeting FOREIGN KEY (MeetingId) REFERENCES dbo.Meeting (MeetingId),
    CONSTRAINT FK_MeetingMinute_CreatedBy FOREIGN KEY (CreatedByUserId) REFERENCES dbo.ConfiguredUser (ConfiguredUserId),
    CONSTRAINT FK_MeetingMinute_UpdatedBy FOREIGN KEY (UpdatedByUserId) REFERENCES dbo.ConfiguredUser (ConfiguredUserId),
    CONSTRAINT CK_MeetingMinute_RelativePath CHECK
        (LEFT(RelativePath, 1) NOT IN (N'\', N'/') AND RelativePath NOT LIKE '[A-Za-z]:%')
);

CREATE TABLE dbo.FileCleanupQueue
(
    FileCleanupId bigint IDENTITY(1,1) NOT NULL CONSTRAINT PK_FileCleanupQueue PRIMARY KEY,
    RelativePath nvarchar(1000) NOT NULL,
    QueuedAtUtc datetime2(0) NOT NULL CONSTRAINT DF_FileCleanupQueue_QueuedAtUtc DEFAULT SYSUTCDATETIME(),
    AttemptCount int NOT NULL CONSTRAINT DF_FileCleanupQueue_AttemptCount DEFAULT (0),
    LastAttemptAtUtc datetime2(0) NULL,
    LastError nvarchar(1000) NULL
);

COMMIT TRANSACTION;
GO
