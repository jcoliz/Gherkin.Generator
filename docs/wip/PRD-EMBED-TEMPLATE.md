---
status: Implemented
references:
    - docs\USER-GUIDE.md
    - docs\wip\PACK-TEMPLATE-DESIGN.md
    - docs\wip\PRD-PACK-TEMPLATE.md
---

# Product Requirements Document: Embed template into analyzer

## Problem Statement

When user installs Gherkin.Generator, they must also copy the Default.mustache template from the GitHub site in order
to effectively use the generator. This is an extra step that adds complexity and room for mistakes.

Likewise, when the generator is updated, user must get an updated copy from the site which matches the
functionality included in the new release.

---

## Goals & Non-Goals

### Goals
- [ ] New user has working template with no additional effort other than adding the package
- [ ] When user upgrades package version, user has updated package immediately with no effort
- [ ] Advanced user can exert detailed control over test generation by customizing the template

### Non-Goals
- Upgrade user's custom template to new versions of the library.

---

## User Stories

### Story 1: User - Installs
**As a** developer creating tests in my project
**I want** to have a working Gherkin.Generator after installing
**So that** I can be up and running quickly with a minimum of steps

**Acceptance Criteria**:
- [ ] User can add Gherkin.Generator to a new package, and get results without manually copying a template from anywhere

### Story 2: User - Updates
**As a** developer creating tests in my project
**I want** to have a working Gherkin.Generator after updating package versions
**So that** I don't get difficult-to-troubleshoot errors caused by out-of-date templates

**Acceptance Criteria**:
- [ ] User can update Gherkin.Generator package, which has changes to its CRIF, without any breakages

### Story 3: Advanced User - Customized
**As a** developer wanting rich control over my tests
**I want** to make detailed changes to the format of the generated tests
**So that** I can accommodate my unique testing needs

**Acceptance Criteria**:
- [ ] Download the default template from the project, customize it, and add it to their project

---

## Technical Approach (Optional)

1. Embed `Default.template` into the analyzer project
2. Use it as the template if one is not specified in additional files
3. If one **is** listed in additional files in the project, use that instead

---

## Open Questions

- [x] **How should advanced users obtain the default template for customization?**
  - **Answer**: Document curl/wget command in User Guide to download from GitHub. Simple, cross-platform, no tooling needed.
  - **Alternative**: Could provide PowerShell/Bash helper script in future if user feedback indicates need.

- [x] **Should we auto-export the embedded template to the project directory?**
  - **Answer**: No. Keep embedded template as the invisible default. Only export when user explicitly wants to customize.
  - **Rationale**: Cleaner projects, simpler implementation, no "which template am I using?" confusion.

- [x] **What happens to existing projects already using custom templates?**
  - **Answer**: No impact. Generator checks AdditionalFiles first. If custom template present, it's used. If not, embedded template is used.
  - **Migration path**: Existing projects continue working unchanged.

- [x] **How do we handle backwards compatibility with projects on old package versions that expect manual template?**
  - **Answer**: Not a breaking change. Old versions required manual template or they failed. New version works with or without manual template.
  - **Benefit**: Projects that forgot to add template will now work automatically.

---

## Dependencies & Constraints

**Dependencies**:
- Existing [`templates/Default.mustache`](../../templates/Default.mustache) file in repository

**Constraints**:
- Must work across Windows, Linux, and macOS platforms
- Must not break existing projects that are using a customized mustache template
- Solution must work with standard NuGet package installation mechanisms

---

## Notes & Context

Currently, users must manually copy Default.mustache from the GitHub repository after installing the package. This creates potential for:
- Version mismatch between generator and template
- Installation errors due to missing template
- Confusion for new users about where to get the template

The solution should integrate with standard NuGet package installation so users get a working setup immediately.

**Related Documents**:
- [`docs/USER-GUIDE.md`](../USER-GUIDE.md) - Documents current manual template installation process and custom template usage
- [`PRD-PACK-TEMPLATE.md`](PRD-PACK-TEMPLATE.md) - Previous design superseded by this approach

---

## Handoff Checklist (for AI implementation)

When handing this off for detailed design/implementation:
- [ ] Document stays within PRD scope (WHAT/WHY). If implementation details are needed, they are in a separate Design Document. See [`PRD-GUIDANCE.md`](PRD-GUIDANCE.md).
- [ ] All user stories have clear acceptance criteria
- [ ] Open questions are resolved or documented as design decisions
- [ ] Technical approach section indicates affected layers
- [ ] Code patterns to follow are referenced (links to similar controllers/features)
