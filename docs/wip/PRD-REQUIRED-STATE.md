---
status: Implementation in progress # Draft | In Review | Approved | Implemented
design_document: [Link to design document]
issue: 25
---

# Product Requirements Document: Required State Annotations

## Problem Statement

The generator currently produces executable scenarios from Gherkin steps, but there is no explicit model for the flow of shared state between steps. In real-world test suites, one step often establishes a fact, object, or context value that a later step relies on. Today, that dependency is implicit and easy to miss.

This creates a few recurring problems:

- Step authors can write a valid scenario that compiles, yet fails during execution because a required object was never created.
- Shared-state requirements are invisible in generated test code, making it harder to review or debug a scenario.
- The generator cannot distinguish between a scenario that is incomplete and one that is intentionally relying on external setup.

This PRD introduces explicit, declarative state requirements so the generator can surface, validate, and warn about shared-state flow as part of the scenario design itself.

## Goals & Non-Goals

### Goals

- [ ] Allow step methods to declare what shared state they require before they can run.
- [ ] Allow step methods to declare what shared state they provide after they run.
- [ ] Model shared state as scenario-scoped, not global, so the requirement is evaluated within the current generated test flow.
- [ ] Surface state requirements and state creation directly in generated test bodies as comments and warnings.
- [ ] Allow the generator to warn when a required state item is not provided earlier in the same scenario path.
- [ ] Ensure the feature works with standard step ordering, including Background steps and consecutive scenario steps.

### Non-Goals

- [ ] This feature does not define the methodology for steps to share state. That is app-specific.
- [ ] This feature does not define a full dependency injection or runtime state framework.
- [ ] This feature does not require a separate global state registry or persistence layer across scenarios.
- [ ] This feature does not attempt to infer semantic correctness of the state description beyond explicit annotations.
- [ ] This feature does not change Gherkin syntax or require changes to the feature file format.
- [ ] This feature is not a replacement for ordinary compile-time validation or runtime assertions in step implementations.

---

## Definitions

### Shared State

Shared state is a named value or object created by one scenario step and consumed by a later step within the same scenario execution. Examples include a logged-in user, a seeded database row, an ephemeral token, or a reset password value.

### State Name

A state name is the unique identifier used in the attribute, such as DefaultUser or NewPassword. It is the contract that later steps reference. The name should be stable and meaningful to authors across the step suite.

### State Description

A state description is optional explanatory text that clarifies what the state represents and why it matters. It should be written in terms meaningful to a test author, not just an implementation detail.

### Scenario Scope

State is only considered within the current generated test and the steps that run before the current point in that scenario. Background steps are included in the same analysis history because they are part of the scenario setup flow.

---

## User Stories

### Story 1: Developer - Declare step state contracts
**As a** developer of an application writing tests to use this library
**I want** to annotate steps with what state they require and provide
**So that** the generator can expose shared-state flow in a reviewable and checkable way

**Acceptance Criteria**:
- [X] `[Provides]` annotations can be added to step definition methods.
- [X] `[Requires]` annotations can be added to step definition methods.
- [X] Multiple state entries can be declared on the same step method.
- [X] A state entry includes both a name and an optional human-readable description.
- [X] These attributes are declarative metadata attached to step methods for generator analysis. They are not feature-file annotations, test-framework infrastructure, or runtime execution directives.

### Story 2: Developer - Review state flow in generated tests
**As a** developer of an application writing tests to use this library
**I want** to easily review what each step requires and provides while reading generated test code
**So that** I can detect missing state before the test is run

**Acceptance Criteria**:
- [X] Generated tests include comments before each step listing the state it requires.
- [X] Generated tests include comments after each step listing the state it provides.
- [X] The comments use the same human-readable descriptions provided in the annotations.
- [X] The comments are generated in the same order the step metadata is discovered.
- [X] Background setup steps are included in the same analysis so their state contributions and dependencies are visible.

### Story 3: Developer - Receive warnings for missing state
**As a** developer of an application writing tests to use this library
**I want** to receive a warning when a step requires state that has not been provided earlier in the scenario
**So that** missing shared state is immediately visible during test generation or compilation

**Acceptance Criteria**:
- [ ] The generator can determine, for each step in order, whether required state has already been provided by an earlier step in the scenario.
- [ ] If required state is absent, the generated method emits a compiler warning before the step call.
- [ ] The warning includes the missing state name and its description.
- [ ] A step with no missing requirements emits no warning.
- [ ] The check includes Background steps as part of the scenario preconditions.

