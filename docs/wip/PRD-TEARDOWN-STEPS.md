---
Status: Approved
---

# Product Requirements Document: Specify tear-down steps for features

## Problem Statement

Features are implemented as a test class. Sometimes we need bring-up and
tear-down steps to put the test into the correct state, and to clean up
after the test.

Currently, bring-up steps can be accomplished by creating a `Background`
section. However, there is no Gherkin-native approach to specifying tear-
down steps.

## Goals

- Specify tear-down steps to be completed after each scenario in a feature is run
- Feature files remain valid Gherkin syntax

## Non-goals

- One-time setup or tear-down which happens only once per feature is out of scope for this PRD.

## Solution: `And afterward` keyword in Background

Tear-down steps are specified within the `Background` section using the
`And afterward` prefix. Steps beginning with "afterward" are routed to a
`[TearDown]` method in the generated test class, while all other steps
remain in the `[SetUp]` method as they do today.

### Syntax

```gherkin
Background:
  Given the application is running
  And I am logged in as customer
  And afterward the shopping cart is cleared
  And afterward the session is reset
```

This keeps setup and teardown together in one place, reads naturally as
English, and is valid Gherkin syntax (the parser treats "afterward" as
part of the step text).

### Design Notes

- **Case insensitive**: The "afterward" prefix is matched case-insensitively.
  `And afterward`, `And Afterward`, and `And AFTERWARD` are all recognized.
- **Any step keyword**: The "afterward" prefix is recognized on any step
  keyword (`Given`, `When`, `Then`, `And`, `But`, `*`), not just `And`.
  While `And afterward` is the idiomatic usage, other keywords work to
  avoid brittleness.
- **Base class `[TearDown]` interaction**: If the test base class already
  has a `[TearDown]` method, both the base class and generated teardown
  will run according to standard NUnit inheritance behavior (derived class
  `[TearDown]` runs first, then base class). This project has no opinion
  on the interaction; it follows NUnit conventions.
- **Documentation**: The User Guide should be updated after this feature
  is implemented to document the `And afterward` syntax and the
  `[Afterward]` attribute.
- **Samples**: The sample tests should be updated to include demonstrating use of this feature.

### Step Matching with `[Afterward]` Attribute

Tear-down steps match exclusively against step definitions decorated with
the `[Afterward]` attribute. The "afterward" prefix is stripped from the
step text before matching, so `And afterward the shopping cart is cleared`
matches against `[Afterward("the shopping cart is cleared")]`.

```csharp
public class ShoppingCartSteps
{
    [Afterward("the shopping cart is cleared")]
    public async Task TheShoppingCartIsCleared()
    {
        _context.Cart.Clear();
    }
}
```

The `[Afterward]` attribute creates a dedicated keyword category for
teardown steps, keeping them separate from `[Given]`/`[When]`/`[Then]`
matching. This prevents accidental collisions where a setup step and a
teardown step share the same text but have different implementations.

A step definition can be made available in multiple contexts by applying
multiple attributes, following the same pattern already used for sharing
steps across `[Given]` and `[When]`:

```csharp
public class ShoppingCartSteps
{
    [Given("the shopping cart is empty")]
    [Afterward("the shopping cart is cleared")]
    public async Task TheShoppingCartIsCleared()
    {
        _context.Cart.Clear();
    }
}
```

### Generated Output

The Background section generates two test lifecycle methods:

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
    // And afterward the shopping cart is cleared
    await ShoppingCartSteps.TheShoppingCartIsCleared();

    // And afterward the session is reset
    await SessionSteps.TheSessionIsReset();
}
```

### Example: Complete Feature File

```gherkin
Feature: Shopping Cart
  As a customer
  I want to add items to my cart
  So that I can purchase multiple items

Background:
  Given the application is running
  And I am logged in as customer
  And afterward the shopping cart is cleared

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

## User Stories

### Story 1: User - Specifies tear-down steps

**As a** developer creating tests in my project
**I want** to specify tear-down steps in my feature file's Background section
**So that** my environment is returned to a clean state after each scenario

