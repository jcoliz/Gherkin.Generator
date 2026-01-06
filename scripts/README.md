# Scripts

PowerShell scripts for common development tasks.

## Available Scripts

### Run-TestsWithCoverage.ps1

Runs unit tests with code coverage collection and generates an HTML report.

**Usage:**
```powershell
.\scripts\Run-TestsWithCoverage.ps1
```

**What it does:**
- Executes all unit tests in `tests/Unit` with XPlat Code Coverage collection
- Automatically installs ReportGenerator tool if not present
- Generates a human-readable HTML coverage report
- Opens the report in your default browser

**Requirements:**
- .NET SDK installed and available in PATH
- ReportGenerator tool (auto-installed if missing)

**Output:**
- Coverage report location: `coverage-report/index.html`
- Coverage data file: `tests/Unit/TestResults/*/coverage.cobertura.xml`
