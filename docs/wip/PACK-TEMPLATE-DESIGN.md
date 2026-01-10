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

## Implementation Attempts and Results

### Attempt 1: ContentFiles Only
**Approach**: Package templates using NuGet `contentFiles` feature with `copyToOutput="true"`.

**Result**: ❌ Failed
- Files packaged correctly in `.nupkg` at `contentFiles/any/any/Templates/`
- `.nuspec` contains proper `<contentFiles>` metadata
- **Issue**: SDK-style projects with PackageReference don't automatically copy contentFiles to project directory (this only works with legacy packages.config projects)

### Attempt 2: MSBuild .targets File
**Approach**: Create [`src/Analyzer/build/Gherkin.Generator.targets`](../../src/Analyzer/build/Gherkin.Generator.targets) with a target that runs `BeforeTargets="BeforeBuild"` to copy templates from NuGet cache to project directory.

**Result**: ❌ Failed
- `.targets` file packaged in both `build/` and `buildTransitive/` folders for compatibility
- **Issue**: Target never executes - no "Copied template files" message in build output
- Likely cause: Analyzer packages may not support automatic import of build targets, or targets run too late (after source generators)

### Attempt 3: MSBuild .props File
**Approach**: Create [`src/Analyzer/build/Gherkin.Generator.props`](../../src/Analyzer/build/Gherkin.Generator.props) to add templates from NuGet cache directly to `AdditionalFiles` so generator can read them from cache.

**Result**: ❌ Failed
- `.props` file packaged in both `build/` and `buildTransitive/` folders
- **Issue**: Props file not imported or templates not added to AdditionalFiles
- Generator still reports "No .mustache template file found in AdditionalFiles"

### Root Causes Identified

1. **SDK-style Project Limitations**: Modern SDK-style projects with `PackageReference` don't automatically copy `contentFiles` to the project directory like legacy `packages.config` projects did.

2. **Analyzer Package Restrictions**: Analyzer/source generator packages may have limitations on MSBuild integration:
   - Build `.props` and `.targets` files may not be automatically imported
   - Or they import too late in the build process (after source generators run)

3. **Timing Issues**: Source generators run very early in the compilation pipeline, before most MSBuild targets execute.

## Alternative Approaches to Consider

### Option 1: Keep Manual Installation (Recommended for Now)
**Pros**:
- Known to work
- Simple and transparent
- Users can easily customize

**Cons**:
- Extra step for users
- Must remember to update template when upgrading

**Implementation**: Improve documentation in User Guide with clear instructions.

### Option 2: Embedded Template with Auto-Export
**Approach**: Embed Default.mustache as a resource in the generator assembly. If generator doesn't find a template in AdditionalFiles, automatically write the embedded template to `Templates/Default.mustache`.

**Pros**:
- No manual installation required
- Template version always matches generator

**Cons**:
- Requires generator to write to file system (may have permissions issues)
- Less discoverable for users who want to customize
- Complicates the generator code

### Option 3: Dotnet New Template (Same or Separate Package)
**Approach**: Use `dotnet new` template functionality to scaffold template files. Could potentially be in the same package using `<PackageType>Analyzer;Template</PackageType>` (multiple package types), or as a separate companion package.

**Same Package Approach**:
```xml
<PropertyGroup>
  <PackageType>Analyzer;Template</PackageType>
</PropertyGroup>
```

Then users would:
1. `dotnet add package Gherkin.Generator` (installs analyzer)
2. `dotnet new install Gherkin.Generator` (installs template from same package)
3. `dotnet new gherkin-template` (scaffolds template files)

**Pros**:
- Standard .NET tooling
- Single package to maintain
- Version synchronization guaranteed (same version number)

**Cons**:
- **Two separate installation commands required** for same package (confusing UX)
- Still requires manual step (`dotnet new gherkin-template`) after installation
- Only helps with new projects, not existing ones
- **Unknown compatibility**: May have conflicts or unexpected behavior with multiple package types
- Requires creating `.template.config/template.json` and template structure
- **Documentation burden**: Users must understand and remember both installation commands