**Acceptance Criteria**:
- [ ] Steps in Background prefixed with "afterward" are generated into a `[TearDown]` method
- [ ] The "afterward" prefix is stripped before step matching
- [ ] "Afterward" steps match exclusively against `[Afterward]` step definition attributes
- [ ] Non-"afterward" Background steps continue to generate into the `[SetUp]` method as before
- [ ] Tear-down steps support the same step matching as setup steps (including parameterized steps and data tables)
- [ ] Feature files with "afterward" steps remain valid Gherkin and parse without errors
- [ ] Features without "afterward" steps continue to work unchanged (no `[TearDown]` method generated)
- [ ] A step definition can be shared across contexts by applying both `[Given]` and `[Afterward]` attributes

### Story 2: User - Gets stubs for unimplemented tear-down steps

**As a** developer writing a feature file
**I want** unimplemented `And afterward` steps to generate stub methods with `[Afterward]` attributes
**So that** I can copy them into my step definition classes and implement them

When an `And afterward` step has no matching `[Afterward]` step definition,
the generator should produce a stub method in the generated file, following
the same pattern used for unimplemented `[Given]`/`[When]`/`[Then]` steps
today.

**Example**: Given this feature file:

```gherkin
Background:
  Given the application is running
  And afterward the shopping cart is cleared
```

If no `[Afterward("the shopping cart is cleared")]` step definition exists,
the generated file should include:

```csharp
#region Stubs for Unimplemented Steps

/// <summary>
/// Afterward the shopping cart is cleared
/// </summary>
[Afterward("the shopping cart is cleared")]
public async Task TheShoppingCartIsCleared()
{
    throw new NotImplementedException();
}

#endregion
```

The developer copies this stub into a step definition class, implements
the body, and rebuilds. On the next build, the generator matches the step
and the stub is no longer produced.

**Acceptance Criteria**:
- [ ] Unimplemented "afterward" steps generate stub methods with `[Afterward]` attribute
- [ ] Stub methods follow the same format as existing unimplemented step stubs (summary comment, attribute, `throw new NotImplementedException()`)
- [ ] The step text in the `[Afterward]` attribute has the "afterward" prefix stripped
- [ ] Stubs support parameterized step text (placeholders produce method parameters)
- [ ] Stubs support data table parameters
- [ ] The `[TearDown]` method still calls the stub via `this.MethodName()` (same as unimplemented setup steps use `this`)
- [ ] Once the step is implemented in a step class, the stub is no longer generated

### Story 3: User - Gets warning for `And afterward` in scenarios

**As a** developer writing a feature file
**I want** to be warned if I use `And afterward` inside a scenario
**So that** I know to move teardown steps to the Background section

The `And afterward` prefix is only meaningful in Background sections. If
a developer writes `And afterward` inside a Scenario, the generator should
emit a compiler warning and skip the step entirely (no generated code for
that step).

**Example**: Given this feature file:

```gherkin
Scenario: Add item to cart
  Given the application is running
  When I add "Widget" to the cart
  Then the cart should contain 1 item
  And afterward the shopping cart is cleared
```

The generator should:
- Emit a warning: `"afterward" steps are only supported in Background sections`
- Not generate any code for the `And afterward` step in the test method

**Acceptance Criteria**:
- [ ] `And afterward` steps appearing in a Scenario emit a compiler warning
- [ ] The warning message clearly states that "afterward" is only supported in Background sections
- [ ] No code is generated for the `And afterward` step in the scenario's test method
- [ ] The rest of the scenario's steps are generated normally
- [ ] The warning does not block compilation (it is a warning, not an error)

## Alternatives Considered

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
Requires a new `[After]` attribute. Teardown intent is not visible as
steps in the feature file.

### `[After]` attribute with implicit binding

Step classes with `[After]` methods automatically contribute teardown for
any feature that uses their steps, following the SpecFlow/Cucumber model.

**Rejected because**: Implicit binding is surprising — adding a new step
class to a feature silently adds teardown. No feature-file visibility of
teardown behavior. Ordering across multiple step classes is ambiguous.

### Matching `And afterward` against `[Given]` instead of `[Afterward]`

An earlier version of this proposal matched "afterward" steps against
`[Given]` step definitions (since `And` in a Background normalizes to
`Given`).

**Rejected because**: No semantic distinction between setup and teardown
step definitions. A developer reading `[Given("the shopping cart is
cleared")]` cannot tell it is used for teardown. Risks accidental reuse
of a setup step in a teardown context with different intent.
