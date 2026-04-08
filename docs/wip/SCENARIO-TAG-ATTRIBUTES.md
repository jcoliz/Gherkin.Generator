---
status: Implementation in progress
---

# Scenario Tag Attributes Design

Support NUnit test attributes via Gherkin scenario tags using the `@prefix:value` convention.

## Scope

Three new scenario-level tags that map to NUnit attributes:

| Gherkin Tag | NUnit Attribute | Value Type |
|---|---|---|
| `@category:name` | `[Category("name")]` | string |
| `@order:n` | `[Order(n)]` | integer |
| `@retry:n` | `[Retry(n)]` | integer |

All tags are **scenario-level only**. Feature-level support is out of scope for this iteration.

## Gherkin Usage

```gherkin
Feature: Transaction Management

Rule: Transaction Creation

@category:smoke @order:1
Scenario: Create a new transaction
  Given I am logged in
  When I create a transaction
  Then the transaction is saved

@category:smoke @category:regression @order:2
Scenario: Verify transaction in list
  Given I am logged in
  When I view the transactions list
  Then I should see the transaction

@retry:3
Scenario: Flaky external service call
  Given the external service is available
  When I sync transactions
  Then the sync should complete
```

## Generated Output

```csharp
[Category("smoke")]
[Order(1)]
[Test]
public async Task CreateANewTransaction()
{
    // ...
}

[Category("smoke")]
[Category("regression")]
[Order(2)]
[Test]
public async Task VerifyTransactionInList()
{
    // ...
}

[Retry(3)]
[Test]
public async Task FlakyExternalServiceCall()
{
    // ...
}
```

## Attribute Ordering in Generated Code

Attributes are generated in a consistent order above each test method:

1. `[Explicit]` - if present
2. `[Category]` - one per category, alphabetical
3. `[Order]` - if present
4. `[Retry]` - if present
5. `[TestCase]` - if scenario outline
6. `[Test]`

## Design Details

### CRIF Model Changes

Add new properties to `ScenarioCrif` in [`CrifModels.cs`](../../src/Lib/CrifModels.cs):

```csharp
public class ScenarioCrif
{
    // ... existing properties ...

    /// <summary>
    /// List of NUnit category names for this scenario.
    /// </summary>
    /// <remarks>
    /// Set by @category:name tags on the scenario. Multiple categories are supported.
    /// Uses a simple List of string so Mustache can render with {{.}} directly.
    /// </remarks>
    public List<string> Categories { get; set; } = [];

    /// <summary>
    /// Optional execution order for this scenario.
    /// </summary>
    /// <remarks>
    /// Set by @order:n tag on the scenario. Ordered tests run before unordered tests.
    /// </remarks>
    public int? Order { get; set; }

    /// <summary>
    /// Whether this scenario has an Order attribute.
    /// </summary>
    /// <remarks>
    /// Needed for Mustache conditional rendering since Mustache cannot check for null integers.
    /// </remarks>
    public bool HasOrder => Order.HasValue;

    /// <summary>
    /// Optional retry count for this scenario.
    /// </summary>
    /// <remarks>
    /// Set by @retry:n tag on the scenario. NUnit will retry the test up to n times.
    /// </remarks>
    public int? RetryCount { get; set; }

    /// <summary>
    /// Whether this scenario has a Retry attribute.
    /// </summary>
    /// <remarks>
    /// Needed for Mustache conditional rendering since Mustache cannot check for null integers.
    /// </remarks>
    public bool HasRetry => RetryCount.HasValue;
}
```

No new supporting model needed. `Categories` is a `List<string>`, consistent with how `Usings`, `DescriptionLines`, and `Classes` work in the existing CRIF.

### Tag Processing Changes

Add a new method `ProcessScenarioTags` in [`GherkinToCrifConverter.cs`](../../src/Lib/GherkinToCrifConverter.cs), called from `ConvertScenario`. This extracts from the existing `ProcessExplicitTag` and adds the three new tag processors:

```
ConvertScenario
  └── ProcessScenarioTags  (new, replaces ProcessExplicitTag call)
        ├── ProcessExplicitTag  (existing, moved here)
        ├── ProcessCategoryTags  (new)
        ├── ProcessOrderTag  (new)
        └── ProcessRetryTag  (new)
```

