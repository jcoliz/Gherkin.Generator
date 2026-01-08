# Gherkin.Generator

[![CI](https://github.com/jcoliz/Gherkin.Generator/actions/workflows/ci.yml/badge.svg)](https://github.com/jcoliz/Gherkin.Generator/actions/workflows/ci.yml)
[![codecov](https://codecov.io/gh/jcoliz/Gherkin.Generator/branch/main/graph/badge.svg)](https://codecov.io/gh/jcoliz/Gherkin.Generator)
[![NuGet](https://img.shields.io/nuget/v/Gherkin.Generator.svg)](https://www.nuget.org/packages/Gherkin.Generator/)
[![.NET Standard 2.0](https://img.shields.io/badge/.NET%20Standard-2.0-blue.svg)](https://docs.microsoft.com/en-us/dotnet/standard/net-standard)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

A source generator that converts Gherkin `.feature` files into executable C# test methods, enabling behavior-driven development with automatic test code generation.

## Features

- **Automatic code generation** - Converts `.feature` files to C# test methods at build time
- **Scenario support** - Handles standard scenarios and scenario outlines with example tables
- **Flexible templating** - Uses Mustache templates for customizable code generation
- **Step matching** - Discovers and maps Gherkin steps to your step implementation methods

## Why Gherkin.Generator?

### Advantages Over Reqnroll/SpecFlow

- **⚡ Zero runtime overhead** - All code generation happens at compile-time; no reflection or runtime discovery
- **🔍 Transparent generation** - See exactly what code is generated for debugging and understanding
- **✅ Build-time validation** - Missing or mismatched steps are caught during compilation, not at test execution
- **🎨 Full template control** - Customize generated code via Mustache templates to match any infrastructure
- **📦 Framework agnostic** - Generate for NUnit, xUnit, MSTest, or any custom test framework
- **🚀 Faster test execution** - Direct method calls without runtime binding overhead
- **📋 Automatic stub generation** - Generates stub methods for unimplemented steps with documentation
- **🛠️ Simpler setup** - Just add package, configure `[GeneratedTestBase]` attribute, and include files in `.csproj`

### Key Trade-offs

- **Maturity**: Reqnroll is more mature with richer IDE tooling (Visual Studio/Rider extensions)
- **Extensibility**: Reqnroll offers more runtime flexibility with plugins, hooks, and dependency injection
- **Development workflow**: Reqnroll's runtime binding enables hot reload—modify step implementations and immediately re-run tests without rebuilding.
 
Gherkin.Generator optimizes for **performance, transparency, and control** at the cost of runtime flexibility. Choose Reqnroll if you need advanced runtime features and mature IDE integration. Choose Gherkin.Generator if you want compile-time validation, minimal overhead, and full control over generated code.

## Documentation

- [User Guide](docs/USER-GUIDE.md) - Complete guide for using Gherkin.Generator
- [Developer Guide](docs/DEVELOPER.md) - For contributors working on this project

## Status

In active development. Core features complete. Preparing to test released version internally.

## Acknowledgments

This project was developed with assistance from [Roo Code](https://roo.dev/), an AI coding assistant.
