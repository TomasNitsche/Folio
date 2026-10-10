# Creates Folio database and applies schema, stored procedures, and seed data.
# Requires sqlcmd (SQL Server tools) or SSMS.

param(
    [string]$Server = "localhost\SQLEXPRESS",
    [string]$Database = "Folio"
)

$ErrorActionPreference = "Stop"
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path

function Invoke-SqlFile {
    param([string]$File)
    Write-Host "Running $File ..."
    sqlcmd -S $Server -b -f 65001 -i $File
    if ($LASTEXITCODE -ne 0) { throw "Failed: $File" }
}

Write-Host "=== Folio database setup ===" -ForegroundColor Cyan
Write-Host "Server: $Server"

Invoke-SqlFile (Join-Path $scriptDir "000_CreateDatabase.sql")

$files = @(
    "001_Schema.sql",
    "002_StoredProcedures.sql",
    "003_SeedData.sql"
)

foreach ($file in $files) {
    $path = Join-Path $scriptDir $file
    sqlcmd -S $Server -d $Database -b -f 65001 -i $path
    if ($LASTEXITCODE -ne 0) { throw "Failed: $file" }
    Write-Host "OK: $file" -ForegroundColor Green
}

Write-Host ""
Write-Host "Done. Database '$Database' is ready." -ForegroundColor Green
Write-Host "Next: set Persistence:Provider=Sql in appsettings.Development.json and run the API." -ForegroundColor Yellow
