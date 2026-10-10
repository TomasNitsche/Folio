/*
  Folio — stored procedures
  Naming: Folio_{Entity}_{Action}
*/
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

/* ── Users ─────────────────────────────────────────────────── */

CREATE OR ALTER PROCEDURE dbo.Folio_User_SELECT_All
AS
BEGIN
    SET NOCOUNT ON;
    SELECT Id, UserName, DisplayName, Email, Role, IsActive, CreatedAt
    FROM dbo.Users
    WHERE IsActive = 1
    ORDER BY DisplayName;
END
GO

CREATE OR ALTER PROCEDURE dbo.Folio_User_SELECT_ById
    @Id UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    SELECT Id, UserName, DisplayName, Email, Role, IsActive, CreatedAt
    FROM dbo.Users
    WHERE Id = @Id;
END
GO

CREATE OR ALTER PROCEDURE dbo.Folio_User_INSERT
    @Id          UNIQUEIDENTIFIER,
    @UserName    NVARCHAR(256),
    @DisplayName NVARCHAR(256),
    @Email       NVARCHAR(256),
    @Role        TINYINT
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS (SELECT 1 FROM dbo.Users WHERE Id = @Id)
        INSERT INTO dbo.Users (Id, UserName, DisplayName, Email, Role, IsActive, CreatedAt)
        VALUES (@Id, @UserName, @DisplayName, @Email, @Role, 1, SYSUTCDATETIME());
END
GO

CREATE OR ALTER PROCEDURE dbo.Folio_User_SELECT_AllAdmin
AS
BEGIN
    SET NOCOUNT ON;
    SELECT Id, UserName, DisplayName, Email, Role, IsActive, CreatedAt
    FROM dbo.Users
    ORDER BY IsActive DESC, DisplayName;
END
GO

CREATE OR ALTER PROCEDURE dbo.Folio_User_CREATE
    @Id          UNIQUEIDENTIFIER,
    @UserName    NVARCHAR(256),
    @DisplayName NVARCHAR(256),
    @Email       NVARCHAR(256),
    @Role        TINYINT
AS
BEGIN
    SET NOCOUNT ON;
    IF EXISTS (SELECT 1 FROM dbo.Users WHERE UserName = @UserName)
        THROW 50001, 'Username already exists.', 1;
    IF EXISTS (SELECT 1 FROM dbo.Users WHERE Email = @Email)
        THROW 50002, 'Email already exists.', 1;

    INSERT INTO dbo.Users (Id, UserName, DisplayName, Email, Role, IsActive, CreatedAt)
    VALUES (@Id, @UserName, @DisplayName, @Email, @Role, 1, SYSUTCDATETIME());
END
GO

CREATE OR ALTER PROCEDURE dbo.Folio_User_UPDATE
    @Id          UNIQUEIDENTIFIER,
    @DisplayName NVARCHAR(256),
    @Email       NVARCHAR(256),
    @Role        TINYINT
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS (SELECT 1 FROM dbo.Users WHERE Id = @Id)
        THROW 50003, 'User not found.', 1;
    IF EXISTS (SELECT 1 FROM dbo.Users WHERE Email = @Email AND Id <> @Id)
        THROW 50002, 'Email already exists.', 1;

    UPDATE dbo.Users
    SET DisplayName = @DisplayName,
        Email = @Email,
        Role = @Role
    WHERE Id = @Id;
END
GO

CREATE OR ALTER PROCEDURE dbo.Folio_User_DEACTIVATE
    @Id UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS (SELECT 1 FROM dbo.Users WHERE Id = @Id)
        THROW 50003, 'User not found.', 1;

    UPDATE dbo.Users SET IsActive = 0 WHERE Id = @Id;
END
GO

CREATE OR ALTER PROCEDURE dbo.Folio_User_REACTIVATE
    @Id UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS (SELECT 1 FROM dbo.Users WHERE Id = @Id)
        THROW 50003, 'User not found.', 1;

    UPDATE dbo.Users SET IsActive = 1 WHERE Id = @Id;
END
GO

/* ── Dev companies (Registry substitute in Development) ────────── */

CREATE OR ALTER PROCEDURE dbo.Folio_DevCompany_SELECT_ValidOn
    @ValidOn DATE
