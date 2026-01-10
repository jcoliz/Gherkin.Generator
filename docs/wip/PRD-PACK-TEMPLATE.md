---
status: Draft # Draft | In Review | Approved | Implemented
ado: [Link to ADO Item]
references:
    - docs\USER-GUIDE.md
---

# Product Requirements Document: Pack template into library release

## Problem Statement

When user installs Gherkin.Generator, they must also copy the Default.mustache template from the GitHub site in order
to effectively use the generator. This is an extra step that adds complexity and room for mistakes.

Likewise, when the generator is updated, user must get an updated copy from the site which matches the
functionality included in the new release.

---

## Goals & Non-Goals

### Goals
- [ ] Install matching Default.mustache template into user's project folder when installing the package
- [ ] Update latest matching Default.mustache template when upgrading to a new package version
- [ ] Ensure template version always matches the generator version
- [ ] Provide Templates/Default.mustache.md file documenting the overwrite behavior

### Non-Goals
- Support for preventing overwrites of Default.mustache on updates (users must use custom filenames for customized templates)
- Migration or backup of existing Default.mustache files before overwriting
- Detection or notification of user customizations to Default.mustache

---

## User Stories

### Story 1: User - Installs
**As a** developer creating tests in my project
**I want** to have a default mustache template in my project directory after installing Gherkin.Generator
**So that** I have a usable installation without any extra steps

**Acceptance Criteria**:
- [ ] Matching Default.mustache template in Templates/ folder on install
- [ ] Default.mustache.md file in Templates/ folder explaining overwrite policy and how to create custom templates

### Story 2: User - Updates
**As a** developer creating tests in my project
**I want** to have a an updated default mustache template in my project directory after updating Gherkin.Generator
**So that** I don't get difficult-to-troubleshoot errors caused by out-of-date template

**Acceptance Criteria**:
- [ ] Matching Default.mustache template in Templates/ folder on update
- [ ] Existing Default.mustache file is overwritten with the updated version

---

## Technical Approach (Optional)

Not actually sure of the technical approach!

**Key Business Rules**:
1. **Template Overwrite Policy** - Default.mustache is always overwritten on package install and upgrade. Users who want customized templates must copy Default.mustache to a different filename (e.g., Custom.mustache) and reference it in their .csproj file.
2. **Version Matching** - The bundled Default.mustache template version must always match the Gherkin.Generator package version to ensure compatibility.
3. **Standard Location** - Template is installed to Templates/ directory relative to the project root, consistent with existing documentation.
4. **User Documentation** - A Default.mustache.md file is installed alongside the template to document the overwrite behavior and guide users on creating custom templates. This provides in-context documentation without installation message noise.
5. **Directory Creation** - The Templates/ directory is automatically created if it doesn't exist, following standard NuGet contentFiles behavior.

---

## Open Questions

- [X] What happens if the Templates/ directory doesn't exist - create it automatically or fail with clear error? **A** Create

---

## Dependencies & Constraints

**Dependencies**:
- NuGet packaging and deployment mechanism that supports content file installation
- Existing [`templates/Default.mustache`](../../templates/Default.mustache) file in repository

**Constraints**:
- Must work across Windows, Linux, and macOS platforms
- Must not break existing projects that have manually copied Default.mustache
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

---

## Handoff Checklist (for AI implementation)

When handing this off for detailed design/implementation:
- [ ] Document stays within PRD scope (WHAT/WHY). If implementation details are needed, they are in a separate Design Document. See [`PRD-GUIDANCE.md`](PRD-GUIDANCE.md).
- [ ] All user stories have clear acceptance criteria
- [ ] Open questions are resolved or documented as design decisions
- [ ] Technical approach section indicates affected layers
- [ ] Code patterns to follow are referenced (links to similar controllers/features)
