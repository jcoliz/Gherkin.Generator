# Developer Documentation

This guide is for developers working on the Gherkin.Generator project itself.

## Project Structure

The project contains:

- **[`src/Analyzer/`](../src/Analyzer/)** - Source generator and analyzer
  - [`GherkinSourceGenerator.cs`](../src/Analyzer/GherkinSourceGenerator.cs) - Incremental source generator
  - [`StepMethodAnalyzer.cs`](../src/Analyzer/StepMethodAnalyzer.cs) - Discovers step definitions
- **[`src/Lib/`](../src/Lib/)** - Core library
  - [`GherkinToCrifConverter.cs`](../src/Lib/GherkinToCrifConverter.cs) - Parses Gherkin to CRIF
  - [`FunctionalTestGenerator.cs`](../src/Lib/FunctionalTestGenerator.cs) - Generates C# from CRIF
- **[`tests/Unit/`](../tests/Unit/)** - Unit tests with comprehensive coverage
- **[`templates/`](../templates/)** - Mustache templates for code generation

## Building

```bash
# Build the solution
dotnet build

# Build specific project
dotnet build src/Analyzer
dotnet build src/Lib
```

## Testing

```bash
# Run all unit tests
dotnet test tests/Unit

# Run tests with coverage
.\scripts\Run-TestsWithCoverage.ps1

# Run specific test
dotnet test tests/Unit --filter "FullyQualifiedName~GherkinToCrifConverterTests"
```

## Packaging

```bash
# Create NuGet package
dotnet pack src/Analyzer -c Release

# Package will be created at:
# src/Analyzer/bin/Release/Gherkin.Generator.0.0.1.nupkg
```

## Development Workflow

1. **Make changes** to source code
2. **Update unit tests** in [`tests/Unit/`](../tests/Unit/) to match changes
3. **Run tests** to verify all pass: `dotnet test tests/Unit`
4. **Build solution** to ensure no compilation errors: `dotnet build`
5. **Commit changes** following [Commit Conventions](COMMIT-CONVENTIONS.md)

## Key Conventions

### Test Documentation

Tests use Gherkin-style comments instead of Arrange/Act/Assert:

```csharp
[Test]
public void ParseScenario_ValidInput_ReturnsScenario()
{
    // Given: A valid Gherkin scenario
    var input = "Scenario: Test\nGiven something";

    // When: Parsing the scenario
    var result = parser.ParseScenario(input);

    // Then: Should return valid scenario
    Assert.That(result, Is.Not.Null);
}
```

### XML Documentation

All public classes and methods require comprehensive XML documentation:

```csharp
/// <summary>
/// Converts Gherkin feature files to CRIF format.
/// </summary>
/// <param name="logger">Logger for diagnostic output.</param>
public class GherkinToCrifConverter(ILogger? logger = null)
{
    /// <summary>
    /// Converts a Gherkin feature file to CRIF YAML format.
    /// </summary>
    /// <param name="featureText">The Gherkin feature file content.</param>
    /// <returns>CRIF YAML representation of the feature.</returns>
    public string Convert(string featureText)
    {
        // Implementation
    }
}
```

Tests should describe behavior and requirements, not implementation details. Use one primary action per test where practical and add tests for behavior changes and regressions. Run the relevant tests after modifying tests or production behavior. For required coverage, complexity, and package-based validation thresholds, follow the project-wide [agent instructions](../AGENTS.md) and [testing approach](TESTING-APPROACH.md).

Use NUnit's `[Explicit("reason")]` for tests that should not run by default because they require special infrastructure or manual verification; do not use `[Ignore]` for this purpose.

### Production Regexes

Prefer source-generated regular expressions in production code using `[GeneratedRegex]` on a partial method. Ordinary regexes are acceptable in test code when they improve readability.

### XML Documentation

