/*
  Folio — development seed data
  Safe to re-run: MERGE upserts users; companies use MERGE SP.
  Unicode: all string literals use N'...' prefix for NVARCHAR.
*/
SET NOCOUNT ON;
GO

/* ── Users ─────────────────────────────────────────────────── */
/* Role: 1 Admin, 2 Contributor, 3 Confirmer, 4 PackFiller, 5 ReportingLead */

DECLARE @Users TABLE (
    Id UNIQUEIDENTIFIER,
    UserName NVARCHAR(256),
    DisplayName NVARCHAR(256),
    Email NVARCHAR(256),
    Role TINYINT
);

INSERT INTO @Users VALUES
('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa1', N'test.testovic',  N'Test Testovič',   N'test@folio.local',  1),
('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa2', N'folio.admin',   N'Folio Admin',    N'admin@folio.local',          1),
('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa3', N'folio.lead',    N'Reporting Lead',  N'lead@folio.local',           5),
('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa4', N'karel.gott',     N'Karel Gott',      N'karel.gott@example.com',      3),
('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa5', N'jan.novak',      N'Jan Novák',       N'jan.novak@example.com',       3),
('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa6', N'marie.vesela',   N'Marie Veselá',    N'marie.vesela@example.com',    3),
('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa7', N'petr.horak',     N'Petr Horák',      N'petr.horak@example.com',      4),
('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa8', N'eva.kralova',    N'Eva Králová',     N'eva.kralova@example.com',     4),
('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa9', N'folio.uploader',N'Folio Uploader', N'uploader@folio.local',       2);

MERGE dbo.Users AS target
USING @Users AS source ON target.Id = source.Id
WHEN NOT MATCHED THEN
    INSERT (Id, UserName, DisplayName, Email, Role, IsActive, CreatedAt)
    VALUES (source.Id, source.UserName, source.DisplayName, source.Email, source.Role, 1, SYSUTCDATETIME())
WHEN MATCHED THEN
    UPDATE SET
        UserName = source.UserName,
        DisplayName = source.DisplayName,
        Email = source.Email,
        Role = source.Role;
GO

/* ── Dev companies (Registry substitute) ───────────────────────── */

