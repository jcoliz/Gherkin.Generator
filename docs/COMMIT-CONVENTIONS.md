# Commit Conventions

This project follows a structured commit message format to maintain a clear and readable git history. Following these conventions helps with automated changelog generation, easier code reviews, and better collaboration.

## Format

All commit messages should follow this structure:

```
<type>(<scope>): <subject>

[optional body]

[optional footer]
```

Total commit size should stay under 50 words for most commits, but **always** under 100 words even for the most complex commit.

## Types

Use one of the following types to categorize your commit:

- **feat**: A new feature for the user 
- **fix**: A bug fix in production code
- **docs**: Documentation changes only
- **style**: Code style changes (formatting, missing semicolons, etc.) with no logic changes
- **refactor**: Code changes that neither fix a bug nor add a feature
- **perf**: Performance improvements
- **test**: Adding, updating, fixing, or refactoring tests (use this for all test-related changes)
- **build**: Changes to build system, dependencies, or project configuration (e.g., NuGet packages, npm dependencies, .csproj files)
- **revert**: Reverts a previous commit

**Note**: Use `test` type for all test changes, including new tests, fixing broken tests, and refactoring test code. The scope (unit/functional/integration) indicates which type of test.

## Scopes

Use these project-specific scopes to identify the area of change:

### Project Scopes

Scope should correspond to the project where the majority of the change was made. For commits which cross scope, use the scope closest to the user.

- **analyzer**: The Gherkin.Generator analyzer project
- **lib**: Gherkin.Generator.Lib
- **utils**: Gherkin.Generator.Utils
- **tool**: Gherkin.Generator.Tool
- **tests(unit)**: Unit tests
- **tests(example)**: Example tests 
- **templates**: Sample mustache templates
- **scripts**: PowerShell automation scripts
- **ci**: Changes to CI/CD configuration files and scripts, including release deployment
- **deps**: Dependency build changes

## Subject Line

The subject line should:

- Use imperative mood ("add" not "added" or "adds")
- Not capitalize the first letter (makes grep easier)
- Not end with a period
- Be limited to 72 characters
- Be concise but descriptive

### Examples

✅ **Good**:
```
feat(lib): add gherkin step parser for scenario outlines
fix(analyzer): resolve null reference in code generation
docs(user-guide): move mustache template section
feat(lib): add support for background steps
feat(utils): implement DataTable parsing logic
refactor(analyzer): simplify syntax tree traversal
```

❌ **Bad**:
```
Added new feature.
Fixed bug
Update files
```

## Body (Optional)

Include a body when the commit needs additional explanation:

- Separate from subject with a blank line
- Wrap lines at 72 characters
- Explain **what** and **why**, not **how**
- Use bullet points for multiple items

### Example

```
refactor(lib): simplify gherkin parser configuration

- Extract template path resolution to separate method
- Remove unused parsing options
- Add XML documentation for public members

This improves testability and makes the parser easier to maintain.
```

## Footer (Optional)

Use the footer for:

### Breaking Changes

Prefix with `BREAKING CHANGE:` followed by a description:

```
feat(lib)!: redesign gherkin parser interface

BREAKING CHANGE: GherkinParser.Parse() now returns Result<GherkinDocument>
instead of GherkinDocument. Update all callers to handle the Result pattern.
```

Note: The `!` after the type/scope is a visual indicator of a breaking change.

### Issue References

Reference issues that this commit addresses:

```
fix(analyzer): correct validation logic for scenario steps

Fixes #123
Closes #456
```

### Co-authors

Credit co-authors when pair programming:

```
feat(lib): implement scenario outline example expansion

Co-authored-by: Jane Doe <jane@example.com>
```

## Complete Examples

### Simple Feature

```
feat(lib): add gherkin feature file parser
```

### Bug Fix with Details

```
fix(analyzer): prevent duplicate code generation for steps

Check for existing generated methods before creating new ones
to avoid compilation errors in generated test files.

Fixes #78
```

### Test Changes

```
test(unit): add validation tests for gherkin step parser
test(example): fix flaky template generation test
test(unit): refactor test fixture setup for better performance
```

### Refactoring with Multiple Changes

```
refactor(lib): restructure gherkin parser organization

- Move validation logic to separate validator class
- Extract AST transformation to mapper
- Improve error handling with Result pattern
- Add comprehensive unit tests

This improves code maintainability and testability while
maintaining the same external API.
```

### Documentation Update

```
docs(readme): update installation instructions for .NET 9
```

### Infrastructure Change

```
build(ci): add automated nuget deployment workflow

Implements continuous deployment to NuGet on release creation.
Includes version synchronization for analyzer and utils packages.
```

## Best Practices

1. **Make atomic commits**: Each commit should represent a single logical change
2. **Commit early and often**: Don't wait until you have a massive changeset
3. **Write meaningful messages**: Future you (and your team) will thank you
4. **Use the body**: Don't be afraid to explain the context and reasoning
5. **Reference issues**: Link commits to issue tracking for better traceability
6. **Review before pushing**: Use `git log` to review your commit messages

## Tools

In the future, we will consider using these tools to enforce commit conventions:

- **[Commitizen](https://github.com/commitizen/cz-cli)**: Interactive commit message builder
- **[commitlint](https://commitlint.js.org/)**: Lint commit messages
- **[Husky](https://typicode.github.io/husky/)**: Git hooks to enforce conventions

## Resources

- [Conventional Commits Specification](https://www.conventionalcommits.org/)
- [Angular Commit Guidelines](https://github.com/angular/angular/blob/main/CONTRIBUTING.md#commit)
- [How to Write a Git Commit Message](https://chris.beams.io/posts/git-commit/)
