# Gherkin.Generator

[![CI](https://github.com/jcoliz/Gherkin.Generator/actions/workflows/ci.yml/badge.svg)](https://github.com/jcoliz/Gherkin.Generator/actions/workflows/ci.yml)

Generate running C# test code from Gherkin feature files.

## What It Does

Gherkin.Generator converts Gherkin `.feature` files into executable C# test methods, eliminating the need to manually write test code for each scenario. It supports standard Gherkin features including scenarios, scenario outlines with example tables, and step definitions.

## How It Works

1. Write your test scenarios in Gherkin syntax (`.feature` files)
2. The generator converts them to a CRIF (Common Runtime Intermediate Format) YAML representation
3. CRIF is transformed into C# test code using Mustache templates
4. Generated code integrates with your existing test infrastructure

## Components

- **[`Gherkin.Generator`](src/Analyzer/)** - Roslyn source generator and analyzer for automatic code generation
- **[`Gherkin.Generator.Lib`](src/Lib/)** - Core parsing and generation logic
- **[`templates/`](templates/)** - Mustache templates for code generation

## Usage

### For End Users (Coming Soon)

Once published to NuGet, add the analyzer to your test project:

```xml
<PackageReference Include="Gherkin.Generator" Version="0.0.1" />
```

Then:

1. **Write feature files** - Create `.feature` files with your Gherkin scenarios
2. **Add to project** - Include them as AdditionalFiles in your `.csproj`:
   ```xml
   <ItemGroup>
     <AdditionalFiles Include="Features\*.feature" />
     <AdditionalFiles Include="Features\YourTemplate.mustache" />
   </ItemGroup>
   ```
3. **Define step implementations** - Write step methods with `[Given]`, `[When]`, `[Then]` attributes
4. **Build** - The generator automatically creates test methods at build time

### For Developers

The project contains:

- **[`Gherkin.Generator`](src/Analyzer/)** - Source generator and analyzer
  - [`GherkinSourceGenerator`](src/Analyzer/GherkinSourceGenerator.cs) - Incremental source generator
  - [`StepMethodAnalyzer`](src/Analyzer/StepMethodAnalyzer.cs) - Discovers step definitions
- **[`Gherkin.Generator.Lib`](src/Lib/)** - Core library
  - [`GherkinToCrifConverter`](src/Lib/GherkinToCrifConverter.cs) - Parses Gherkin to CRIF
  - [`FunctionalTestGenerator`](src/Lib/FunctionalTestGenerator.cs) - Generates C# from CRIF

See [`tests/Unit/`](tests/Unit/) for usage examples.

```bash
# Build the solution
dotnet build

# Run tests
dotnet test tests/Unit

# Run tests with coverage
.\scripts\Run-TestsWithCoverage.ps1
```

## Documentation

- [Project Plan](docs/wip/PROJECT-PLAN.md) - Development roadmap and architecture
- [Commit Conventions](docs/COMMIT-CONVENTIONS.md) - Contribution guidelines

## Status

This project is in active development. The core Gherkin parsing and code generation features are complete and being prepared for initial release to NuGet.
