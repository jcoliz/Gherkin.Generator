---
status: Draft
created: 2026-01-06
related_documents:
  - docs/RELEASE-WORKFLOW.md
  - tests/Example/README.md
---

# Testing Approach for Gherkin.Generator

## Overview

This document describes our testing strategy for the Gherkin.Generator source generator project, focusing on the unique challenges of testing source generators and our pragmatic approach to validation.

## The Core Challenge: Why We Cannot Easily Test a New Generator

**We cannot easily test the generator in-place using project references.**

Unlike typical .NET libraries where you can simply add a project reference and start using the code, source generators have special runtime requirements that make them effectively untestable without packaging infrastructure.

### The Problem: Source Generator Dependencies and Roslyn's Assembly Loading

The Gherkin.Generator has two critical runtime dependencies:
- **Gherkin** (v30.0.2) - For parsing .feature files
- **Stubble.Core** (v1.10.8) - For Mustache template rendering

These are NOT compile-time dependencies—the generator needs them at runtime when Roslyn executes the source generator during compilation.

#### Roslyn's Assembly Loading Model

When the C# compiler (Roslyn) loads a source generator:

1. The generator DLL is loaded into an isolated `AssemblyLoadContext`
2. Roslyn looks for the generator's dependencies in specific locations:
   - The analyzer/generator's original directory (for package references)
   - NuGet package cache directories
3. Roslyn **does NOT** automatically probe:
   - The generator's build output directory (for project references)
   - The consuming project's dependency directories
   - Transitive dependency paths

This means when you use a project reference to a source generator, even though `Gherkin.dll` and `Stubble.Core.dll` exist in the generator's output directory, **the compiler cannot find them at runtime**.

#### Error Manifestation

```
CSC : error GHERKIN002: Error generating test for feature:
Could not load file or assembly 'Gherkin, Version=30.0.2.0, Culture=neutral,
PublicKeyToken=86496cfa5b4a5851'. The system cannot find the file specified.
```

### What This Means for Testing

- We cannot have a "test project" that references the generator project and validates its output
- We cannot easily iterate on generator changes and see results immediately
- We cannot run integration tests in the normal .NET development workflow
- **Every test of the generator requires packaging it first**

### Why Package References Work

Source generators **do** work reliably when consumed as NuGet packages because:

1. NuGet packages declare dependencies in their `.nuspec` file
2. NuGet restores all dependencies to the package cache
3. Roslyn knows to look in the package cache for analyzer dependencies
4. Dependencies are properly isolated per package

**This is why Microsoft's own source generators (Regex, JSON serialization, etc.) are always distributed as NuGet packages, never as project references.**

## Current Testing Strategy

Given these constraints, we employ a multi-layered testing approach:

### 1. Comprehensive Unit Testing (Primary Validation)

**Location:** [`tests/Unit/`](../../tests/Unit/)

**Coverage:** 80+ unit tests covering all core functionality

**What We Test:**
- Gherkin parsing and AST construction
- CRIF (Common Representation Intermediate Format) conversion
- Step matching logic (parameterized steps, And keyword handling, etc.)
- Code generation via Mustache templates
- Error handling and diagnostics
- Edge cases and boundary conditions

**Approach:**
- Test the generator's internal components in isolation
- Mock file system access using in-memory strings
- Validate generated code as text output
- Verify step matching algorithms with known inputs/outputs
- Test error conditions and diagnostic reporting

**Benefits:**
- Fast execution (no packaging required)
- Easy to debug and iterate
- High code coverage achievable
- Reliable CI/CD integration

**Limitations:**
- Does not test actual Roslyn integration
- Does not validate that generated code actually compiles
- Does not test end-to-end workflow

### 2. Example Project Testing (Deployment Validation)

**Location:** [`tests/Example/`](../../tests/Example/)

**Purpose:** Validate that a deployed/packaged version of the generator actually works in a real project

**Current Status:** Uses the generator to create tests from its feature files and normally references the released version from NuGet. To test in-development changes, temporarily reference a locally packed pre-release version using the workflow below; publishing to NuGet is not required for this validation.

**Testing Workflow:**
1. Example project references a published/packaged version of the generator
2. Build the Example project (triggering source generation from feature files)
3. Run the generated tests to validate behavior
4. Tests pass, confirming the generator works end-to-end

**The Requirement:** To test changes to the generator itself end to end, we must:
1. Make changes to the generator code
2. Package the new version locally or publish it
3. Update Example project to reference the new package version
4. Build Example project to trigger generation with the new version
5. Run the generated tests to validate the changes

**What This Validates:**
- The generator works in Roslyn's execution environment
- All dependencies are properly included in the package
- Generated code compiles correctly
- Generated tests can execute and pass
- End-to-end user experience

## Pragmatic Testing Workflow

Since we cannot easily test the generator in-place, our testing workflow follows this pattern:

### During Development

1. **Write/modify generator code** in [`src/Analyzer/`](../../src/Analyzer/) or [`src/Lib/`](../../src/Lib/)
2. **Write/update unit tests** in [`tests/Unit/`](../../tests/Unit/) to cover the changes
3. **Run unit tests** to validate correctness: `dotnet test tests/Unit`
4. **Collect and inspect coverage** with `.\scripts\Collect-CodeCoverage.ps1`. Every non-generated method with executable lines must have at least 90% line coverage.
5. **Resolve complexity warnings** so there are zero cyclomatic-complexity warnings.
6. **Iterate** until the tests pass and both quality thresholds are met.

Unit tests validate generator logic, but they do not replace package-based validation against the Example project.

### Before Committing an Unreleased Feature

