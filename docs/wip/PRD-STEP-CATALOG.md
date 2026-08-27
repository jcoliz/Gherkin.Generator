---
project: Gherkin.Generator
status: Draft # Draft | In Review | Approved | Implemented
target_release: [Release Milestone when predominance of work is expected]
design_document: [Link to design document]
issue: 23
---

# Product Requirements Document: Step Catalog

## Problem Statement

When writing new tests, it's difficult to know which steps already exist, and should be used instead of writing new steps.
This leads to accidentally writing semi-duplicate steps that have to be merged later. It also slows down test-writing because
too much time is spent searching through steps files.

## Goals & Non-Goals

### Goals

- [ ] Generate a deterministic catalog of all step bindings available to the consuming test project.
- [ ] Preserve the exact Gherkin binding text, including keywords, aliases, and parameter placeholders.
- [ ] Include enough declaring-symbol and documentation metadata for an author to decide whether a step is reusable.
- [ ] Regenerate the catalog when bindings change
- [ ] Regenerate the catalog when relevant XML documentation changes (in the future when documentation is used as an input).

### Non-Goals

- Do not change generated test behavior or require application test projects to maintain a hand-written catalog.
- Do not infer semantic equivalence between different step phrases.
- Do not validate whether a step passes at runtime.
- Do not replace GHERKIN004 diagnostics for unmatched feature steps.
- Do not require application developers to maintain a separate catalog file.
- Do not include arbitrary source-code implementation details beyond declaring symbol and documentation metadata.

---

## User Stories

### Story 1: Developer - Find available steps
**As a** developer of an application writing tests to use this library
**I want** to easily discover existing steps with their exact Gherkin language
**So that** I can re-use existing steps and quickly know if I need to write new ones

**Acceptance Criteria**:
- [ ] A steps catalog file is created when the tests are built
- [ ] The catalog contains every binding available to the generated tests, including inherited or referenced-library bindings, or explicitly documents that it contains only bindings declared in the consuming project.
- [ ] The exact matching language from the step attribute including parameter placeholders
- [ ] The steps are grouped by source file
- [ ] The steps are further organized by keyword (Given/Then/When)
- [ ] Within each of those categories, the steps are listed alphabetically
- [ ] Existing generated test output remains unchanged except for the new catalog artifact.

### Story 2: Developer - Understand step behavior
**As a** developer of an application writing tests to use this library
**I want** optional documentation and state-flow details for each step
**So that** I can determine whether an existing step is appropriate without opening every source file

**Acceptance Criteria**:
- [ ] The catalog can include the XML `<summary>` documentation for the declaring class and method
- [ ] The catalog can include `REQUIRES:` and `PROVIDES:` metadata from the step documentation
- [ ] The catalog identifies the declaring type and method so the implementation can be located
- [ ] Missing XML documentation or metadata does not prevent a step from appearing in the catalog
- [ ] XML documentation support is independent of the initial catalog of exact binding phrases
- [ ] The design documents whether XML documentation is read from source, compiler-generated XML files, or another supported input
- [ ] Each step includes the symbol which implements the step, so author can review details of the step, in the form Type.MethodName (Would be nice)
---

## Technical Approach

The catalog should be made available as a build-produced artifact that developers can easily open or discover. Markdown is one possible presentation format; the implementation may choose the output mechanism and format that best fits Gherkin.Generator and the consuming build environment, provided the acceptance criteria are met.

The following illustrates the information and organization the catalog should provide. It is not intended to prescribe the output format or implementation.

```markdown
# Step Catalog

## ManageSteps.cs

* Given selected the first {count} items

* When changing the bulk store to {string1}
* When selecting the first item

* Then the store is still selected
```

## Key Business Rules

### Does not affect test behaviour

The creation of a step catalog should not alter the generation of tests.

### Include all available steps

All steps consumed by the generator which were available to be matched should be included in the output.

### Aliases and duplicate behavior

When a single step is represented by multiple attributes, such as the following, this results in three separate entries in the resulting catalog. The fact that they resolve to the same C# method is not relevant. What we need to know is: Which steps are available?

```cs
[Given("user is logged in")]
[Given("user has logged in")]
[When("user logs in")]
```

---

## Open Questions

- [ ] Exact method of presenting the catalog needs to be determined

---

## Success Metrics

- A developer can locate the exact binding for a known Gherkin phrase without manually searching every step file.
- A developer can identify whether a proposed step is new or a close reuse candidate from the generated catalog.
- The catalog remains correct after adding, renaming, removing, or aliasing a step.
- The catalog generation adds no observable change to generated test execution.
- The catalog generation time remains acceptable for incremental and clean builds.

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
- [ ] Document stays within PRD scope (WHAT/WHY). If implementation details are needed, they are in a separate Design Document. See [`PRD-GUIDANCE.md`](PRD-GUIDANCE.md).
- [ ] All user stories have clear acceptance criteria
- [ ] Open questions are resolved or documented as design decisions
- [ ] Technical approach section indicates affected layers
- [ ] Code patterns to follow are referenced (links to similar controllers/features)
