# Setup Test Environment script

## Abstract

Automates local testing of analyzer changes by packaging and installing into Example project.
Addresses Roslyn assembly loading limitation (see docs/TESTING-APPROACH.md).

## Goal

Enable rapid iteration on analyzer features without publishing to NuGet.

## Actions

1. Check git status - Fail if uncommitted changes (unless -SkipGitCheck)
2. Build and pack analyzer:
   - Build solution in Release mode
   - Create local package with timestamped version (9999.0.0-local.TIMESTAMP)
   - Pack to local-packages directory
   - Clear NuGet caches
3. Install local package into Example:
   - Add local-packages as NuGet source (if needed)
   - Update Example.csproj package reference to new version
   - Force restore Example project
4. Build and test Example:
   - Clean Example project
   - Build (triggers source generation with new analyzer version)
   - Run tests to validate behavior
5. Cleanup:
   - Restore Example.csproj to original state (unless -KeepChanges)
   - Display results and next steps

## Parameters

- None for now

## Error Handling:

Each step should validate success before proceeding
Use try/finally to ensure cleanup runs even on failure

## Diagnostic Output:

Show package version being created
Show local feed path
Show generated files location
Colorize output (Cyan for info, Green for success, Yellow for warnings)

## Success Criteria

- All Example tests pass
- Generated code compiles without errors
- No source generator diagnostics/warnings