Use the local pre-release package workflow below to test generator changes end to end. The package script temporarily updates the Example project; undo that package injection and remove any temporary Example feature, template, or test changes before committing the unreleased feature. Do not commit `local-packages` or pre-release package references.

After that feature is published to NuGet, update the Example project to demonstrate the released functionality and commit that as a separate follow-up change. See [`RELEASE-WORKFLOW.md`](../RELEASE-WORKFLOW.md).

### In CI/CD

The CI workflow restores and builds the solution, runs unit tests with coverage collection, and uploads the Cobertura report. CI coverage collection does not replace the required local report review for the per-method coverage threshold, and the CI workflow does not perform the local pre-release package validation.

Run the local package validation script when changing generator behavior or package output, as described below.

## Pre-Release Testing with a Local Package Feed

To validate in-development generator changes end-to-end before publishing, pack the generator to a
local folder feed and point the Example project at it. This is automated by two scripts in
[`scripts/`](../../scripts/):

### Running the Validation

```powershell
.\scripts\Test-PreReleasePackage.ps1
```

This script:

1. **Packs a pre-release version** of both `src/Analyzer` and `src/Utils` into `./local-packages`.
   Both must be packed with the same version—`Gherkin.Generator.Utils` is a real package dependency
   of the generator, not embedded in its analyzer payload, so packing only `src/Analyzer` fails
   restore with `NU1101`.
   ```powershell
   dotnet pack src/Analyzer -c Release -p:Version=<version> -o ./local-packages
   dotnet pack src/Utils -c Release -p:Version=<version> -o ./local-packages
   ```
   By default the version is timestamped (e.g. `0.0.1-local.20260826193044`) so repeated runs don't
   collide with stale entries in the local NuGet cache. Pass `-Version` to pin a specific value.

2. **Updates the Example project** to reference the pre-release version from the local feed:
   ```powershell
   dotnet add tests/Example package Gherkin.Generator --version <version> --source ./local-packages
   ```

3. **Restores using both the local feed and nuget.org**—the Example project's other dependencies
   (NUnit, etc.) still need to resolve from nuget.org:
   ```powershell
   dotnet restore tests/Example --source ./local-packages --source https://api.nuget.org/v3/index.json
   ```

4. **Runs the Example project's tests** against the pre-release build. Note `dotnet test` doesn't
   accept `--source`, so the restore step above must run first:
   ```powershell
   dotnet test tests/Example
   ```

If all tests pass, the pre-release bits are validated end to end. Do not commit the temporary
Example package reference or temporary Example changes used to exercise unreleased functionality.
Undo the package injection with the script below and inspect the final diff before committing.

### Undoing the Validation

```powershell
.\scripts\Undo-PreReleasePackage.ps1
```

This reverts `tests/Example/Gherkin.Generator.Tests.Example.csproj` to its committed state via
`git checkout`, deletes `./local-packages`, and restores the Example project against its committed
package version. Check the working tree first: the script uses `git checkout` on the Example project
file, so do not run it if that file contains pre-existing work you need to keep. Also manually remove
any temporary feature, template, or test changes added to the Example project for the pre-release
validation. Confirm the final diff contains none of those temporary changes.

## Why Must Example Tests Use a Package?

The Example project uses the generator to create tests from feature files. It cannot reliably validate the generator through a project reference, so in-development changes must first be packed and injected as a local NuGet package. The local package can be tested without publishing it.

**The Constraint:** Testing changes made to the generator itself requires packaging the changed code for the consumer build:

- We make changes to the generator code
- Unit tests validate the logic is correct
- We package the changed code locally
- The Example project tests the local package end to end

**Current Approach:** During development, temporarily inject a local pre-release package into the Example project to validate changes end to end. Restore the Example project to its committed state before committing unreleased functionality. Once the version is published to NuGet, update the Example project to demonstrate that released feature in a separate follow-up change.

**Our Strategy:**
1. **During development:** Use unit tests, inspect per-method coverage, and resolve cyclomatic-complexity warnings
2. **Before committing unreleased generator changes:** Test the packed build with the local Example workflow, then undo temporary Example changes
3. **After publishing to NuGet:** Add or update Example tests for the released functionality and commit that separately

This keeps end-to-end validation tied to the actual NuGet package layout while keeping unreleased package and Example-project changes out of feature commits.

## Summary

| Test Layer | Purpose | Speed | When | Confidence Level |
|------------|---------|-------|------|------------------|
| **Unit Tests** | Validate generator logic | Fast | Every change | High for logic |
| **Example Project** | Validate deployment | Slow | Before release | High for integration |
| **Pre-Release** | Validate in real usage | Slowest | Before publish | Highest |

Our testing strategy prioritizes:
1. **Fast feedback** via comprehensive unit tests
2. **Packaged end-to-end validation** via a local pre-release package and the Example project
3. **A published-feature example** via Example project updates after publication

This approach acknowledges the reality that **source generators are not normal libraries** and adapts our testing strategy accordingly.

## References

- [`RELEASE-WORKFLOW.md`](../RELEASE-WORKFLOW.md) - Step-by-step release process including testing
- [`tests/Example/README.md`](../../tests/Example/README.md) - Example project documentation
- [Roslyn Source Generators Cookbook](https://github.com/dotnet/roslyn/blob/main/docs/features/source-generators.cookbook.md) - Official guidance on source generators
- [Deploying Source Generators](https://github.com/dotnet/roslyn/blob/main/docs/features/source-generators.cookbook.md#distributing-source-generators-as-nuget-packages) - Why NuGet packages are the recommended distribution method
