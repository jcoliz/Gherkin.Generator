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

### Test-PreReleasePackage.ps1

Packs the generator as a local pre-release NuGet package and validates it against the Example project. See [`TESTING-APPROACH.md`](../docs/TESTING-APPROACH.md#pre-release-testing-with-a-local-package-feed) for the full workflow.

**Usage:**
```powershell
.\scripts\Test-PreReleasePackage.ps1
.\scripts\Test-PreReleasePackage.ps1 -Version "0.2.0-local.1"
```

**What it does:**
- Packs `src/Analyzer` and `src/Utils` into `./local-packages` with the given (or a timestamped) pre-release version
- Updates `tests/Example`'s `Gherkin.Generator` package reference to that version
- Restores and runs the Example project's tests against the pre-release build

**Requirements:**
- .NET SDK installed and available in PATH

### Undo-PreReleasePackage.ps1

Reverts the changes made by `Test-PreReleasePackage.ps1`.

**Usage:**
```powershell
.\scripts\Undo-PreReleasePackage.ps1
```

**What it does:**
- Reverts `tests/Example/Gherkin.Generator.Tests.Example.csproj` via `git checkout`
- Deletes `./local-packages`
- Restores the Example project against its committed package version

**Requirements:**
- git installed and available in PATH