AS
BEGIN
    SET NOCOUNT ON;
    SELECT RegistryEntityId, Name, Code, @ValidOn AS ValidOn
    FROM dbo.DevCompanies
    WHERE IsActive = 1
      AND ValidFrom <= @ValidOn
      AND (ValidTo IS NULL OR ValidTo >= @ValidOn)
    ORDER BY Name;
END
GO

CREATE OR ALTER PROCEDURE dbo.Folio_DevCompany_MERGE
    @RegistryEntityId INT,
    @Name         NVARCHAR(500),
    @Code         NVARCHAR(50),
    @ValidFrom    DATE,
    @ValidTo      DATE = NULL
AS
BEGIN
    SET NOCOUNT ON;
    MERGE dbo.DevCompanies AS target
    USING (SELECT @RegistryEntityId AS RegistryEntityId) AS source
    ON target.RegistryEntityId = source.RegistryEntityId
    WHEN MATCHED THEN
        UPDATE SET Name = @Name, Code = @Code, ValidFrom = @ValidFrom, ValidTo = @ValidTo, IsActive = 1
    WHEN NOT MATCHED THEN
        INSERT (RegistryEntityId, Name, Code, ValidFrom, ValidTo, IsActive)
        VALUES (@RegistryEntityId, @Name, @Code, @ValidFrom, @ValidTo, 1);
END
GO

/* ── Reporting periods ─────────────────────────────────────── */

CREATE OR ALTER PROCEDURE dbo.Folio_Period_SELECT_ByType
    @ConfirmationType TINYINT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT p.Id, p.ConfirmationType, p.Code, p.StartDate, p.EndDate, p.Status, p.IsDefault,
           p.SnapshotDate, p.OpenedBy, p.CreatedAt, p.ClosedAt,
           EntityCount = (SELECT COUNT(*) FROM dbo.PeriodEntities pe WHERE pe.PeriodId = p.Id)
    FROM dbo.ReportingPeriods p
    WHERE p.ConfirmationType = @ConfirmationType
    ORDER BY p.Code DESC;
END
GO

CREATE OR ALTER PROCEDURE dbo.Folio_Period_SELECT_ById
    @Id UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    SELECT p.Id, p.ConfirmationType, p.Code, p.StartDate, p.EndDate, p.Status, p.IsDefault,
           p.SnapshotDate, p.OpenedBy, p.CreatedAt, p.ClosedAt,
           EntityCount = (SELECT COUNT(*) FROM dbo.PeriodEntities pe WHERE pe.PeriodId = p.Id)
    FROM dbo.ReportingPeriods p
    WHERE p.Id = @Id;
END
GO

CREATE OR ALTER PROCEDURE dbo.Folio_Period_SELECT_ByCode
    @Code NVARCHAR(20)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT p.Id, p.ConfirmationType, p.Code, p.StartDate, p.EndDate, p.Status, p.IsDefault,
           p.SnapshotDate, p.OpenedBy, p.CreatedAt, p.ClosedAt,
           EntityCount = (SELECT COUNT(*) FROM dbo.PeriodEntities pe WHERE pe.PeriodId = p.Id)
    FROM dbo.ReportingPeriods p
    WHERE p.Code = @Code;
END
GO

CREATE OR ALTER PROCEDURE dbo.Folio_Period_INSERT
    @Id               UNIQUEIDENTIFIER,
    @ConfirmationType TINYINT,
    @Code             NVARCHAR(20),
    @StartDate        DATE,
    @EndDate          DATE,
    @Status           TINYINT,
    @SnapshotDate     DATE,
    @OpenedBy         NVARCHAR(256),
    @CreatedAt        DATETIME2(3),
    @IsDefault        BIT = 0
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO dbo.ReportingPeriods
        (Id, ConfirmationType, Code, StartDate, EndDate, Status, SnapshotDate, OpenedBy, CreatedAt, IsDefault)
    VALUES
        (@Id, @ConfirmationType, @Code, @StartDate, @EndDate, @Status, @SnapshotDate, @OpenedBy, @CreatedAt, @IsDefault);
END
GO

CREATE OR ALTER PROCEDURE dbo.Folio_Period_UPDATE
    @Id       UNIQUEIDENTIFIER,
    @Status   TINYINT,
    @ClosedAt DATETIME2(3) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.ReportingPeriods
    SET Status = @Status, ClosedAt = @ClosedAt
    WHERE Id = @Id;
END
GO

CREATE OR ALTER PROCEDURE dbo.Folio_Period_SET_Default
    @Id               UNIQUEIDENTIFIER,
    @ConfirmationType TINYINT