Integer parsing for `@order:n` and `@retry:n` should use `int.TryParse`. Invalid values are silently ignored -- matching the existing behavior where unrecognized tags are silently skipped.

### Template Changes

Update [`Default.mustache`](../../templates/Default.mustache) to render the new attributes. The scenario section currently looks like:

```mustache
{{#IsExplicit}}
[Explicit{{#ExplicitReason}}("{{.}}"){{/ExplicitReason}}]
{{/IsExplicit}}
{{#TestCases}}
[TestCase({{{.}}})]
{{/TestCases}}
[Test]
```

Updated to:

```mustache
{{#IsExplicit}}
[Explicit{{#ExplicitReason}}("{{.}}"){{/ExplicitReason}}]
{{/IsExplicit}}
{{#Categories}}
[Category("{{.}}")]
{{/Categories}}
{{#HasOrder}}
[Order({{Order}})]
{{/HasOrder}}
{{#HasRetry}}
[Retry({{RetryCount}})]
{{/HasRetry}}
{{#TestCases}}
[TestCase({{{.}}})]
{{/TestCases}}
[Test]
```

### Multiple Categories

A scenario can have multiple `@category:x` tags. Each generates a separate `[Category]` attribute:

```gherkin
@category:smoke @category:regression
Scenario: Important test
```

Generates:

```csharp
[Category("smoke")]
[Category("regression")]
[Test]
```

### Error Handling

| Condition | Behavior |
|---|---|
| `@order:abc` - non-integer value | Silently ignored, no Order attribute generated |
| `@retry:0` - zero value | Passed through as-is, NUnit handles validation |
| `@retry:-1` - negative value | Passed through as-is, NUnit handles validation |
| `@category:` - empty value | Silently ignored, no Category attribute generated |
| `@order:1 @order:2` - duplicate tag | Last value wins |
| `@retry:3 @retry:5` - duplicate tag | Last value wins |

## Implementation Plan

### Step 1: CRIF Model

Add `CategoryCrif` class and new properties to `ScenarioCrif` in [`CrifModels.cs`](../../src/Lib/CrifModels.cs).

### Step 2: Tag Processing

Refactor `ConvertScenario` in [`GherkinToCrifConverter.cs`](../../src/Lib/GherkinToCrifConverter.cs) to call a unified `ProcessScenarioTags` method. Add `ProcessCategoryTags`, `ProcessOrderTag`, and `ProcessRetryTag`.

### Step 3: Template Update

Update [`Default.mustache`](../../templates/Default.mustache) to render `[Category]`, `[Order]`, and `[Retry]` attributes.

### Step 4: Unit Tests - CRIF Conversion

Add tests in [`GherkinToCrifConverterTests.cs`](../../tests/Unit/GherkinToCrifConverterTests.cs):

- Scenario with single `@category:name` tag
- Scenario with multiple `@category` tags
- Scenario with `@order:n` tag
- Scenario with `@retry:n` tag
- Scenario with all three tags combined
- `@order:abc` invalid value is ignored
- `@retry:abc` invalid value is ignored
- `@category:` empty value is ignored
- Duplicate `@order` tags, last wins
- Tags combined with `@explicit`

### Step 5: Unit Tests - Code Generation

Add tests in [`FunctionalTestGeneratorTests.cs`](../../tests/Unit/FunctionalTestGeneratorTests.cs):

- Generated output contains `[Category("name")]`
- Generated output contains `[Order(n)]`
- Generated output contains `[Retry(n)]`
- Correct attribute ordering in output
- Multiple categories generate multiple attributes

### Step 6: Documentation

Update [`USER-GUIDE.md`](../../docs/USER-GUIDE.md):

- Add new tags to the Feature Tags section
- Add examples showing category filtering with `dotnet test --filter`
- Add examples showing order and retry usage

### Step 7: Example Feature

Update [`ShoppingCart.feature`](../../tests/Example/Features/ShoppingCart.feature) to demonstrate at least one of the new tags.
