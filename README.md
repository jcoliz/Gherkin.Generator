# Gherkin.Generator

[![.NET](https://github.com/jcoliz/Gherkin.Generator/actions/workflows/build/badge.svg)](https://github.com/jcoliz/Gherkin.Generator/actions/workflows/build)

Generate running C# test code from Gherkin feature files.

## What It Does

Gherkin.Generator converts Gherkin `.feature` files into executable C# test methods, eliminating the need to manually write test code for each scenario. It supports standard Gherkin features including scenarios, scenario outlines with example tables, and step definitions.

## How It Works

1. Write your test scenarios in Gherkin syntax (`.feature` files)
2. The generator converts them to a CRIF (Common Runtime Intermediate Format) YAML representation
3. CRIF is transformed into C# test code using Mustache templates
4. Generated code integrates with your existing test infrastructure

## Components

- **[`Gherkin.Generator.Lib`](src/Lib/)** - Core parsing and generation logic
- **[`templates/`](templates/)** - Mustache templates for code generation

## Usage

The library provides two main classes:

- [`GherkinToCrifConverter`](src/Lib/GherkinToCrifConverter.cs) - Parses Gherkin and produces CRIF YAML
- [`FunctionalTestGenerator`](src/Lib/FunctionalTestGenerator.cs) - Generates C# code from CRIF using templates

See [`tests/Unit/`](tests/Unit/) for usage examples.

## Getting Started

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