AS
BEGIN
    SET NOCOUNT ON;
    /* At most one default period per confirmation type */
    UPDATE dbo.ReportingPeriods
    SET IsDefault = CASE WHEN Id = @Id THEN 1 ELSE 0 END
    WHERE ConfirmationType = @ConfirmationType
      AND (Id = @Id OR IsDefault = 1);
END
GO

/* ── Period entities ───────────────────────────────────────── */

CREATE OR ALTER PROCEDURE dbo.Folio_PeriodEntity_SELECT_ByPeriodId
    @PeriodId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    SELECT pe.Id, pe.PeriodId, pe.RegistryEntityId, pe.Name, pe.Code, pe.SnapshotDate
    FROM dbo.PeriodEntities pe
    WHERE pe.PeriodId = @PeriodId
    ORDER BY pe.Name;
END
GO

CREATE OR ALTER PROCEDURE dbo.Folio_PeriodEntity_SELECT_ById
    @Id UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    SELECT pe.Id, pe.PeriodId, pe.RegistryEntityId, pe.Name, pe.Code, pe.SnapshotDate,
           p.Id AS Period_Id, p.ConfirmationType AS Period_ConfirmationType, p.Code AS Period_Code,
           p.StartDate AS Period_StartDate, p.EndDate AS Period_EndDate, p.Status AS Period_Status,
           p.IsDefault AS Period_IsDefault,
           p.SnapshotDate AS Period_SnapshotDate, p.OpenedBy AS Period_OpenedBy,
           p.CreatedAt AS Period_CreatedAt, p.ClosedAt AS Period_ClosedAt
    FROM dbo.PeriodEntities pe
    INNER JOIN dbo.ReportingPeriods p ON p.Id = pe.PeriodId
    WHERE pe.Id = @Id;
END
GO

CREATE OR ALTER PROCEDURE dbo.Folio_PeriodEntity_INSERT
    @Id           UNIQUEIDENTIFIER,
    @PeriodId     UNIQUEIDENTIFIER,
    @RegistryEntityId INT,
    @Name         NVARCHAR(500),
    @Code         NVARCHAR(50),
    @SnapshotDate DATE
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO dbo.PeriodEntities (Id, PeriodId, RegistryEntityId, Name, Code, SnapshotDate)
    VALUES (@Id, @PeriodId, @RegistryEntityId, @Name, @Code, @SnapshotDate);
END
GO

/* ── Entity packages ───────────────────────────────────────── */

CREATE OR ALTER PROCEDURE dbo.Folio_Package_SELECT_ByPeriodEntityId
    @PeriodEntityId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    SELECT Id, PeriodEntityId, Version, IsFinal, Status, CreatedAt, UpdatedAt, CreatedBy
    FROM dbo.EntityPackages
    WHERE PeriodEntityId = @PeriodEntityId
    ORDER BY Version DESC;
END
GO

CREATE OR ALTER PROCEDURE dbo.Folio_Package_SELECT_Latest
    @PeriodEntityId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    SELECT TOP 1 Id, PeriodEntityId, Version, IsFinal, Status, CreatedAt, UpdatedAt, CreatedBy
    FROM dbo.EntityPackages
    WHERE PeriodEntityId = @PeriodEntityId
    ORDER BY Version DESC;
END
GO

CREATE OR ALTER PROCEDURE dbo.Folio_Package_INSERT
    @Id             UNIQUEIDENTIFIER,
    @PeriodEntityId UNIQUEIDENTIFIER,
    @Version        INT,
    @IsFinal        BIT,
    @Status         TINYINT,
    @CreatedAt      DATETIME2(3),
    @UpdatedAt      DATETIME2(3),
    @CreatedBy      NVARCHAR(256) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO dbo.EntityPackages
        (Id, PeriodEntityId, Version, IsFinal, Status, CreatedAt, UpdatedAt, CreatedBy)
    VALUES
        (@Id, @PeriodEntityId, @Version, @IsFinal, @Status, @CreatedAt, @UpdatedAt, @CreatedBy);
END
GO

CREATE OR ALTER PROCEDURE dbo.Folio_Package_UPDATE
    @Id        UNIQUEIDENTIFIER,
    @IsFinal   BIT,
    @Status    TINYINT,
    @UpdatedAt DATETIME2(3)
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.EntityPackages
    SET IsFinal = @IsFinal, Status = @Status, UpdatedAt = @UpdatedAt
    WHERE Id = @Id;