- Add XML `<summary>` documentation to public and internal APIs, classes, and record types. Document parameters and meaningful return values; document exceptions the method intentionally throws.
- Document primary-constructor parameters in the type's XML comments. Positional record properties do not need duplicate property comments.
- Test methods marked with `[Test]` do not need XML comments; test fixture types and test helpers do.
- Document private methods when their purpose or behavior is not obvious. Logger-message methods do not need comments.
- Use clear, consistent descriptions for common parameters, such as "Logger for diagnostic output."

### PowerShell Scripts

- Include comment-based help with a synopsis, description, examples, and prerequisites; document parameters and relevant links where applicable.
- Use `[CmdletBinding()]`, validate parameters, and set `$ErrorActionPreference = "Stop"`.
- Resolve paths from `$PSScriptRoot`; use `Push-Location`/`Pop-Location` with `try/finally` when changing directories.
- Check `$LASTEXITCODE` after external commands and report failures explicitly. Keep helper functions before the main script body.
- Use PascalCase approved verb-noun names for scripts and functions. Add new scripts to [`scripts/README.md`](../scripts/README.md).
- Follow existing script patterns in [`scripts/`](../scripts/).

### Documentation and Planning

- Put drafts, proposals, architecture plans, and investigation notes in [`docs/wip/`](wip/); keep finalized project documentation in `docs/` or the relevant project directory.
- Do not add a new top-level directory without first asking the user.
- Use YAML frontmatter for planning documents that need status, dates, or other metadata. Keep it relevant to the document type.
- Keep [`docs/USER-GUIDE.md`](USER-GUIDE.md) current as both the user-facing guide and functional specification.

### Commit Messages

Follow [Commit Conventions](COMMIT-CONVENTIONS.md) for message format, scopes, and examples.

## Architecture

### Generation Pipeline

1. **Gherkin → CRIF**: [`GherkinToCrifConverter`](../src/Lib/GherkinToCrifConverter.cs) parses `.feature` files into CRIF YAML
2. **CRIF → C#**: [`FunctionalTestGenerator`](../src/Lib/FunctionalTestGenerator.cs) uses Mustache templates to generate test code
3. **Source Generator**: [`GherkinSourceGenerator`](../src/Analyzer/GherkinSourceGenerator.cs) orchestrates the pipeline at build time

### CRIF Format

CRIF (Common Runtime Intermediate Format) is a YAML representation that bridges Gherkin and generated code:

```yaml
namespace: MyTests
class: MyFeatureTests
scenarios:
  - name: My_Scenario
    steps:
      - step_type: Given
        step_text: something exists
        method: GivenSomethingExists
```

## Debugging Source Generators

Source generators can be challenging to debug. Options:

1. **Unit tests** - Test [`GherkinToCrifConverter`](../src/Lib/GherkinToCrifConverter.cs) and [`FunctionalTestGenerator`](../src/Lib/FunctionalTestGenerator.cs) directly
2. **Generated files** - Check `obj/` directory for generated source files
3. **Build output** - Enable detailed logging: `dotnet build -v detailed`
4. **Debugger** - Attach to source generator process (advanced)

## Contributing

1. **Follow conventions** documented in this guide
2. **Maintain test coverage** - Add unit tests for all new functionality
3. **Update documentation** - Keep README and docs in sync with changes
4. **Use commit conventions** - Follow [Commit Conventions](COMMIT-CONVENTIONS.md)

## Release Process

See [Project Plan](wip/PROJECT-PLAN.md) for detailed release strategy.

For initial 0.0.1 release:
1. Verify all tests pass
2. Update version in [`Gherkin.Generator.csproj`](../src/Analyzer/Gherkin.Generator.csproj)
3. Create package: `dotnet pack src/Analyzer -c Release`
4. Push to NuGet (manual initially, automated via GitHub Actions later)

## Resources

- [Roslyn Source Generators](https://github.com/dotnet/roslyn/blob/main/docs/features/source-generators.md)
- [Gherkin Syntax](https://cucumber.io/docs/gherkin/reference/)
- [Mustache Template Syntax](https://mustache.github.io/mustache.5.html)