EXEC dbo.Folio_DevCompany_MERGE @RegistryEntityId=101, @Name=N'HC Sparta Praha', @Code=N'SPA', @ValidFrom='2020-01-01';
EXEC dbo.Folio_DevCompany_MERGE @RegistryEntityId=102, @Name=N'HC Slavia Praha', @Code=N'SLA', @ValidFrom='2020-01-01';
EXEC dbo.Folio_DevCompany_MERGE @RegistryEntityId=103, @Name=N'HC Oceláři Třinec', @Code=N'TRI', @ValidFrom='2020-01-01';
EXEC dbo.Folio_DevCompany_MERGE @RegistryEntityId=104, @Name=N'HC Kometa Brno', @Code=N'KOM', @ValidFrom='2020-01-01';
EXEC dbo.Folio_DevCompany_MERGE @RegistryEntityId=105, @Name=N'HC Energie Karlovy Vary', @Code=N'KVA', @ValidFrom='2020-01-01';
EXEC dbo.Folio_DevCompany_MERGE @RegistryEntityId=106, @Name=N'HC Dynamo Pardubice', @Code=N'PAR', @ValidFrom='2020-01-01';
EXEC dbo.Folio_DevCompany_MERGE @RegistryEntityId=107, @Name=N'Bílí Tygři Liberec', @Code=N'LIB', @ValidFrom='2020-01-01';
EXEC dbo.Folio_DevCompany_MERGE @RegistryEntityId=108, @Name=N'HC Olomouc', @Code=N'OLO', @ValidFrom='2020-01-01';
EXEC dbo.Folio_DevCompany_MERGE @RegistryEntityId=109, @Name=N'HC Verva Litvínov', @Code=N'LIT', @ValidFrom='2020-01-01';
EXEC dbo.Folio_DevCompany_MERGE @RegistryEntityId=110, @Name=N'HC Vítkovice Ridera', @Code=N'VIT', @ValidFrom='2020-01-01';
EXEC dbo.Folio_DevCompany_MERGE @RegistryEntityId=111, @Name=N'Mountfield HK', @Code=N'HKR', @ValidFrom='2020-01-01';
EXEC dbo.Folio_DevCompany_MERGE @RegistryEntityId=112, @Name=N'Motor České Budějovice', @Code=N'CBU', @ValidFrom='2020-01-01';
EXEC dbo.Folio_DevCompany_MERGE @RegistryEntityId=113, @Name=N'HC Škoda Plzeň', @Code=N'PLZ', @ValidFrom='2020-01-01';
EXEC dbo.Folio_DevCompany_MERGE @RegistryEntityId=114, @Name=N'Rytíři Kladno', @Code=N'KLA', @ValidFrom='2020-01-01';
EXEC dbo.Folio_DevCompany_MERGE @RegistryEntityId=115, @Name=N'HC Dukla Jihlava', @Code=N'JIH', @ValidFrom='2020-01-01';
EXEC dbo.Folio_DevCompany_MERGE @RegistryEntityId=116, @Name=N'HC Stadion Litoměřice', @Code=N'LTM', @ValidFrom='2020-01-01';
EXEC dbo.Folio_DevCompany_MERGE @RegistryEntityId=117, @Name=N'BK Mladá Boleslav', @Code=N'MBO', @ValidFrom='2020-01-01';
EXEC dbo.Folio_DevCompany_MERGE @RegistryEntityId=118, @Name=N'HC Frýdek-Místek', @Code=N'FMI', @ValidFrom='2020-01-01';
EXEC dbo.Folio_DevCompany_MERGE @RegistryEntityId=119, @Name=N'HC Zubr Přerov', @Code=N'PRE', @ValidFrom='2020-01-01';
EXEC dbo.Folio_DevCompany_MERGE @RegistryEntityId=120, @Name=N'HC Slovan Ústí nad Labem', @Code=N'UST', @ValidFrom='2020-01-01';
EXEC dbo.Folio_DevCompany_MERGE @RegistryEntityId=121, @Name=N'Horácká Slavia Třebíč', @Code=N'TRE', @ValidFrom='2020-01-01';
EXEC dbo.Folio_DevCompany_MERGE @RegistryEntityId=122, @Name=N'HC Benátky nad Jizerou', @Code=N'BEN', @ValidFrom='2020-01-01';
EXEC dbo.Folio_DevCompany_MERGE @RegistryEntityId=123, @Name=N'Berounští Medvědi', @Code=N'BER', @ValidFrom='2020-01-01';
EXEC dbo.Folio_DevCompany_MERGE @RegistryEntityId=124, @Name=N'HC Tábor', @Code=N'TAB', @ValidFrom='2020-01-01';
EXEC dbo.Folio_DevCompany_MERGE @RegistryEntityId=125, @Name=N'HC Baník Sokolov', @Code=N'SOK', @ValidFrom='2020-01-01';
EXEC dbo.Folio_DevCompany_MERGE @RegistryEntityId=126, @Name=N'Jestřábi Prostějov', @Code=N'PRO', @ValidFrom='2020-01-01';
EXEC dbo.Folio_DevCompany_MERGE @RegistryEntityId=127, @Name=N'AZ Havířov', @Code=N'HAV', @ValidFrom='2020-01-01';
EXEC dbo.Folio_DevCompany_MERGE @RegistryEntityId=128, @Name=N'HC RT Torax Poruba', @Code=N'POR', @ValidFrom='2020-01-01';
EXEC dbo.Folio_DevCompany_MERGE @RegistryEntityId=129, @Name=N'HC Stadion Vrchlabí', @Code=N'VRC', @ValidFrom='2020-01-01';
EXEC dbo.Folio_DevCompany_MERGE @RegistryEntityId=130, @Name=N'SK Kadaň', @Code=N'KAD', @ValidFrom='2020-01-01';
GO

PRINT 'Seed data applied successfully.';
GO
