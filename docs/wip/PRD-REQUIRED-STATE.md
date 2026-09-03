---
status: Draft (for Story 1) # Draft | In Review | Approved | Implemented
design_document: [Link to design document]
issue: [TBD]
---

# Product Requirements Document: Required State Annotations

## Problem Statement

Steps communicate between each other using shared state maintained in the test infrastructure.
One step will require state provided by another step. There is no visibility or enforcement of this
requirement, so it's too easy to write tests that fail because the required state has not been
provided.

## Goals & Non-Goals

### Goals

- [ ] Annotate what shared state is required for a step to complete
- [ ] Annotate what shared state is provided by a step
- [ ] Surface these shared state mutations in generated tests in a way that makes it easy to understand whether required state is provided
- [ ] Give the test user a warning when required shared state is not provided

### Non-Goals

- TBD

---

## User Stories

### Story 1: Developer - Annotate shared state mutations in steps
**As a** developer of an application writing tests to use this library
**I want** to annotate my shared state annotations
**So that** I can manually review step paths to discover missing shared state errors

**Acceptance Criteria**:
- [ ] `[Provides]` annotations can be added to step definitions
- [ ] `[Requires]` annotations can be added to step definitions

### Story 1: Developer - Review shared state mutations in generated tests
**As a** developer of an application writing tests to use this library
**I want** to easily discover whether required state is provided in each generated test
**So that** catch missing shared state errors before the test runs

**Acceptance Criteria**:
- [ ] Generated tests include comments before each step with required state showing what state they require
- [ ] Generated tests include comments after each step with required state showing what state they provide

### Story 2: Developer - Receive compiler warnings when required state mutations are missing 
**As a** developer of an application writing tests to use this library
**I want** to receive a compiler warning when one step requires state which has not been already provided in this test run
**So that** missing shared state errors are immediately evident at compile time


**Acceptance Criteria**:
- [ ] Generated tests produce a warning when shared state is missing

---

## Technical Approach

### Annotation

New attributes are created, allowing annotation as follows:

```cs
    /// <summary>
    /// When user changes their password on the profile page
    /// </summary>
    [When("user changes their password on the profile page")]
    [Given("user changes their password on the profile page")]
    [Provides("DefaultUser", "a UserDetails object containing the username, email, and password of the created test user account, which is needed to verify that the reset link was sent to the correct email address.")]
    [Provides("NewPassword", "the new password submitted by the user on the Reset Page, which is created by appending 'New' to the original password of the test user account.")]
    public async Task UserChangesTheirPasswordOnTheProfilePage()
```

```cs
    /// <summary>
    /// When user submits email and weak password on Reset Page
    /// </summary>
    [When("user submits email and a weak password on Reset Page")]
    [Requires("DefaultUser", "a UserDetails object containing the username, email, and password of the created test user account.")]
    public async Task UserSubmitsEmailAndWeakPasswordOnResetPage()
```

### CRIF

Required and provided shared state are added to the CRIF `Gherkin.Generator.Lib.StepCrif` in `src\Lib\CrifModels.cs`.

```cs
    /// <summary>
    /// List of state names that this step requires (e.g., "UserLoggedIn", "DatabaseSeeded").
    /// </summary>
    public List<SharedStateCrif> RequiresState { get; set; } = [];

    /// <summary>
    /// List of state names that this step provides (e.g., "UserLoggedIn", "DatabaseSeeded").
    /// </summary>
    public List<SharedStateCrif> ProvidesState { get; set; } = [];
```

SharedStateCrif has this shape:

- string Name: State identifier given in the attribute
- string? Description: Optional state description given in the attribute
- bool IsMissing: True if we've analyzed the step flow and determined the required state was not provided by an earlier step

### Generated Test Comments

When tests are generated, these CRIF elements are included in the generated tests:

```cs
    /// <summary>
    /// Login with new password after reset
    /// </summary>
    [Test]
    public async Task LoginWithNewPasswordAfterReset()
    {
        // Given a test user account which had its password reset
        await PasswordSteps.ATestUserAccountWhichHadItsPasswordReset();
        // Provides DefaultUser: a UserDetails object containing the username, email, and password of the created test user account, which is needed to verify that the reset link was sent to the correct email address.
        // Provides ResetCode: the reset code included in the reset link sent to the user's email, which is needed to navigate to the password reset page.
        // Provides NewPassword: the new password submitted by the user on the Reset Page, which is created by appending 'New' to the original password of the test user account.

        // When user logs in with the new password
        // Requires DefaultUser: a UserDetails object containing the username, email, and password of the created test user account, where the password is the original password before the reset or change        
        // Requires NewPassword: the password which the user has changed their password to
        await PasswordSteps.UserCanLogInWithTheNewPassword();

        // Then user is logged in successfully
        await AuthenticationSteps.UserLogsInSuccessfully();

    }
```

### Missing shared state warnings

When required shared state is missing, an error is added into the generated test

```cs
    /// <summary>
    /// Login with new password after reset
    /// </summary>
    [Test]
    public async Task LoginWithNewPasswordAfterReset()
    {
        // When user logs in with the new password
#warning "Missing DefaultUser: a UserDetails object containing the username, email, and password of the created test user account, where the password is the original password before the reset or change"
#warning "Missing NewPassword: the password which the user has changed their password to"
        await PasswordSteps.UserCanLogInWithTheNewPassword();

        // Then user is logged in successfully
        await AuthenticationSteps.UserLogsInSuccessfully();

    }
```

This will require an analysis pass for each step in the CRIF where we review the steps which came before to see if required state
was provided. Of course, we will need to review background steps too!

---

## Open Questions

- [ ] TBD

---

## Success Metrics

- TBD

---

## Dependencies & Constraints

**Dependencies**:
- [Other features or systems this depends on]

**Constraints**:
- [Technical, time, or resource constraints]

---

## Notes & Context

[Any additional context, links to related documents, or background information]

**Related Documents**:
- [Link to related PRDs]
- [Link to analysis documents]

---

## Handoff Checklist (for AI implementation)

When handing this off for detailed design/implementation:
- [ ] All user stories have clear acceptance criteria
- [ ] Open questions are resolved or documented as design decisions
- [ ] Technical approach section indicates affected layers
- [ ] Code patterns to follow are referenced (links to similar controllers/features)