**Separate Package Approach** (`Gherkin.Generator.Templates`):
- Same pros/cons but easier to reason about (standard pattern)
- Still requires two installation commands, but clearer separation

**Assessment**: While technically possible to use multiple package types, the user experience is poor because:
1. Users still need two commands: `dotnet add package` + `dotnet new install`
2. Template installation is not automatic (requires explicit `dotnet new install`)
3. Scaffolding requires third command: `dotnet new gherkin-template`
4. More complex than embedded template approach (Option 5)

### Option 5: Embedded Template Only (Recommended - Simplified)
**Approach**: Embed `Default.mustache` as a resource in the analyzer assembly. Generator always uses the embedded template by default. **No automatic export** - keep it simple. Users can manually trigger export if they want to customize.

**Pros**:
- ✅ **Zero manual steps** - Template works automatically, no file needed
- ✅ **Version synchronization** - Template always matches generator version
- ✅ **Works with existing projects** - No installation needed
- ✅ **Single package** - No companion packages to maintain
- ✅ **Simpler implementation** - No file system writes, no export logic
- ✅ **No file clutter** - Template only exported if user wants to customize
- ✅ **Clean upgrades** - No concerns about overwriting user files

**Cons**:
- Template file not immediately visible for users who want to customize
- Need mechanism for users to export template when they want to customize

**Implementation approach**:
```csharp
// In GherkinSourceGenerator.cs
private string GetTemplateContent(GeneratorExecutionContext context)
{
    // First try AdditionalFiles (user's custom template)
    var templateFile = context.AdditionalFiles
        .FirstOrDefault(f => f.Path.EndsWith(".mustache"));
    
    if (templateFile != null)
        return templateFile.GetText()?.ToString();
    
    // Use embedded default template
    var assembly = typeof(GherkinSourceGenerator).Assembly;
    using var stream = assembly.GetManifestResourceStream(
        "Gherkin.Generator.Templates.Default.mustache");
    using var reader = new StreamReader(stream);
    return reader.ReadToEnd();
}
```

**User experience (default):**
1. Install package: `dotnet add package Gherkin.Generator`
2. Add feature file to AdditionalFiles
3. Build project
4. Tests generated using embedded template ✅

**User experience (customization):**
1. Run export command/script: `dotnet gherkin-export-template [filename]`
2. Or manually copy from GitHub/docs: `curl https://raw.githubusercontent.com/.../Default.mustache > Templates/MyCustom.mustache`
3. Add to AdditionalFiles: `<AdditionalFiles Include="Templates\MyCustom.mustache" />`
4. Build uses custom template

**Export mechanism options:**

**A. Simple PowerShell/Bash Script** (in repo):
```powershell
# scripts/Export-Template.ps1
param([string]$FileName = "Custom.mustache")
$templateUrl = "https://raw.githubusercontent.com/jcoliz/Gherkin.Generator/main/templates/Default.mustache"
New-Item -ItemType Directory -Force -Path "Templates"
Invoke-WebRequest -Uri $templateUrl -OutFile "Templates/$FileName"
```

**B. Dotnet Tool** (future enhancement):
```bash
dotnet tool install --global Gherkin.Generator.Tools
dotnet gherkin-export-template MyCustom.mustache
```

**C. Source Generator Diagnostic + Code Fix** (advanced):
- Generator emits info diagnostic: "Using embedded template. To customize, export template file."
- Code fix: "Export template to project" - creates file automatically

**D. Documentation with curl/wget command** (simplest):
```bash
# In User Guide
mkdir Templates
curl https://raw.githubusercontent.com/.../Default.mustache -o Templates/MyCustom.mustache
```

**Recommendation for export mechanism**: Start with option **D** (documentation with curl command) - simplest, no tooling needed. Can add options A-C later if user feedback indicates need.

### Option 4: Init Command Tool
**Approach**: Create a dotnet tool (`dotnet gherkin init`) that copies template files to the current project.

**Pros**:
- Clean separation of concerns
- Can update existing projects
- Standard .NET tooling pattern

**Cons**:
- Additional package to install
- Extra step in documentation
- Tool needs to find/copy correct template version

## Recommendation

**Implement Option 5 (Embedded Template Only - No Auto-Export).** This provides the best user experience with minimal complexity:

