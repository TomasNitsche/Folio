/*
  Folio — database schema
  Run after creating the database (see database/README.md).
*/
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

IF OBJECT_ID(N'dbo.Users', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Users
    (
        Id           UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_Users PRIMARY KEY,
        UserName     NVARCHAR(256)    NOT NULL,
        DisplayName  NVARCHAR(256)    NOT NULL,
        Email        NVARCHAR(256)    NOT NULL,
        Role         TINYINT          NOT NULL, -- 1 Admin, 2 Contributor, 3 Confirmer, 4 PackFiller, 5 ReportingLead
        IsActive     BIT              NOT NULL CONSTRAINT DF_Users_IsActive DEFAULT (1),
        CreatedAt    DATETIME2(3)     NOT NULL CONSTRAINT DF_Users_CreatedAt DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT UQ_Users_UserName UNIQUE (UserName),
        CONSTRAINT UQ_Users_Email UNIQUE (Email)
    );
END
GO

IF OBJECT_ID(N'dbo.DevCompanies', N'U') IS NULL
BEGIN
    /* Development-only catalog simulating Registry entity master data */
    CREATE TABLE dbo.DevCompanies
    (
        RegistryEntityId INT            NOT NULL CONSTRAINT PK_DevCompanies PRIMARY KEY,
        Name         NVARCHAR(500)  NOT NULL,
        Code         NVARCHAR(50)   NOT NULL,
        ValidFrom    DATE           NOT NULL,
        ValidTo      DATE           NULL,
        IsActive     BIT            NOT NULL CONSTRAINT DF_DevCompanies_IsActive DEFAULT (1),
        CONSTRAINT UQ_DevCompanies_Code UNIQUE (Code)
    );
END
GO

IF OBJECT_ID(N'dbo.ReportingPeriods', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.ReportingPeriods
    (
        Id               UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_ReportingPeriods PRIMARY KEY,
        ConfirmationType TINYINT          NOT NULL, -- 1 Transfers, 2 Budget, 3 Annual
        Code             NVARCHAR(20)     NOT NULL,
        StartDate        DATE             NOT NULL,
        EndDate          DATE             NOT NULL,
        Status           TINYINT          NOT NULL, -- 1 Draft, 2 Active, 3 Closed
        SnapshotDate     DATE             NOT NULL,
        OpenedBy         NVARCHAR(256)    NOT NULL,
        CreatedAt        DATETIME2(3)     NOT NULL,
        ClosedAt         DATETIME2(3)     NULL,
        CONSTRAINT UQ_ReportingPeriods_Code UNIQUE (Code)
    );
    CREATE INDEX IX_ReportingPeriods_ConfirmationType ON dbo.ReportingPeriods (ConfirmationType);
END
GO

IF COL_LENGTH(N'dbo.ReportingPeriods', N'IsDefault') IS NULL
BEGIN
    /* The period the reporting team currently collects — at most one per confirmation type */
    ALTER TABLE dbo.ReportingPeriods
        ADD IsDefault BIT NOT NULL CONSTRAINT DF_ReportingPeriods_IsDefault DEFAULT (0);
END
GO

IF OBJECT_ID(N'dbo.PeriodEntities', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.PeriodEntities
    (
        Id            UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_PeriodEntities PRIMARY KEY,
        PeriodId      UNIQUEIDENTIFIER NOT NULL,
        RegistryEntityId  INT              NOT NULL,
        Name          NVARCHAR(500)    NOT NULL,
        Code          NVARCHAR(50)     NOT NULL,
        SnapshotDate  DATE             NOT NULL,
        CONSTRAINT FK_PeriodEntities_Period FOREIGN KEY (PeriodId) REFERENCES dbo.ReportingPeriods (Id)
    );
    CREATE INDEX IX_PeriodEntities_PeriodId ON dbo.PeriodEntities (PeriodId);
    CREATE UNIQUE INDEX UQ_PeriodEntities_Period_Registry ON dbo.PeriodEntities (PeriodId, RegistryEntityId);
END
GO

IF OBJECT_ID(N'dbo.EntityPackages', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.EntityPackages
    (
        Id              UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_EntityPackages PRIMARY KEY,
        PeriodEntityId  UNIQUEIDENTIFIER NOT NULL,
        Version         INT              NOT NULL,
        IsFinal         BIT              NOT NULL CONSTRAINT DF_EntityPackages_IsFinal DEFAULT (0),
        Status          TINYINT          NOT NULL, -- 1 Draft, 2 Pending, 3 Confirmed, 4 Rejected
        CreatedAt       DATETIME2(3)     NOT NULL,
        UpdatedAt       DATETIME2(3)     NOT NULL,
        CreatedBy       NVARCHAR(256)    NULL,
        CONSTRAINT FK_EntityPackages_PeriodEntity FOREIGN KEY (PeriodEntityId) REFERENCES dbo.PeriodEntities (Id)
    );
    CREATE INDEX IX_EntityPackages_PeriodEntityId ON dbo.EntityPackages (PeriodEntityId);
    CREATE UNIQUE INDEX UQ_EntityPackages_Entity_Version ON dbo.EntityPackages (PeriodEntityId, Version);
END
GO

IF OBJECT_ID(N'dbo.StoredDocuments', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.StoredDocuments
    (
        Id              UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_StoredDocuments PRIMARY KEY,
        PackageId       UNIQUEIDENTIFIER NULL,
        PeriodEntityId  UNIQUEIDENTIFIER NULL,
        Kind            TINYINT          NOT NULL, -- 1 Static, 2 Signed, 3 ExcelPackage
        FileName        NVARCHAR(500)    NOT NULL,
        StoragePath     NVARCHAR(1000)   NOT NULL,
        PackageVersion  INT              NULL,
        UploadedBy      NVARCHAR(256)    NULL,
        UploadedAt      DATETIME2(3)     NOT NULL,
        CONSTRAINT FK_StoredDocuments_Package FOREIGN KEY (PackageId) REFERENCES dbo.EntityPackages (Id),
        CONSTRAINT FK_StoredDocuments_PeriodEntity FOREIGN KEY (PeriodEntityId) REFERENCES dbo.PeriodEntities (Id)
    );
    CREATE INDEX IX_StoredDocuments_PeriodEntityId ON dbo.StoredDocuments (PeriodEntityId);
END
GO

IF OBJECT_ID(N'dbo.EntityAssignments', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.EntityAssignments
    (
        Id              UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_EntityAssignments PRIMARY KEY,
        PeriodEntityId  UNIQUEIDENTIFIER NOT NULL,
        UserId          UNIQUEIDENTIFIER NOT NULL,
        CanConfirm      BIT              NOT NULL,
        CanFillPackage  BIT              NOT NULL,
        CONSTRAINT FK_EntityAssignments_PeriodEntity FOREIGN KEY (PeriodEntityId) REFERENCES dbo.PeriodEntities (Id),
        CONSTRAINT FK_EntityAssignments_User FOREIGN KEY (UserId) REFERENCES dbo.Users (Id)
    );
    CREATE INDEX IX_EntityAssignments_PeriodEntityId ON dbo.EntityAssignments (PeriodEntityId);
END
GO

IF OBJECT_ID(N'dbo.AuditEntries', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.AuditEntries
    (
        Id           UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_AuditEntries PRIMARY KEY,
        EntityType   NVARCHAR(100)    NOT NULL,
        EntityId     UNIQUEIDENTIFIER NOT NULL,
        Action       NVARCHAR(100)    NOT NULL,
        PerformedBy  NVARCHAR(256)    NOT NULL,
        PerformedAt  DATETIME2(3)     NOT NULL,
        Details      NVARCHAR(MAX)    NULL
    );
    CREATE INDEX IX_AuditEntries_Entity ON dbo.AuditEntries (EntityType, EntityId);
END
GO

PRINT 'Schema created successfully.';
GO
