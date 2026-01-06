# Release Workflow

This document describes the process for releasing new versions of Gherkin.Generator to NuGet.

## Prerequisites

### One-Time Setup

1. **Get a NuGet API Key**:
   - Go to https://www.nuget.org/
   - Sign in and navigate to API Keys
   - Create a new API key with "Push" permission for `Gherkin.Generator`
   - Copy the key (you won't be able to see it again)

2. **Add to GitHub Secrets**:
   - Go to repository Settings → Secrets and variables → Actions
   - Click "New repository secret"
   - Name: `NUGET_API_KEY`
   - Value: Paste your NuGet API key
   - Click "Add secret"

## Release Process

### 1. Verify CI Status

Before creating a release, ensure the commit you want to release has a passing CI workflow:

1. Go to https://github.com/jcoliz/Gherkin.Generator/actions
2. Find the CI workflow run for your commit
3. Verify all checks passed (build, tests, etc.)
4. Only proceed if CI is green ✅

### 2. Create and Push Tag

Create a git tag for the release version:

```powershell
# Create tag for the version (e.g., 0.0.1)
git tag 0.0.1

# Push the tag to GitHub
git push origin 0.0.1
```

**Note**: Use semantic version format (e.g., `0.0.1`, `0.1.0`, `1.0.0`). No `v` prefix needed (though workflow handles it if present).

### 3. Create GitHub Release

1. **Navigate to Releases**:
   - Go to https://github.com/jcoliz/Gherkin.Generator/releases
   - Click "Draft a new release"

2. **Set Release Details**:
   - **Tag**: Select the tag you just pushed (e.g., `0.0.1`)
   - **Target**: `main` branch (should auto-select based on tag)
   - **Release Title**: Same as tag (e.g., `0.0.1`)
   - **Description**: Add release notes describing:
     - New features
     - Bug fixes
     - Breaking changes (if any)
     - Migration notes (if needed)

3. **Publish**:
   - Click "Publish release"
   - The release workflow will automatically start

### 4. Monitor Workflow

1. **Check Progress**:
   - Go to Actions tab: https://github.com/jcoliz/Gherkin.Generator/actions
   - Find the running "Release" workflow
   - Watch the progress

2. **Workflow Steps**:
   - ✅ Checkout code
   - ✅ Setup .NET
   - ✅ Restore dependencies
   - ✅ Build solution
   - ✅ Run unit tests
   - ✅ Extract version from tag
   - ✅ Pack NuGet package
   - ✅ Publish to NuGet
   - ✅ Upload artifacts

### 5. Verify Publication

1. **Check NuGet.org**:
   - Visit https://www.nuget.org/packages/Gherkin.Generator/
   - New version should appear (may take a few minutes)
   - Verify package details and metadata

### 6. Update Example Test

1. **Update latest template**: Copy latest template into example project
   ```powershell
   Copy-Item templates/Default.mustache tests/Example/Templates/Default.mustache
   ```

2. **Update package version**: Ensure example test has the newly-released version
   ```powershell
   dotnet add tests/Example package Gherkin.Generator
   ```

3. **Run tests**
   ```powershell
   dotnet test tests/Example
   ```

## Version Strategy

According to [PROJECT-PLAN.md](wip/PROJECT-PLAN.md):

- **0.0.1** - Initial MVP release with core functionality
- **0.1.0** - First release that works with YoFi.V3
- **1.0.0** - Stable release when YoFi.V3 is fully migrated
- **Future versions** - Follow [semantic versioning](https://semver.org/):
  - PATCH (0.0.x) - Bug fixes
  - MINOR (0.x.0) - New features (backward compatible)
  - MAJOR (x.0.0) - Breaking changes

## Troubleshooting

### Workflow Fails

1. **Check the logs** in the GitHub Actions tab
2. **Common issues**:
   - Tests failing: Fix tests before releasing
   - Build errors: Ensure code compiles locally
   - NuGet API key invalid: Regenerate and update secret

### Package Already Exists

If you try to publish the same version twice:
- The workflow uses `--skip-duplicate` flag, so it won't fail
- But you should delete the failed release and create a new one with an incremented version
- NuGet.org doesn't allow replacing existing versions

### Wrong Version Published

If you accidentally publish the wrong version:
1. **Unlist** the package on NuGet.org (Settings → Listing)
2. Create a new release with the correct version
3. Cannot delete versions from NuGet.org, only unlist them

## Workflow Configuration

The release workflow is defined in [`.github/workflows/release.yml`](../.github/workflows/release.yml).

Key features:
- Triggered on release publication
- Overrides csproj version with release tag version
- Runs tests before publishing
- Uploads artifacts for verification
- Uses `--skip-duplicate` to handle re-runs

## Updating the Workflow

To modify the release process:
1. Edit [`.github/workflows/release.yml`](../.github/workflows/release.yml)
2. Test locally if possible (e.g., `dotnet pack` command)
3. Commit and push changes
4. Create a test release to verify

## Manual Release (Emergency)

If GitHub Actions is unavailable:

```bash
# Build and test
dotnet build --configuration Release
dotnet test tests/Unit --no-build --configuration Release

# Create package
dotnet pack src/Analyzer --configuration Release -p:Version=0.0.1 --output ./artifacts

# Push to NuGet
dotnet nuget push ./artifacts/*.nupkg --api-key YOUR_API_KEY --source https://api.nuget.org/v3/index.json
```

**Note**: Replace `YOUR_API_KEY` with your actual NuGet API key and `0.0.1` with the target version.