END
GO

/* ── Documents ─────────────────────────────────────────────── */

CREATE OR ALTER PROCEDURE dbo.Folio_Document_SELECT_ById
    @Id UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    SELECT Id, PackageId, PeriodEntityId, Kind, FileName, StoragePath, PackageVersion, UploadedBy, UploadedAt
    FROM dbo.StoredDocuments
    WHERE Id = @Id;
END
GO

CREATE OR ALTER PROCEDURE dbo.Folio_Document_SELECT_ByPeriodEntityId
    @PeriodEntityId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    SELECT Id, PackageId, PeriodEntityId, Kind, FileName, StoragePath, PackageVersion, UploadedBy, UploadedAt
    FROM dbo.StoredDocuments
    WHERE PeriodEntityId = @PeriodEntityId
    ORDER BY UploadedAt DESC;
END
GO

CREATE OR ALTER PROCEDURE dbo.Folio_Document_INSERT
    @Id             UNIQUEIDENTIFIER,
    @PackageId      UNIQUEIDENTIFIER = NULL,
    @PeriodEntityId UNIQUEIDENTIFIER = NULL,
    @Kind           TINYINT,
    @FileName       NVARCHAR(500),
    @StoragePath    NVARCHAR(1000),
    @PackageVersion INT = NULL,
    @UploadedBy     NVARCHAR(256) = NULL,
    @UploadedAt     DATETIME2(3)
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO dbo.StoredDocuments
        (Id, PackageId, PeriodEntityId, Kind, FileName, StoragePath, PackageVersion, UploadedBy, UploadedAt)
    VALUES
        (@Id, @PackageId, @PeriodEntityId, @Kind, @FileName, @StoragePath, @PackageVersion, @UploadedBy, @UploadedAt);
END
GO

CREATE OR ALTER PROCEDURE dbo.Folio_Document_UPDATE_File
    @Id          UNIQUEIDENTIFIER,
    @FileName    NVARCHAR(500),
    @StoragePath NVARCHAR(1000)
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.StoredDocuments
    SET FileName = @FileName,
        StoragePath = @StoragePath
    WHERE Id = @Id;
END
GO

/* ── Assignments ───────────────────────────────────────────── */

CREATE OR ALTER PROCEDURE dbo.Folio_Assignment_SELECT_ByPeriodEntityId
    @PeriodEntityId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    SELECT a.Id, a.PeriodEntityId, a.UserId, a.CanConfirm, a.CanFillPackage,
           u.UserName, u.DisplayName, u.Email
    FROM dbo.EntityAssignments a
    INNER JOIN dbo.Users u ON u.Id = a.UserId
    WHERE a.PeriodEntityId = @PeriodEntityId;
END
GO

CREATE OR ALTER PROCEDURE dbo.Folio_Assignment_INSERT
    @Id             UNIQUEIDENTIFIER,
    @PeriodEntityId UNIQUEIDENTIFIER,
    @UserId         UNIQUEIDENTIFIER,
    @CanConfirm     BIT,
    @CanFillPackage BIT
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO dbo.EntityAssignments (Id, PeriodEntityId, UserId, CanConfirm, CanFillPackage)
    VALUES (@Id, @PeriodEntityId, @UserId, @CanConfirm, @CanFillPackage);
END
GO

CREATE OR ALTER PROCEDURE dbo.Folio_Assignment_DELETE_ByPeriodEntityId
    @PeriodEntityId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    DELETE FROM dbo.EntityAssignments WHERE PeriodEntityId = @PeriodEntityId;
END
GO

/* ── Audit ─────────────────────────────────────────────────── */

CREATE OR ALTER PROCEDURE dbo.Folio_Audit_INSERT
    @Id          UNIQUEIDENTIFIER,
    @EntityType  NVARCHAR(100),
    @EntityId    UNIQUEIDENTIFIER,
    @Action      NVARCHAR(100),
    @PerformedBy NVARCHAR(256),
    @PerformedAt DATETIME2(3),
    @Details     NVARCHAR(MAX) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO dbo.AuditEntries (Id, EntityType, EntityId, Action, PerformedBy, PerformedAt, Details)
    VALUES (@Id, @EntityType, @EntityId, @Action, @PerformedBy, @PerformedAt, @Details);
END
GO

PRINT 'Stored procedures created successfully.';
GO