1. **Embed Default.mustache** as a resource in the analyzer assembly
2. **Use embedded template by default** - No files needed, works immediately
3. **Provide export mechanism** for users who want to customize (start with documentation/curl command)
4. **Update documentation** - Explain embedded template and customization workflow

### Why Option 5 (No Auto-Export) Over Other Approaches?

- **vs Option 1 (Manual installation)**: Creates friction, requires remembering to copy on install/upgrade
- **vs Option 2 (Embedded with auto-export)**: Creates file clutter for users who don't need customization, complicates generator with file I/O
- **vs Option 3 (Template package)**: Requires multiple commands, complex UX, requires understanding dotnet template system
- **vs Option 4 (Init tool)**: Requires separate tool installation, extra package to maintain

### Benefits of No Auto-Export

1. **Cleaner projects** - No `Templates/` folder unless user needs customization
2. **Simpler implementation** - No file system writes, no "should I overwrite?" logic
3. **Always up-to-date** - Users get latest template with each package upgrade automatically
4. **Less confusion** - No questions about "which template is being used" or "why are there two templates"
5. **Progressive disclosure** - Advanced customization only when needed

### Implementation Plan for Option 5 (No Auto-Export)

1. **Embed template as resource** in [`src/Analyzer/Gherkin.Generator.csproj`](../../src/Analyzer/Gherkin.Generator.csproj):
   ```xml
   <ItemGroup>
     <EmbeddedResource Include="..\..\templates\Default.mustache" />
   </ItemGroup>
   ```

2. **Update [`GherkinSourceGenerator.cs`](../../src/Analyzer/GherkinSourceGenerator.cs)** to:
   - Load template from embedded resource if not found in AdditionalFiles
   - Use embedded template as fallback (no export needed)
   - Simple, clean implementation

3. **Update [`docs/USER-GUIDE.md`](../../docs/USER-GUIDE.md)** with new workflow:
   - Remove manual "copy template file" instructions from installation
   - Document that embedded template is used automatically
   - Add "Customizing Templates" section with export options:
     - Option A: Download from GitHub with curl/wget
     - Option B: Copy from another project
     - Option C: Use provided script (if we add one)
   - Document adding custom template to AdditionalFiles

4. **Optional: Create export helper script** (future enhancement):
   - PowerShell script: [`scripts/Export-Template.ps1`](../../scripts/Export-Template.ps1)
   - Bash script: `scripts/export-template.sh`
   - Downloads template from GitHub to project

5. **Test thoroughly**:
   - Default behavior: verify embedded template works without any files
   - Custom template: verify AdditionalFiles template overrides embedded
   - Upgrade scenario: verify embedded template updates automatically

6. **Clean up investigation artifacts**:
   - Remove [`src/Analyzer/build/Gherkin.Generator.targets`](../../src/Analyzer/build/Gherkin.Generator.targets)
   - Remove [`src/Analyzer/build/Gherkin.Generator.props`](../../src/Analyzer/build/Gherkin.Generator.props)
   - Remove or repurpose [`templates/Default.mustache.md`](../../templates/Default.mustache.md) (not needed if no auto-export)

## Files Created During Investigation

These files can be removed if we abandon this approach:
- [`src/Analyzer/build/Gherkin.Generator.targets`](../../src/Analyzer/build/Gherkin.Generator.targets)
- [`src/Analyzer/build/Gherkin.Generator.props`](../../src/Analyzer/build/Gherkin.Generator.props)
- [`templates/Default.mustache.md`](../../templates/Default.mustache.md) - Keep this for future use

## References

- [NuGet ContentFiles Documentation](https://learn.microsoft.com/en-us/nuget/reference/nuspec#including-content-files)
- [MSBuild Pack Target](https://learn.microsoft.com/en-us/nuget/reference/msbuild-targets)
- [SDK-style Projects and Content](https://learn.microsoft.com/en-us/nuget/consume-packages/package-references-in-project-files#controlling-dependency-assets)
- [`src/Analyzer/Gherkin.Generator.csproj`](../../src/Analyzer/Gherkin.Generator.csproj) - Current package configuration
