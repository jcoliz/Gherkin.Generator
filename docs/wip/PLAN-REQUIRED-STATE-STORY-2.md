---
status: Implemented # Draft | In Review | Approved | Implemented
prd_document: PRD-REQUIRED-STATE.md
issue: 25
story: 2
---
# Plan: Story 2 - Review state flow in generated tests

TL;DR: Add generator-side state metadata to the CRIF and render it into the generated test body as step-level comments, using the same execution order as the scenario, including Background steps. This keeps the feature reviewable without changing runtime behavior or feature syntax.

## Steps

1. Confirm the metadata model for `Requires` and `Provides` is captured from step methods and stored on the step-level CRIF objects, with `Name`, `Description`, and any ordering metadata needed for rendered output. This depends on the existing attribute definitions in RequiresAttribute.cs and ProvidesAttribute.cs, plus the metadata extraction path in StepMethodAnalyzer.cs.

2. Extend the conversion pipeline that turns matched steps into `StepCrif` instances so each step carries its required/provided state list in the same order seen by the generator. This should include Background steps in the ordered scenario flow and keep the data shape aligned with the CRIF contracts in CrifModels.cs.

3. Update the generated test template in Default.mustache so each step emits a clear “before” comment for required state and an “after” comment for provided state. Ensure the comments are generated in execution order and keep the output readable without disturbing the existing generated code structure.

4. Add focused tests that cover:
   - valid state flow with Background and scenario steps
   - a missing dependency producing the expected warning/comment pattern
   - generated output without state annotations staying unchanged

5. Validate with the relevant `dotnet test` target for the generator/test project and confirm the generated output matches the Story 2 acceptance criteria.

## Relevant files

- RequiresAttribute.cs — required-state metadata contract
- ProvidesAttribute.cs — provided-state metadata contract
- StepMethodAnalyzer.cs — existing metadata extraction pipeline
- CrifModels.cs — step CRIF model and shared-state metadata locations
- StepProcessor.cs — conversion of matched steps into CRIF objects
- Default.mustache — generated test template rendering
- tests — generator tests to extend with Story 2 scenarios

## Verification

1. Run the relevant generator test suite and confirm the new Story 2 assertions pass.
2. Inspect generated output for a scenario with Background state and verify the required/provided state comments appear in the correct execution order.
3. Verify scenarios without any state annotations still generate unchanged output.

## Decisions

- Keep Story 2 scoped to generated-code reviewability; no runtime enforcement or feature-file syntax changes.
- Use the actual scenario execution order, including Background steps, as the canonical ordering for state comments.
- Treat acceptance criteria as generator output requirements, not as a separate runtime artifact.

## Further considerations

1. Repeated state names represent the same scenario state, not conflicting duplicates. The generator should render a single logical state name in comments and warnings, while preserving the fact that it may be referenced by multiple steps in the scenario flow.
2. Keep the rendering optional for `Description` so comments remain clean when descriptions are omitted.
3. Watch template formatting closely so comments remain readable and do not break adjacent generated code.
