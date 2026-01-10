---
status: Draft
prd: docs/wip/PRD-PACK-TEMPLATE.md
---

# Design Document: Pack Template into Library Release

## Overview

This document describes the implementation approach for automatically installing the Default.mustache template file into user projects when they install or upgrade the Gherkin.Generator NuGet package.

## Feasibility Analysis

**Yes, this is possible using NuGet contentFiles functionality.**

NuGet supports including content files in packages that are automatically copied to the consuming project. For SDK-style projects (.NET Core and later), this is done using the `contentFiles` feature which:
- Automatically creates directory structure in the target project
- Copies files on package install
- Updates files on package upgrade
- Supports including files in version control

## Technical Approach

### 1. Add ContentFiles to Package

Modify [`src/Analyzer/Gherkin.Generator.csproj`](../../src/Analyzer/Gherkin.Generator.csproj) to include the template files as content:

```xml
<ItemGroup>
  <!-- Pack template files as content -->
  <None Include="..\..\templates\Default.mustache" Pack="true" PackagePath="contentFiles\any\any\Templates" />
  <Content Include="..\..\templates\Default.mustache">
    <PackagePath>contentFiles\any\any\Templates\Default.mustache</PackagePath>
    <CopyToOutputDirectory>Never</CopyToOutputDirectory>
    <BuildAction>None</BuildAction>
    <IncludeInPackage>true</IncludeInPackage>
  </Content>
</ItemGroup>
```

**Path structure**: `contentFiles\any\any\` means:
- First `any`: Any target framework
- Second `any`: Any programming language
- Followed by the relative path in the target project (`Templates\`)

### 2. Create Default.mustache.md Documentation

Create [`templates/Default.mustache.md`](../../templates/Default.mustache.md) with content explaining:
- That Default.mustache is always overwritten on package updates
- How to create custom templates (copy to different filename)
- Reference to User Guide for more details

Then include it in the package alongside the template:

```xml
<None Include="..\..\templates\Default.mustache.md" Pack="true" PackagePath="contentFiles\any\any\Templates" />
<Content Include="..\..\templates\Default.mustache.md">
  <PackagePath>contentFiles\any\any\Templates\Default.mustache.md</PackagePath>
  <CopyToOutputDirectory>Never</CopyToOutputDirectory>
  <BuildAction>None</BuildAction>
  <IncludeInPackage>true</IncludeInPackage>
</Content>
```

### 3. Configure ContentFiles Behavior

Add property to control content file copying:

```xml
<PropertyGroup>
  <!-- Ensure content files are copied to project -->
  <ContentTargetFolders>contentFiles</ContentTargetFolders>
</PropertyGroup>
```

### 4. Test the Package Locally

Before publishing, test the package installation locally:

1. Build the package: `dotnet pack src/Analyzer/Gherkin.Generator.csproj`
2. Create a test project
3. Install the local package: `dotnet add package Gherkin.Generator --source /path/to/nupkg`
4. Verify Templates/Default.mustache and Templates/Default.mustache.md are created
5. Upgrade the package version and verify overwrite behavior

## Implementation Steps

1. **Create Default.mustache.md documentation file** in [`templates/`](../../templates/) directory
   - Document overwrite policy
   - Provide custom template guidance
   - Link to User Guide

2. **Update Gherkin.Generator.csproj** to include contentFiles
   - Add `<Content>` items for both template files
   - Configure packaging paths
   - Set appropriate build actions

3. **Test locally** with a test project
   - Verify install behavior
   - Verify upgrade/overwrite behavior
   - Test on Windows, Linux, and macOS if possible

4. **Update User Guide** ([`docs/USER-GUIDE.md`](../../docs/USER-GUIDE.md))
   - Remove manual template installation steps
   - Document that template is now included automatically
   - Keep custom template customization section

5. **Update version** in [`src/Directory.Build.props`](../../src/Directory.Build.props)
   - Increment version for this new feature

6. **Test package publication workflow**
   - Verify package builds correctly
   - Verify content files are included in .nupkg

## Alternative Approaches Considered

### Build Targets Approach
Instead of contentFiles, use MSBuild targets to copy files on build. This was rejected because:
- More complex to implement
- Requires executing custom build logic
- ContentFiles is the standard NuGet approach for this scenario

### Source Generator Approach
Emit the template content directly in the generator. This was rejected because:
- Template must be a physical file for Stubble to load
- Users need to customize templates
- Would prevent template versioning and updates

### Package Tools Approach
Install template as a dotnet tool or global tool. This was rejected because:
- Adds complexity (separate tool installation)
- Doesn't guarantee version matching with generator
- Not integrated with package management

## Benefits of ContentFiles Approach

1. **Standard NuGet mechanism** - Well-documented and widely used
2. **Automatic directory creation** - NuGet creates Templates/ if needed
3. **Version synchronization** - Template version always matches package version
4. **Cross-platform** - Works on Windows, Linux, macOS
5. **Simple implementation** - Just MSBuild configuration, no custom code
6. **Discoverable** - Files appear in project, easy to find and modify

## Risks and Mitigations

### Risk: Overwriting user customizations
**Mitigation**: Default.mustache.md clearly documents the overwrite policy and guides users to copy to custom filename

### Risk: Version control conflicts
**Mitigation**: Users should commit Templates/ directory to source control; merge conflicts are standard Git workflow

### Risk: Different behavior in legacy .csproj vs SDK-style
**Mitigation**: Gherkin.Generator targets .NET Standard 2.0 and primarily serves modern SDK-style projects; legacy projects would need manual template management (document this in User Guide if needed)

### Risk: Build-time vs install-time copying
**Mitigation**: ContentFiles copies on restore/install, not on build, which is the desired behavior

## Success Criteria

- [ ] Default.mustache appears in Templates/ folder on package install
- [ ] Default.mustache.md appears in Templates/ folder on package install
- [ ] Default.mustache is overwritten on package upgrade
- [ ] Works on Windows, Linux, and macOS
- [ ] No manual template copying required
- [ ] User Guide reflects new automatic installation

## References

- [NuGet ContentFiles Documentation](https://learn.microsoft.com/en-us/nuget/reference/nuspec#including-content-files)
- [MSBuild Pack Target](https://learn.microsoft.com/en-us/nuget/reference/msbuild-targets)
- [`src/Analyzer/Gherkin.Generator.csproj`](../../src/Analyzer/Gherkin.Generator.csproj) - Current package configuration
