# Agent Instructions

Agents own implementation, tests, and documentation end to end; do not rely on the user to review code. Work from explicit requirements and automated tests, and do not call a feature complete until the relevant checks below pass.

## Requirements and Documentation

- Treat [`docs/USER-GUIDE.md`](docs/USER-GUIDE.md) as both the user-facing guide and the functional specification. Read the relevant sections before changing behavior.
- Keep the user guide accurate in the same change as any user-visible behavior or feature. Add or update tests to express the requirement; resolve conflicts between the requirements, guide, and tests rather than silently choosing one.
- Follow the coding, testing, scripting, and documentation conventions in [`docs/DEVELOPER.md`](docs/DEVELOPER.md).

## Required Verification

- Add or update automated tests for every behavior change and regression fix.
- Run the relevant tests and build, then run `.\scripts\Collect-CodeCoverage.ps1` before calling a feature done. The script clears prior `TestResults`; preserve any needed non-generated content first. Inspect `TestResults\CoverageReport\index.html`: every non-generated method with executable lines must have at least 90% line coverage; do not rely on an aggregate project or class percentage.
- Leave zero cyclomatic-complexity warnings. Address warnings rather than suppressing them or accepting them as unrelated to a new change.
- For the project-specific test strategy and commands, see [`docs/TESTING-APPROACH.md`](docs/TESTING-APPROACH.md).

## Example Project and Package Validation

Source-generator behavior must be validated through the packaged consumer workflow, not only project-reference unit tests. Run `.\scripts\Test-PreReleasePackage.ps1` for end-to-end validation against `tests\Example` when changing generator behavior or its package output.

That script changes the Example project's package reference and creates `local-packages`. Before committing:

1. If the Example project was modified only for pre-release testing, run `.\scripts\Undo-PreReleasePackage.ps1` to restore its package reference and remove the local package feed. Check `git status` first so existing user changes are not overwritten.
2. Remove any temporary Example feature, template, or test changes used to validate unreleased functionality. Do not commit pre-release package references or unreleased Example tests.
3. Inspect the final diff and confirm no temporary package artifacts or test-injection changes remain.

After the feature version is published to NuGet, add or update the Example project to demonstrate the released functionality and commit that as a separate follow-up change. See [`docs/RELEASE-WORKFLOW.md`](docs/RELEASE-WORKFLOW.md).
