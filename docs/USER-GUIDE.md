# User Guide

Complete guide for using Gherkin.Generator to create behavior-driven tests with automatic code generation.

## Table of Contents

- [Getting Started](#getting-started)
- [Writing Feature Files](#writing-feature-files)
- [Authoring Step Definitions](#authoring-step-definitions)
- [Customizing Templates](#customizing-templates)
- [Advanced Features](#advanced-features)
- [Troubleshooting](#troubleshooting)

## Getting Started

### Installation

Add the package to your test project:

```xml
<PackageReference Include="Gherkin.Generator" Version="0.0.1" />
```

### Basic Setup

1. **Create a Features directory** in your test project
2. **Copy the template** from [`templates/Default.mustache`](../templates/Default.mustache)
3. **Configure your project** to include feature files:

```xml
<ItemGroup>
  <AdditionalFiles Include="Features\*.feature" />
  <AdditionalFiles Include="Templates\Default.mustache" />
</ItemGroup>
```

### Retaining Generated Files

**Important:** Configure your project to emit generated files to a visible location for debugging.

Add to your `.csproj`:

```xml
<PropertyGroup>
  <EmitCompilerGeneratedFiles>true</EmitCompilerGeneratedFiles>
  <CompilerGeneratedFilesOutputPath>obj\GeneratedFiles</CompilerGeneratedFilesOutputPath>
</PropertyGroup>
```

This creates an `obj/GeneratedFiles/` directory with all generated test code. Benefits:

- **Debugging** - See exactly what code is generated from your features
- **Troubleshooting** - Identify compilation errors and step matching issues
- **Learning** - Understand how Gherkin maps to C# test methods
- **Inspect unimplemented stubs** - See which steps need implementation

The `obj/` directory is typically in `.gitignore`, which is appropriate since generated files should be regenerated on each build. This keeps your repository clean while making generated code easily accessible for debugging.

**Pro tip:** Keep these files open in your editor while authoring feature files to see real-time code generation results.

## Writing Feature Files

Feature files use Gherkin syntax to describe behavior in plain language.

### Minimal Feature

```gherkin
Feature: User Login

Scenario: Successful login
  Given the user is on the login page
  When the user enters valid credentials
  Then the user should see the dashboard
```

### Feature with Description

Add user stories or acceptance criteria:

```gherkin
Feature: User Login
  As a registered user
  I want to log in to my account
  So that I can access my dashboard

Scenario: Successful login
  Given the user is on the login page
  When the user enters valid credentials
  Then the user should see the dashboard
```

### Organizing with Rules

Group related scenarios:

```gherkin
Feature: Transaction Management

Rule: Transaction Creation

Scenario: Create transaction with valid data
  Given I am logged in
  When I create a transaction
  Then the transaction is saved

Scenario: Cannot create transaction with missing fields
  Given I am logged in
  When I create a transaction without a payee
  Then I should see a validation error
```

### Background Steps

Setup steps that run before each scenario:

```gherkin
Feature: Transaction Management

Background:
  Given the application is running
  And I am logged in as admin

Scenario: Create transaction
  When I create a transaction
  Then the transaction is saved
```

### Data Tables

Pass structured data to steps:

```gherkin
Scenario: Transaction with multiple fields
  Given a transaction with the following fields:
    | Field  | Value        |
    | Date   | 2024-01-15   |
    | Payee  | Coffee Shop  |
    | Amount | 4.50         |
  When I save the transaction
  Then the transaction is stored
```

### Scenario Outlines

Run the same scenario with multiple values:

```gherkin
Scenario Outline: Navigate to different pages
  Given the application is running
  When I navigate to <page>
  Then I should see the <title> heading

Examples:
  | page      | title     |
  | /weather  | Weather   |
  | /counter  | Counter   |
  | /about    | About Us  |
```

### Feature Tags

Configure namespace, base class, and imports:

```gherkin
@namespace:MyApp.Tests.Functional.Features
@baseclass:FunctionalTestBase
@using:System.Collections.Generic
Feature: User Login
```

**Common tags:**
- `@namespace:` - Set the namespace for generated test class
- `@baseclass:` - Set the base class (supports fully-qualified names)
- `@using:` - Add using directives (can use multiple times)
- `@explicit` - Mark scenario as explicit (requires manual execution)

## Authoring Step Definitions

Step definitions are C# methods that implement the behavior described in feature files.

### Basic Step Definition

Use attributes to mark methods as step definitions:

```csharp
public class NavigationSteps
{
    [Given("the application is running")]
    public async Task GivenTheApplicationIsRunning()
    {
        // Implementation
    }
}
```

**Available attributes:** `[Given]`, `[When]`, `[Then]`

### Step Parameters

Capture values from step text using placeholders:

```csharp
[When("I navigate to {page}")]
public async Task WhenINavigateTo(string page)
{
    await _client.GetAsync(page);
}
```

**Placeholder rules:**
- Matches single words: `Savings`
- Matches hyphenated words: `Ski-Village`
- Matches quoted phrases: `"Ski Village"`
- Does not match unquoted phrases with spaces

**Multiple parameters:**

```csharp
[Given("user {firstName} {lastName} exists")]
public async Task UserExists(string firstName, string lastName)
{
    // Implementation
}
```

### Data Tables

Accept structured data as parameters:

```csharp
[Given("a transaction with the following fields:")]
public async Task GivenTransactionWithFields(DataTable table)
{
    foreach (var row in table.Rows)
    {
        var field = row["Field"];
        var value = row["Value"];
        // Use field and value
    }
}
```

### Step Context

Step classes are constructed by the generated test code. The default template constructs them with `this`:

```csharp
// Generated code in test class
protected NavigationSteps NavigationSteps => _theNavigationSteps ??= new(this);
private NavigationSteps? _theNavigationSteps;
```

This passes the test instance to step classes, allowing them to access shared state:

```csharp
public class NavigationSteps
{
    private readonly FunctionalTestBase _context;

    public NavigationSteps(FunctionalTestBase context)
    {
        _context = context;
    }

    [Given("the application is running")]
    public async Task GivenTheApplicationIsRunning()
    {
        await _context.LaunchApplication();
    }
}
```

### Recommended Pattern: Interface-Based Context

For better separation, define an interface for test capabilities:

```csharp
// Infrastructure.TestBase.cs
public interface ITestCapabilities
{
    HttpClient HttpClient { get; }
    Task LaunchApplicationAsync();
    // Other shared capabilities
}

public class FunctionalTestBase : ITestCapabilities
{
    public HttpClient HttpClient { get; private set; }
    
    public async Task LaunchApplicationAsync()
    {
        // Implementation
    }
}
```

Step classes depend on the interface, not the concrete base class:

```csharp
public class NavigationSteps
{
    private readonly ITestCapabilities _context;

    public NavigationSteps(ITestCapabilities context)
    {
        _context = context;
    }

    [Given("the application is running")]
    public async Task GivenTheApplicationIsRunning()
    {
        await _context.LaunchApplicationAsync();
    }
}
```

This approach works with the default template since `FunctionalTestBase` implements `ITestCapabilities`. Benefits:

- **Testability** - Step classes can be unit tested with mocked capabilities
- **Flexibility** - Multiple test base classes can implement the same interface
- **Clarity** - Interface explicitly declares what capabilities steps need

You can modify the template if you need different construction logic.

### Organizing Step Classes

Group related steps into classes:

```
Steps/
  ├── NavigationSteps.cs
  ├── AuthenticationSteps.cs
  └── TransactionSteps.cs
```

The generator discovers all step definitions and matches them to feature steps automatically.

## Customizing Templates

Templates use Mustache syntax to control code generation.

### Template Structure

The default template generates NUnit tests:

```mustache
namespace {{Namespace}};

public partial class {{FileName}}_Feature_Tests : {{BaseClass}}
{
    {{#Scenarios}}
    [Test]
    public async Task {{Method}}()
    {
        {{#Steps}}
        await {{Owner}}.{{Method}}();
        {{/Steps}}
    }
    {{/Scenarios}}
}
```

### Available Template Variables

**Feature-level:**
- `Namespace` - Test class namespace
- `FileName` - Feature file name (without extension)
- `FeatureName` - Human-readable feature name
- `BaseClass` - Test fixture base class
- `Usings` - List of using directives

**Scenario-level:**
- `Name` - Scenario name
- `Method` - PascalCase method name
- `ExplicitTag` - Boolean for [Explicit] attribute
- `TestCases` - List of test case parameters (scenario outlines)
- `Parameters` - Method parameters

**Step-level:**
- `Keyword` - Given/When/Then/And/But
- `Text` - Step description
- `Owner` - Step class instance
- `Method` - Step method name
- `Arguments` - Method arguments

### Creating Custom Templates

Copy [`Default.mustache`](../templates/Default.mustache) and modify for your framework:

```mustache
// xUnit example
namespace {{Namespace}};

public class {{FileName}}_Tests : {{BaseClass}}
{
    {{#Scenarios}}
    [Fact]
    public async Task {{Method}}()
    {
        {{#Steps}}
        await {{Owner}}.{{Method}}();
        {{/Steps}}
    }
    {{/Scenarios}}
}
```

## Advanced Features

### Unimplemented Steps

Steps without matching definitions generate stub methods:

```csharp
#region Unimplemented Steps

/// <summary>
/// Given I am logged in
/// </summary>
async Task IAmLoggedIn()
{
    throw new NotImplementedException();
}

#endregion
```

Implement these stubs in your step classes, then rebuild.

### Explicit Tests

Mark scenarios that require manual execution or special setup:

```gherkin
@explicit
Scenario: Performance test
  Given the application is running
  When I load 10000 transactions
  Then the page should load in under 2 seconds
```

Generates:

```csharp
[Explicit]
[Test]
public async Task PerformanceTest()
{
    // Implementation
}
```

### Scenario Descriptions

Add detailed documentation:

```gherkin
Scenario: Complex business rule
  This scenario validates the complex business rule
  that applies when certain conditions are met.

  Given certain conditions exist
  When an action occurs
  Then the expected result happens
```

### Multiple Example Tables

Scenario outlines support only one examples table per scenario. For multiple test cases, use separate scenarios or combine into one table.

## Troubleshooting

### Build Errors

**Error:** "No matching step definition"
- **Cause:** Step in feature file doesn't match any step method
- **Solution:** Check step text matches exactly (case-insensitive), or implement missing step

**Error:** "Duplicate step definitions"
- **Cause:** Multiple step methods match the same step text
- **Solution:** Make step patterns more specific or remove duplicates

### Step Matching Issues

**Problem:** Parameterized step not matching
- **Check:** Use placeholders `{name}` in step attribute
- **Check:** Parameter types match (string, int, etc.)
- **Check:** Quoted phrases for values with spaces

**Problem:** Data table not working
- **Check:** Method accepts `DataTable` parameter
- **Check:** Table has header row in feature file

### Generated Code Issues

**Problem:** Generated code doesn't compile
- **Check:** Namespace tags set correctly
- **Check:** Base class exists and is accessible
- **Check:** Using directives include required namespaces

**Problem:** Step methods not found at runtime
- **Check:** Step classes are instantiated properly
- **Check:** Constructor accepts correct context type

### Debugging Tips

1. **Check generated files** in `obj/GeneratedFiles/` (or your configured output path)
2. **Review generated test methods** to see exactly how steps map to C# code
3. **Look for unimplemented step stubs** in generated code - these identify missing step definitions
4. **Use verbose build output:** `dotnet build -v detailed`
5. **Test step matching** with unit tests (see examples in [`tests/Unit/`](../tests/Unit/))
6. **Compare with template** to understand how CRIF variables populate the Mustache template
7. **Keep generated files open** in your editor while authoring features for immediate feedback

### Getting Help

- Check example tests in [`tests/Unit/`](../tests/Unit/)
- Review sample CRIF in [`tests/Unit/SampleData/`](../tests/Unit/SampleData/)
- See template reference in [`templates/Default.mustache`](../templates/Default.mustache)

## Examples

### Complete Working Example

See [`tests/Example`](../tests/Example/) for a complete, working shopping cart example that demonstrates:

- **Feature file** with background, rules, scenarios, and scenario outlines
- **Step definitions** organized into multiple step classes
- **Simple implementation** of a shopping cart
- **Test base class** providing shared context
- **Generated test code** (retained for inspection in `obj/GeneratedFiles/`)

The example builds and all tests pass. It's a great starting point for understanding how all the pieces fit together.

**Key files:**
- [`Features/ShoppingCart.feature`](../tests/Example/Features/ShoppingCart.feature) - Complete Gherkin scenarios
- [`Steps/ShoppingCartSteps.cs`](../tests/Example/Steps/ShoppingCartSteps.cs) - Shopping cart step definitions
- [`Steps/ApplicationSteps.cs`](../tests/Example/Steps/ApplicationSteps.cs) - Application setup steps
- [`ShoppingCart.cs`](../tests/Example/ShoppingCart.cs) - Simple cart implementation
- [`FunctionalTestBase.cs`](../tests/Example/FunctionalTestBase.cs) - Test base class

To run the example:

```bash
cd tests/Example
dotnet build
dotnet test
```

## Next Steps

- Read [`DEVELOPER.md`](DEVELOPER.md) if contributing to this project
- Check [`COMMIT-CONVENTIONS.md`](COMMIT-CONVENTIONS.md) for commit guidelines
- See example implementations in the unit tests
- Customize the template for your test framework and conventions