### Story 4: Developer - Maintain understandable step contracts in the step catalog
**As a** developer maintaining a library of steps
**I want** the shared-state contract to be explicit and reviewable in the source code
**So that** I can reason about step ordering and avoid hidden coupling

**Acceptance Criteria**:
- [ ] A state dependency can be identified by name without reading the full implementation body.
- [ ] Authors can document why a state matters, not just what it is called.
- [ ] The contract is visible from the step definition itself and can be inspected by code review tools.

---

## Functional Requirements

### Annotation Syntax

New attributes are created so authors can declare state requirements and provisions on step methods:

```cs
/// <summary>
/// Creates a default user for subsequent reset-password scenarios.
/// </summary>
[When("user changes their password on the profile page")]
[Provides("DefaultUser", "a UserDetails object containing the username, email, and password of the created test user account.")]
[Provides("NewPassword", "the new password submitted by the user on the Reset Page.")]
public async Task UserChangesTheirPasswordOnTheProfilePage()
```

```cs
/// <summary>
/// Attempts a reset using an account that should already exist.
/// </summary>
[When("user submits email and a weak password on Reset Page")]
[Requires("DefaultUser", "a UserDetails object containing the username, email, and password of the created test user account.")]
[Requires("NewPassword", "the password which the user has changed their password to.")]
public async Task UserSubmitsEmailAndWeakPasswordOnResetPage()
```

### Semantics

1. `[Requires]` declares a state item required before the step may execute.
2. `[Provides]` declares a state item produced after the step successfully completes.
3. A step may declare multiple `[Requires]` and `[Provides]` entries.
4. State names are compared by exact identifier value.
5. The generator evaluates state presence in step order, not by method-level isolation or global ordering.
6. A background step contributes state available to later steps in the scenario in the same way as regular steps.
7. Missing state is a warning condition, not a hard compile error.

### State Flow Rules

The generator must evaluate state flow using a scenario-local history model. The following rules apply:

- The generated test begins with an empty state set.
- As each step executes in order, the generator adds any `[Provides]` values to the known state set.
- Before a step executes, the generator checks whether every `[Requires]` value is already present in the known state set.
- If a value is absent, the later step is marked as missing that state.
- A state can be provided multiple times; the most recent value is the active one for the current scenario, but the initial requirement is only that the state name exists in the scenario state set.
- State names are scenario-local. They do not persist across independent scenarios.

### Edge Cases

The following behaviors should be explicitly handled:

- A step may require a state it also provides in the same method. This means the step mutates that state. The requirement is still validated against the state that existed before the step begins; the `Provides` annotation on the same step does not retroactively satisfy that precondition. If a later step requires the same state, that later requirement is satisfied by the state that this earlier step has now produced in execution order.
- A single step may provide more than one state value.
- A state may be provided by a background step and then later required by the scenario step.
- A step may require a state that is never defined anywhere in the scenario. This should always warn.
- A step may require a state that is defined earlier in the same scenario, but only if it was provided earlier in the actual execution order of the scenario, including Background steps. The generator must evaluate state availability in the generated execution sequence, not by source order or reflection order.

---

## Technical Approach

### Attribute and Step Metadata Model

This feature adds two new attributes to the generator utility layer and extends the CRIF model used by the source generator.

The attributes are similar in shape to existing step bindings, except they are metadata-only and do not participate in step matching by themselves.

```cs
public sealed class RequiresAttribute : Attribute
{
    public string Name { get; }
    public string? Description { get; }

    public RequiresAttribute(string name, string? description = null)
    {
        Name = name;
        Description = description;
    }
}

public sealed class ProvidesAttribute : Attribute
{
    public string Name { get; }
    public string? Description { get; }

    public ProvidesAttribute(string name, string? description = null)
    {
        Name = name;
        Description = description;
    }
}
```

### CRIF Extension

Required and provided shared state are added to the CRIF model in `src/Lib/CrifModels.cs`.

```cs
public class SharedStateCrif
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsMissing { get; set; }
}

public class StepCrif
{
    public List<SharedStateCrif> RequiresState { get; set; } = [];
    public List<SharedStateCrif> ProvidesState { get; set; } = [];
}
```

This model is intentionally lightweight. The generator will use the state metadata for review comments and warnings, while the implementation details remain in the step methods themselves.

### Analysis and Generation Pass

The generator performs an ordered flow analysis over the scenario steps:

