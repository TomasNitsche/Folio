@echo off
REM Clean and rebuild Folio solution (fixes missing Folio.Application.dll in VS)
setlocal
cd /d "%~dp0"

echo === Cleaning ===
dotnet clean Folio.sln -v q
if exist src\Folio.Application\bin rmdir /s /q src\Folio.Application\bin
if exist src\Folio.Application\obj rmdir /s /q src\Folio.Application\obj
if exist src\Folio.Infrastructure\bin rmdir /s /q src\Folio.Infrastructure\bin
if exist src\Folio.Infrastructure\obj rmdir /s /q src\Folio.Infrastructure\obj
if exist src\Folio.Api\bin rmdir /s /q src\Folio.Api\bin
if exist src\Folio.Api\obj rmdir /s /q src\Folio.Api\obj
if exist src\Folio.Web\bin rmdir /s /q src\Folio.Web\bin
if exist src\Folio.Web\obj rmdir /s /q src\Folio.Web\obj

echo === Building ===
dotnet build Folio.sln
if errorlevel 1 (
    echo BUILD FAILED
    pause
    exit /b 1
)

echo.
echo BUILD OK - open Folio.sln in Visual Studio
pause
