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

**Current Status:** Uses the generator to create tests from its feature files. However, it references the currently-released version from NuGet, not the in-development version. This means it validates that the generator works in general, but cannot test changes made to the generator until those changes are packaged and deployed.

**Testing Workflow:**
1. Example project references a published/packaged version of the generator
2. Build the Example project (triggering source generation from feature files)
3. Run the generated tests to validate behavior
4. Tests pass, confirming the generator works end-to-end

**The Problem:** To test changes to the generator itself, we must:
1. Make changes to the generator code
2. Package the new version (local or published)
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
4. **Iterate** until unit tests pass

This gives us confidence that the generator logic is correct without requiring packaging.

### Before Release

Currently, we rely on unit tests to validate generator changes. The Example project continues to use the stable published version.

This workflow is documented in more detail in [`RELEASE-WORKFLOW.md`](../RELEASE-WORKFLOW.md).

### In CI/CD

Our CI/CD pipeline could be improved to:
1. Build the solution
2. Run unit tests (fast validation)
3. Pack the generator
4. Update Example project to use the packed version
5. Build Example project (triggering generation)
6. Run Example tests (end-to-end validation)

This would ensure that every commit produces a working, deployable generator.

## Future: Pre-Release Testing Strategy

For a more robust release process, we should adopt a **pre-release validation pattern**:

### Recommended Future Workflow

1. **Pack as pre-release version** (e.g., `1.2.0-beta.1`)
   ```powershell
   dotnet pack src/Analyzer --version-suffix beta.1
   ```

2. **Publish to NuGet as pre-release** or local feed
   ```powershell
   dotnet nuget push ./local-packages/*.nupkg --source local
   ```

3. **Update Example project** to reference pre-release version:
   ```xml
   <PackageReference Include="Gherkin.Generator" Version="1.2.0-beta.1" />
   ```

4. **Run Example tests** against pre-release version
   - Build Example project (triggers generation)
   - Execute generated tests
   - Verify all tests pass

5. **If tests pass:** Promote the same bits to release version
   - Republish same package with release version (e.g., `1.2.0`)
   - No code changes between pre-release and release
   - Same binary artifacts, just version number changes

6. **If tests fail:** Fix issues and repeat with new pre-release version

## Why Can't Example Tests Validate In-Development Generator Changes?

The Example project DOES use the generator to create tests from feature files. However, it can only test against packaged/deployed versions of the generator, not the in-development version.

**The Limitation:** To test changes made to the generator itself requires packaging infrastructure for every development iteration. This creates a validation gap:

- We make changes to the generator code
- Unit tests validate the logic is correct
- But we cannot easily verify the changes work end-to-end until we package and deploy them
- Only after packaging can the Example project test against the new version

**Current Approach:** The Example project references a stable published version of the generator. This validates that the generator works end-to-end, but cannot catch regressions or validate new features until after they're packaged.

**Our Strategy:**
1. **During development:** Rely on comprehensive unit tests for fast validation
2. **Before release:** Package the generator and update Example project to test the new version
3. **After validation passes:** Publish the tested version as a release

This acknowledges that thorough end-to-end testing requires packaging infrastructure, which is appropriate for pre-release validation but too heavyweight for rapid development iteration.

## Summary

| Test Layer | Purpose | Speed | When | Confidence Level |
|------------|---------|-------|------|------------------|
| **Unit Tests** | Validate generator logic | Fast | Every change | High for logic |
| **Example Project** | Validate deployment | Slow | Before release | High for integration |
| **Pre-Release** | Validate in real usage | Slowest | Before publish | Highest |

Our testing strategy prioritizes:
1. **Fast feedback** via comprehensive unit tests
2. **Deployment validation** via Example project testing before release
3. **Production confidence** via pre-release testing (future)

This approach acknowledges the reality that **source generators are not normal libraries** and adapts our testing strategy accordingly.

## References

- [`RELEASE-WORKFLOW.md`](../RELEASE-WORKFLOW.md) - Step-by-step release process including testing
- [`tests/Example/README.md`](../../tests/Example/README.md) - Example project documentation
- [Roslyn Source Generators Cookbook](https://github.com/dotnet/roslyn/blob/main/docs/features/source-generators.cookbook.md) - Official guidance on source generators
- [Deploying Source Generators](https://github.com/dotnet/roslyn/blob/main/docs/features/source-generators.cookbook.md#distributing-source-generators-as-nuget-packages) - Why NuGet packages are the recommended distribution method
