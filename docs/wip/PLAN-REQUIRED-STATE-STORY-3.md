---
status: Approved # Draft | In Review | Approved | Implemented
prd_document: PRD-REQUIRED-STATE.md
issue: 25
story: 3
---

Updated todo list

I’m showing the final Story 3 plan with the unit-test-only scope and the example-test exclusion called out explicitly.

## Plan: Story 3 - Receive warnings for missing state

TL;DR: Build on the completed Story 1 and Story 2 work by evaluating scenario state in execution order, flagging unmet `[Requires]` entries, and emitting warning comments immediately before the offending step call. The implementation should keep `BaseProvides` as the scenario-start state source and validate the behavior with the generator unit test suite only.

### Steps
1. Confirm the canonical state model from Stories 1 and 2: step-level `[Requires]` and `[Provides]` metadata already exists on each step CRIF, `BaseProvides` is stored in project metadata, and the generated template renders the state comments in execution order. This defines the contract for the Story 3 warning logic and depends on the completed metadata pipeline.  
2. Add a scenario-order evaluation pass in the conversion/generation pipeline that walks each step in actual execution order, starting from the `BaseProvides` set and then adding each step’s `ProvidesState` entries after the step completes. Before each step executes, compare its `RequiresState` entries against the accumulated state set and track any missing names. This is the root logic for Story 3 and should include Background steps as part of the same ordered state history.  
3. Wire the missing-state result into the generated test output so the template emits a warning comment before the step call, with the warning text including the missing state name and description. Keep the logic silent when the requirement is satisfied by `BaseProvides` and leave existing `(Provided by base)` comments unchanged.  
4. Add focused regression tests covering:
   - state satisfied by a prior step
   - state missing from earlier steps
   - `BaseProvides` satisfying a requirement without a warning
   - Background state satisfying a later scenario requirement
   - no warning when all requirements are met  
5. Run the generator unit test suite only and confirm the Story 3 assertions pass. Example tests are excluded from this story’s verification scope.

### Relevant files
- PRD-REQUIRED-STATE.md — source acceptance criteria and state-flow rules
- CrifModels.cs — `StepCrif`, `SharedStateCrif`, and state metadata contract
- GherkinToCrifConverter.cs — existing `BaseProvides` marking pass and the right place for ordered state evaluation
- Default.mustache — generated method template where warnings should render before each step invocation
- StepMethodAnalyzer.cs — extraction for `[Requires]`, `[Provides]`, and `[BaseProvides]`
- tests — generator test suite to extend with Story 3 assertions

### Verification
This story is validated through the generator unit test suite only. Example tests are explicitly out of scope.

1. Run the relevant unit test suite for required-state scenarios and confirm all Story 3 assertions pass.
2. Verify that generated test output includes warning comments before steps with missing state requirements and no warnings for valid state chains.
3. Confirm that `BaseProvides` state suppresses warnings appropriately and that Background state integrates correctly in the scenario-order evaluation.

### Decisions
- Story 1 and Story 2 are complete and remain the baseline behavior; Story 3 extends the current metadata and rendering pipeline rather than redesigning it.
- The canonical state history is the scenario execution order, including Background steps, not reflection order or source order.
- `BaseProvides` is treated as scenario-start state and is included before Background evaluation; matching requirements do not warn.
- The warning is advisory only and remains consistent with the PRD’s “warning condition, not a hard compile error” model.
- Verification is unit-test-only; example tests are deliberately excluded from this story.

### Further considerations
1. Rendering the warning as direct comment text just before the step call is the lowest-risk option; it keeps the output readable without adding new runtime APIs.
2. If a step both requires and provides the same state in the same method, the requirement should still be validated against the state before execution begins.
3. Duplicate state names should be treated as scenario-local and deduplicated only for warning evaluation, not for metadata retention.
