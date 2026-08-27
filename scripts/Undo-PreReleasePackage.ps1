<#
.SYNOPSIS
Reverts the changes made by Test-PreReleasePackage.ps1.

.DESCRIPTION
Restores the Example project's Gherkin.Generator PackageReference to its committed state
(via git checkout) and deletes the local-packages folder created during pre-release testing.

.EXAMPLE
.\Undo-PreReleasePackage.ps1
Reverts the Example project and removes local-packages.

.NOTES
Requires git to be installed and available in PATH.
#>

[CmdletBinding()]
param()

$ErrorActionPreference = "Stop"

try {
    $repoRoot = Split-Path $PSScriptRoot -Parent
    Push-Location $repoRoot

    $exampleCsproj = "tests/Example/Gherkin.Generator.Tests.Example.csproj"
    $localPackagesDir = Join-Path $repoRoot "local-packages"

    Write-Host "Reverting $exampleCsproj..." -ForegroundColor Cyan
    git checkout -- $exampleCsproj
    if ($LASTEXITCODE -ne 0) {
        throw "Failed to revert $exampleCsproj"
    }

    if (Test-Path $localPackagesDir) {
        Write-Host "Removing $localPackagesDir..." -ForegroundColor Cyan
        Remove-Item -Recurse -Force $localPackagesDir
    }

    Write-Host "Restoring Example project against the committed package version..." -ForegroundColor Cyan
    dotnet restore tests/Example
    if ($LASTEXITCODE -ne 0) {
        throw "Failed to restore Example project"
    }

    Write-Host "OK Reverted to committed state" -ForegroundColor Green
}
finally {
    Pop-Location
}
