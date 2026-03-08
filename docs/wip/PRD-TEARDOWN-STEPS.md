---
status: Approved
supersedes: "Previous version used `And afterward` keyword approach (rejected as too complex)"
---

# Product Requirements Document: Specify tear-down steps for features

## Problem Statement

Features are implemented as a test class. Sometimes we need bring-up and
tear-down steps to put the test into the correct state, and to clean up
after the test.

Currently, bring-up steps can be accomplished by creating a `Background`
section. However, there is no mechanism for specifying tear-down steps
that run after each scenario.

## Goals

- Allow step classes to define tear-down logic that runs after each scenario
- Keep the implementation simple and low-effort
- Feature files remain valid Gherkin syntax (no changes to feature file format)

## Non-goals

- One-time setup or tear-down which happens only once per feature is out of scope for this PRD.
- Explicit visibility of teardown steps in the feature file is not a goal. Teardown is an implementation concern managed by step writers.

## Solution: `[After]` attribute with implicit binding

Step classes define methods decorated with `[After]` that automatically
contribute teardown for any feature that uses their steps. When a
generated test class references a step class (because its `[Given]`,
`[When]`, or `[Then]` steps were matched), and that step class contains
`[After]` methods, those methods are called in the generated `[TearDown]`
method.

This follows the SpecFlow/Cucumber model where teardown is bound
implicitly to the step classes that are already in use. The test reader
doesn't need to understand teardown details. The step writer handles
cleanup as part of the step class contract.

### Syntax

No changes to feature files. Teardown is specified entirely in step
class code:

```csharp
public class ShoppingCartSteps
{
    [Given("the cart contains:")]
    public async Task CartContains(DataTable table) { /* ... */ }

    [When("I add {item} to the cart")]
    public async Task AddToCart(string item) { /* ... */ }

    [After]
    public async Task ClearCart()
    {
        _context.Cart.Clear();
    }
}
```

Any feature that matches steps from `ShoppingCartSteps` will
automatically call `ClearCart()` in its `[TearDown]` method.

### Design Notes

- **No feature file changes**: The feature file remains unchanged. This
  is purely a step-class-side concern.
- **Implicit binding**: If a step class is referenced by a feature (i.e.,
  any of its `[Given]`/`[When]`/`[Then]` steps are matched), its `[After]`
  methods are included in teardown. If the step class is not used by a
  feature, its `[After]` methods are not included.
- **Multiple `[After]` methods**: A step class may have multiple `[After]`
  methods. All are called during teardown.
- **Ordering within a class**: Multiple `[After]` methods within the same
  step class are called in declaration order.
- **Ordering across classes**: When multiple step classes contribute
  `[After]` methods, the order follows the order in which step classes
  appear in the generated class references (i.e., the order their steps
  were first matched in the feature file).
- **No parameters**: `[After]` methods must be parameterless. The
  analyzer should emit a warning if an `[After]` method has parameters.
- **Base class `[TearDown]` interaction**: If the test base class already
  has a `[TearDown]` method, both the base class and generated teardown
  will run according to standard NUnit inheritance behavior (derived class
  `[TearDown]` runs first, then base class). This project has no opinion
  on the interaction; it follows NUnit conventions.
- **No teardown without steps**: A step class that contains only `[After]`
  methods but no `[Given]`/`[When]`/`[Then]` methods will never be
  referenced by any feature, so its `[After]` methods will never run.
  This is by design, as teardown is tied to step class usage.
- **Documentation**: The User Guide should be updated after this feature
  is implemented to document the `[After]` attribute.
- **Samples**: The sample tests should be updated to include demonstrating
  use of this feature.

### Step Metadata

The `[After]` attribute is discovered during the same step metadata
extraction pass that discovers `[Given]`/`[When]`/`[Then]` attributes.
The metadata for `[After]` methods is stored alongside the existing step
metadata, associated with the step class.

The `[After]` attribute does not participate in step matching. It is not
a keyword in the `NormalizedKeyword` enum. Instead, the generator checks
whether any referenced step class has `[After]` methods, and if so,
generates a `[TearDown]` method that calls them.

### Generated Output

When a feature uses step classes that have `[After]` methods, the
generated test class includes a `[TearDown]` method:

```csharp
[SetUp]
public async Task SetupAsync()
{
    // Given the application is running
    await ApplicationSteps.TheApplicationIsRunning();

    // And I am logged in as customer
    await AuthSteps.IAmLoggedInAsCustomer();
}

[TearDown]
public async Task TeardownAsync()
{
    await ShoppingCartSteps.ClearCart();
}
```

If no referenced step classes have `[After]` methods, no `[TearDown]`
method is generated.

### Example: Complete Feature File and Step Classes

