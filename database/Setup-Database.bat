@echo off
REM Folio database setup - no PowerShell execution policy required
setlocal EnableDelayedExpansion

set "SERVER=localhost\SQLEXPRESS"
set "DATABASE=Folio"
set "SCRIPT_DIR=%~dp0"

echo === Folio database setup ===
echo Server: %SERVER%
echo.

where sqlcmd >nul 2>&1
if errorlevel 1 (
    echo ERROR: sqlcmd not found.
    echo Install "SQL Server Command Line Utilities" or run scripts manually in SSMS.
    echo See database\README.md
    pause
    exit /b 1
)

echo Running 000_CreateDatabase.sql ...
sqlcmd -S "%SERVER%" -b -i "%SCRIPT_DIR%000_CreateDatabase.sql"
if errorlevel 1 goto failed

for %%F in (001_Schema.sql 002_StoredProcedures.sql 003_SeedData.sql) do (
    echo Running %%F ...
    sqlcmd -S "%SERVER%" -d "%DATABASE%" -b -i "%SCRIPT_DIR%%%F"
    if errorlevel 1 goto failed
    echo OK: %%F
)

echo.
echo Done. Database '%DATABASE%' is ready.
echo Next: set Persistence:Provider=Sql in appsettings.Development.json
echo Then open Folio.sln and run Folio.Api
goto end

:failed
echo.
echo FAILED - check errors above.
pause
exit /b 1

:end
pause
