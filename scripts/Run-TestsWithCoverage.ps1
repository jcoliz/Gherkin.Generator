<#
.SYNOPSIS
Runs unit tests with code coverage collection and generates an HTML report.

.DESCRIPTION
This script executes all unit tests in the tests/Unit directory with XPlat Code Coverage
collection, then generates a human-readable HTML coverage report using ReportGenerator.
The report is opened automatically in the default browser.

.EXAMPLE
.\Run-TestsWithCoverage.ps1
Runs all unit tests, collects coverage, and opens the coverage report in browser.

.NOTES
Requires .NET SDK to be installed and available in PATH.
ReportGenerator tool will be installed automatically if not present.
#>

[CmdletBinding()]
param()

$ErrorActionPreference = "Stop"

try {
    $repoRoot = Split-Path $PSScriptRoot -Parent
    Push-Location $repoRoot

    Write-Host "Running unit tests with code coverage..." -ForegroundColor Cyan
    
    # Run tests with coverage collection
    dotnet test tests/Unit --configuration Debug --collect:"XPlat Code Coverage"
    if ($LASTEXITCODE -ne 0) {
        throw "Tests failed with exit code $LASTEXITCODE"
    }
    Write-Host "OK Tests passed" -ForegroundColor Green

    # Find the coverage file
    $coverageFile = Get-ChildItem -Path "tests/Unit/TestResults" -Filter "coverage.cobertura.xml" -Recurse | 
        Sort-Object LastWriteTime -Descending | 
        Select-Object -First 1

    if (-not $coverageFile) {
        throw "Coverage file not found in tests/Unit/TestResults"
    }

    Write-Host "Found coverage file: $($coverageFile.FullName)" -ForegroundColor Cyan

    # Ensure ReportGenerator tool is installed
    Write-Host "Checking for ReportGenerator tool..." -ForegroundColor Cyan
    $reportGenInstalled = dotnet tool list --global | Select-String "reportgenerator"
    if (-not $reportGenInstalled) {
        Write-Host "Installing ReportGenerator tool..." -ForegroundColor Yellow
        dotnet tool install -g dotnet-reportgenerator-globaltool
        if ($LASTEXITCODE -ne 0) {
            throw "Failed to install ReportGenerator tool"
        }
    }

    # Generate HTML report
    $reportDir = Join-Path $repoRoot "coverage-report"
    Write-Host "Generating HTML coverage report..." -ForegroundColor Cyan
    
    reportgenerator `
        -reports:$($coverageFile.FullName) `
        -targetdir:$reportDir `
        -reporttypes:Html
    
    if ($LASTEXITCODE -ne 0) {
        throw "Failed to generate coverage report"
    }

    Write-Host "OK Coverage report generated" -ForegroundColor Green
    
    # Open the report in the default browser
    $indexPath = Join-Path $reportDir "index.html"
    Write-Host "Opening coverage report: $indexPath" -ForegroundColor Cyan
    Start-Process $indexPath

    Write-Host "\nSummary:" -ForegroundColor Cyan
    Write-Host "  Coverage file: $($coverageFile.FullName)" -ForegroundColor White
    Write-Host "  Report location: $reportDir" -ForegroundColor White
}
catch {
    Write-Error "Failed to run tests with coverage: $_"
    Write-Error $_.ScriptStackTrace
    exit 1
}
finally {
    Pop-Location
}