Feature file (unchanged from today's format):

```gherkin
Feature: Shopping Cart
  As a customer
  I want to add items to my cart
  So that I can purchase multiple items

Background:
  Given the application is running
  And I am logged in as customer

Rule: Adding Items

Scenario: Add single item to empty cart
  When I add "Widget" to the cart
  Then the cart should contain 1 item
  And the cart total should be 9.99

Scenario: Add multiple items
  When I add "Widget" to the cart
  And I add "Gadget" to the cart
  Then the cart should contain 2 items
```

Step class with teardown:

```csharp
public class ShoppingCartSteps
{
    private readonly FunctionalTestBase _context;

    public ShoppingCartSteps(FunctionalTestBase context)
    {
        _context = context;
    }

    [When("I add {item} to the cart")]
    public async Task AddToCart(string item)
    {
        _context.Cart.AddItem(item);
    }

    [Then("the cart should contain {quantity} item")]
    [Then("the cart should contain {quantity} items")]
    public async Task CartShouldContainItems(string quantity)
    {
        Assert.That(_context.Cart.ItemCount, Is.EqualTo(int.Parse(quantity)));
    }

    [After]
    public async Task ClearCart()
    {
        _context.Cart.Clear();
    }
}
```

## User Stories

### Story 1: User - Creates `[After]` attribute

**As a** developer using Gherkin.Generator
**I want** an `[After]` attribute available in the Utils package
**So that** I can mark step methods as teardown logic

**Acceptance Criteria**:
- [ ] `[After]` attribute is added to the `Gherkin.Generator.Utils` package
- [ ] `[After]` attribute has `AllowMultiple = false` (a method only needs one)
- [ ] `[After]` attribute takes no parameters (unlike `[Given]`/`[When]`/`[Then]` which take a pattern)
- [ ] `[After]` attribute targets methods only

### Story 2: User - Tear-down methods are discovered and generated

**As a** developer using Gherkin.Generator
**I want** `[After]` methods from my step classes to be called in a generated `[TearDown]` method
**So that** my test environment is cleaned up after each scenario

**Acceptance Criteria**:
- [ ] Step metadata extraction discovers `[After]`-decorated methods alongside `[Given]`/`[When]`/`[Then]`
- [ ] When a step class is referenced by a feature (via matched steps), its `[After]` methods are included in teardown
- [ ] A `[TearDown]` method is generated that calls all `[After]` methods from referenced step classes
- [ ] `[After]` methods are called in declaration order within a class
- [ ] Cross-class ordering follows the order step classes were first referenced
- [ ] Features that don't reference any step classes with `[After]` methods do not generate a `[TearDown]` method
- [ ] The Mustache template is updated to support the optional `[TearDown]` section

### Story 3: User - Gets warning for invalid `[After]` methods

**As a** developer using Gherkin.Generator
**I want** to be warned if my `[After]` methods have parameters
**So that** I know to fix them since teardown methods cannot accept arguments

**Acceptance Criteria**:
- [ ] The analyzer emits a warning when an `[After]`-decorated method has parameters
- [ ] The warning message clearly states that `[After]` methods must be parameterless
- [ ] The warning does not block compilation (it is a warning, not an error)

## Complexity Assessment

This approach is significantly simpler than the previously-considered
`And afterward` keyword approach:

| Concern | `And afterward` approach | `[After]` implicit binding |
|---|---|---|
| Feature file syntax | New keyword pattern to parse | No changes |
| New attribute | `[Afterward]` with pattern text | `[After]` with no parameters |
| Step matching | New keyword category for teardown | No step matching needed |
| Stub generation | Must generate `[Afterward]` stubs | Not applicable |
| Warning for misuse | Must detect in scenarios | Only: parameters on `[After]` |
| CRIF model changes | New teardown step list | Only: `[After]` method list per class |
| Template changes | Teardown section with step calls | Teardown section with simple method calls |
| User stories | 3 stories, complex acceptance criteria | 3 stories, straightforward criteria |

The `[After]` approach requires changes only in the step metadata layer
and code generation template. No Gherkin parsing changes, no new step
matching logic, no stub generation for a new keyword category.

## Alternatives Considered

### `And afterward` keyword in Background

Tear-down steps specified within the `Background` section using the
`And afterward` prefix. Steps beginning with "afterward" are routed to a
`[TearDown]` method, while all other steps remain in the `[SetUp]` method.

**Rejected because**: Too complex for the resulting gain. Requires new
Gherkin keyword parsing, a new `[Afterward]` attribute with pattern
matching, new stub generation logic for unimplemented teardown steps,
and a new warning for misuse in scenarios. The amount of work is
disproportionate to the value delivered.

### `@teardown`-tagged Scenario

A dedicated scenario marked with `@teardown` whose steps route to `[TearDown]`:

```gherkin
@teardown
Scenario: Teardown
  Given the shopping cart is cleared
```

**Rejected because**: Creates a phantom "scenario" visible in test runners
and documentation tools. Separates setup and teardown into different
sections when they are conceptually related.

### `@after:ClassName` feature tag

A feature-level tag referencing step classes that have `[After]` methods:

```gherkin
@after:ShoppingCartSteps
Feature: Shopping Cart
```

**Rejected because**: Introduces a new binding mechanism (by class name)
that differs from the text-based step matching used throughout the system.
Teardown intent is not visible as steps in the feature file, and the tag
creates coupling to class names.

### Matching `And afterward` against `[Given]` instead of `[Afterward]`

An earlier version of the `And afterward` proposal matched "afterward"
steps against `[Given]` step definitions (since `And` in a Background
normalizes to `Given`).

**Rejected because**: No semantic distinction between setup and teardown
step definitions. A developer reading `[Given("the shopping cart is
cleared")]` cannot tell it is used for teardown. Risks accidental reuse
of a setup step in a teardown context with different intent.
