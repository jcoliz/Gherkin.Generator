<#
.SYNOPSIS
Packs the generator as a local pre-release NuGet package and validates it against the Example project.

.DESCRIPTION
This script packages Gherkin.Generator (and its Gherkin.Generator.Utils dependency) into a local
folder feed under scripts/../local-packages, points the Example project's PackageReference at that
pre-release version, restores, and runs the Example project's tests. This lets you validate
in-development generator changes end-to-end without publishing to NuGet.

Run .\scripts\Undo-PreReleasePackage.ps1 afterwards to revert the Example project changes and remove
the local package folder.

.PARAMETER Version
The pre-release version to pack and reference, e.g. "0.1.12-local.1". Defaults to a
timestamped version to avoid colliding with versions already in the local NuGet cache.

.EXAMPLE
.\Test-PreReleasePackage.ps1
Packs a uniquely-versioned pre-release package and runs Example tests against it.

.EXAMPLE
.\Test-PreReleasePackage.ps1 -Version "0.2.0-local.1"
Packs and tests against a specific pre-release version.

.NOTES
Requires .NET SDK to be installed and available in PATH.
#>

[CmdletBinding()]
param(
    [string]$Version = "0.0.1-local.$(Get-Date -Format 'yyyyMMddHHmmss')"
)

$ErrorActionPreference = "Stop"

try {
    $repoRoot = Split-Path $PSScriptRoot -Parent
    Push-Location $repoRoot

    $localPackagesDir = Join-Path $repoRoot "local-packages"

    Write-Host "Packing pre-release version $Version..." -ForegroundColor Cyan

    if (Test-Path $localPackagesDir) {
        Remove-Item -Recurse -Force $localPackagesDir
    }
    New-Item -ItemType Directory -Path $localPackagesDir | Out-Null

    # Utils must be packed too - it's a real package dependency, not embedded in the analyzer package
    dotnet pack src/Analyzer -c Release -p:Version=$Version -o $localPackagesDir
    if ($LASTEXITCODE -ne 0) {
        throw "Failed to pack src/Analyzer"
    }

    dotnet pack src/Utils -c Release -p:Version=$Version -o $localPackagesDir
    if ($LASTEXITCODE -ne 0) {
        throw "Failed to pack src/Utils"
    }

    Write-Host "OK Packages created in $localPackagesDir" -ForegroundColor Green

    Write-Host "Updating Example project to reference $Version..." -ForegroundColor Cyan
    dotnet add tests/Example package Gherkin.Generator --version $Version --source $localPackagesDir
    if ($LASTEXITCODE -ne 0) {
        throw "Failed to add pre-release package reference to Example project"
    }

    # nuget.org is also required to resolve the Example project's other dependencies (NUnit, etc.)
    Write-Host "Restoring Example project..." -ForegroundColor Cyan
    dotnet restore tests/Example --source $localPackagesDir --source https://api.nuget.org/v3/index.json
    if ($LASTEXITCODE -ne 0) {
        throw "Failed to restore Example project"
    }

    Write-Host "Running Example project tests..." -ForegroundColor Cyan
    dotnet test tests/Example
    if ($LASTEXITCODE -ne 0) {
        throw "Example project tests failed"
    }

    Write-Host "OK Pre-release $Version validated against Example project" -ForegroundColor Green
    Write-Host "Run .\scripts\Undo-PreReleasePackage.ps1 to revert these changes." -ForegroundColor Yellow
}
finally {
    Pop-Location
}
