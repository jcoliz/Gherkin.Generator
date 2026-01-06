# Gherkin.Generator

[![CI](https://github.com/jcoliz/Gherkin.Generator/actions/workflows/ci.yml/badge.svg)](https://github.com/jcoliz/Gherkin.Generator/actions/workflows/ci.yml)
[![NuGet](https://img.shields.io/nuget/v/Gherkin.Generator.svg)](https://www.nuget.org/packages/Gherkin.Generator/)

A source generator that converts Gherkin `.feature` files into executable C# test methods, enabling behavior-driven development with automatic test code generation.

## Features

- **Automatic code generation** - Converts `.feature` files to C# test methods at build time
- **Scenario support** - Handles standard scenarios and scenario outlines with example tables
- **Flexible templating** - Uses Mustache templates for customizable code generation
- **Step matching** - Discovers and maps Gherkin steps to your step implementation methods

## Installation

Add to your test project:

```xml
<PackageReference Include="Gherkin.Generator" Version="0.0.1" />
```

## Quick Start

1. **Write feature files** - Create `.feature` files with Gherkin scenarios
2. **Get template** - Copy [`Default.mustache`](templates/Default.mustache) to your Features folder
3. **Add to project** - Include as AdditionalFiles in `.csproj`:
   ```xml
   <ItemGroup>
     <AdditionalFiles Include="Features\*.feature" />
     <AdditionalFiles Include="Features\Default.mustache" />
   </ItemGroup>
   ```
4. **Define step methods** - Write step implementations with `[Given]`, `[When]`, `[Then]` attributes
5. **Build** - Test methods generate automatically

Customize the template to match your test infrastructure and conventions.

## Documentation

- [Developer Guide](docs/DEVELOPER.md) - For contributors working on this project
- [Commit Conventions](docs/COMMIT-CONVENTIONS.md) - Contribution guidelines

## Status

In active development. Core features complete, preparing for initial NuGet release.