1. Collect step metadata for every step in the generated scenario.
2. Include Background steps in the same ordered history, because they are part of the scenario setup.
3. Walk the steps in execution order.
4. For each step:
   - Record each required state item that is not in the known state set as missing.
   - Add the step's provided states to the known state set after generation or after the call, depending on intended semantics.
5. Emit comments for the state requirements and provisions discovered for the step.
6. Emit compiler warnings for missing state before the await call.

This analysis pass is deterministic and based on the ordered step sequence already produced by the generator, not on a separate runtime state tracker.

### Generated Test Comments

When tests are generated, state metadata is included directly in the generated method body.

```cs
[Test]
public async Task LoginWithNewPasswordAfterReset()
{
    // Given a test user account which had its password reset
    await PasswordSteps.ATestUserAccountWhichHadItsPasswordReset();
    // Provides DefaultUser: a UserDetails object containing the username, email, and password of the created test user account.
    // Provides ResetCode: the reset code included in the reset link sent to the user's email.
    // Provides NewPassword: the new password submitted by the user on the Reset Page.

    // When user logs in with the new password
    // Requires DefaultUser: a UserDetails object containing the username, email, and password of the created test user account.
    // Requires NewPassword: the password which the user has changed their password to.
#warning "Missing DefaultUser: a UserDetails object containing the username, email, and password of the created test user account."
    await PasswordSteps.UserCanLogInWithTheNewPassword();
}
```

### Warning Strategy

Warnings are generated as preprocessor-style `#warning` directives inserted immediately before the step invocation. This keeps them visible in the generated file and consistent with the project’s existing compile-time validation patterns.

This is a low-friction first version:

- no runtime failure is introduced
- the generated test still compiles
- the author sees the issue in code review or build output

### Why This Is in the Generator

This feature is implemented in the generator because the flow analysis depends on the exact scenario ordering already known to the generator. The generator is the best place to determine whether a prior step actually produced the state without introducing hidden runtime costs or additional reflection machinery.

---

## Open Questions

- [X] Should a step that both requires and provides the same named state be treated as satisfied before execution, or as an explicit invalid contract requiring a separate warning? **A:** The requirement is validated against the state that existed before the step runs; the step's own `Provides` does not satisfy the same-step precondition, but it does make the state available to later steps in the scenario.
- [X] Should duplicate state names across different step classes be allowed, or should the generator warn on ambiguous state declarations across a scenario? **A:** Duplicate names are allowed if they represent the same scenario-level state contract; the generator evaluates by name and execution order, not by declaring type alone.
- [X] Should `Description` be required for all state entries, or should it be optional and only included in comments when present? **A:** Description is optional and is only a convenience for code review and generated comments.
- [X] Should the generator include a summary of missing-state analysis in a separate artifact, or is generated-file warnings sufficient for the first iteration? **A:** Generated-file warnings are sufficient for the initial and final iteration.

---

## Success Metrics

- A developer can read the generated test and immediately understand which state each step depends on.
- A developer can identify missing required state before executing the scenario.
- The number of hidden step-to-step coupling failures decreases because contracts are visible in source and generated output.
- Generated warnings are actionable and specific to the name and description of the missing state.
- The feature does not materially change scenario generation time or add non-deterministic behavior.

---

## Dependencies & Constraints

**Dependencies**:
- Existing step metadata extraction and step matching pipeline
- Existing CRIF model and generated-test emission layer
- Build-time or generated-file warning conventions already used by the project

**Constraints**:
- The feature must not change the Gherkin feature file syntax.
- State detection must be deterministic and scenario-order based.
- The initial implementation should not require a runtime observer or external tracking service.
- The solution should remain compatible with existing generated test patterns and template customization.

---

## Notes & Context

This feature is intentionally scoped to the contract between steps, not to general-purpose global state management. The goal is clarity: if step A creates a value and step B depends on it, that relationship should be explicit and reviewable.

**Related Documents**:
- [PRD-STEP-CATALOG.md](PRD-STEP-CATALOG.md)
- [PRD-TEARDOWN-STEPS.md](PRD-TEARDOWN-STEPS.md)
- [PROJECT-PLAN.md](PROJECT-PLAN.md)

---

## Handoff Checklist (for AI implementation)

When handing this off for detailed design/implementation:
- [x] All user stories have clear acceptance criteria
- [X] Open questions are resolved or documented as design decisions
- [x] Technical approach section indicates affected layers
- [ ] Code patterns to follow are referenced (links to similar controllers/features)
- [x] The PRD defines state semantics and identifies where warnings are emitted
- [x] The PRD clarifies the scenario-scoped lifecycle of shared state
