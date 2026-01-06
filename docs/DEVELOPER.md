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

## Project Rules

See [`.roorules`](../.roorules) for detailed development conventions including:
- Test documentation with Gherkin-style comments (Given/When/Then)
- Source-generated regex patterns
- XML documentation requirements
- Test execution patterns

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

1. **Follow conventions** documented in [`.roorules`](../.roorules)
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
