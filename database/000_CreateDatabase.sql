/*
  Creates the Folio database on LocalDB (or change the server name).
  Run once from SSMS or sqlcmd.
*/
IF DB_ID(N'Folio') IS NULL
BEGIN
    CREATE DATABASE Folio;
    PRINT 'Database Folio created.';
END
ELSE
    PRINT 'Database Folio already exists.';
GO
