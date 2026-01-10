# Default.mustache Template

This file is automatically installed and updated by the Gherkin.Generator NuGet package.

## ⚠️ Important: This File Is Always Overwritten

**Default.mustache is automatically overwritten whenever you install or upgrade the Gherkin.Generator package.** This ensures the template version always matches the generator version for compatibility.

### If You Need to Customize the Template

**Do not modify Default.mustache directly.** Instead:

1. **Copy** `Default.mustache` to a new file with a different name:
   ```
   Templates/
   ├── Default.mustache      ← Always overwritten (do not modify)
   └── Custom.mustache       ← Your customized version
   ```

2. **Reference** your custom template in your `.csproj` file:
   ```xml
   <ItemGroup>
     <AdditionalFiles Include="Features\*.feature" />
     <AdditionalFiles Include="Templates\Custom.mustache" />
   </ItemGroup>
   ```

3. **Modify** your custom template as needed for your test framework or conventions.

## Template Purpose

This Mustache template controls how the source generator converts Gherkin feature files into C# test code. The default template generates NUnit test classes, but you can customize it for other frameworks (xUnit, MSTest, etc.) or to match your project's coding conventions.

## More Information

For detailed information about:
- Template customization and available variables
- Writing feature files and step definitions
- Advanced features and troubleshooting

See the [User Guide](https://github.com/jcoliz/Gherkin.Generator/blob/main/docs/USER-GUIDE.md#customizing-templates).

## Version Matching

The template version is synchronized with the Gherkin.Generator package version. This ensures compatibility between the generator code and the template structure. If you encounter issues after upgrading, verify that you're using the latest template (which was automatically updated).
