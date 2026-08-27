using Gherkin.Generator.Lib;

namespace Gherkin.Generator.Tests.Unit;

/// <summary>
/// Tests for <see cref="StepCatalogBuilder"/> Markdown rendering.
/// </summary>
[TestFixture]
public class StepCatalogBuilderTests
{
    private static StepMetadata Step(NormalizedKeyword keyword, string text, string sourceFile, string method = "Method", string cls = "Steps") =>
        new()
        {
            NormalizedKeyword = keyword,
            Text = text,
            Method = method,
            Class = cls,
            Namespace = "Some.Namespace",
            Parameters = [],
            SourceFile = sourceFile
        };

    [Test]
    public void Build_NoSteps_ProducesOnlyHeader()
    {
        // Given: An empty step metadata collection
        var steps = new StepMetadataCollection();

        // When: Building the catalog
        var result = StepCatalogBuilder.Build(steps);

        // Then: Only the top-level header is present
        Assert.That(result.TrimEnd(), Is.EqualTo("# Step Catalog"));
    }

    [Test]
    public void Build_GroupsStepsBySourceFile()
    {
        // Given: Steps declared in two different files
        var steps = new StepMetadataCollection();
        steps.AddRange([
            Step(NormalizedKeyword.Given, "I am logged in", "AuthSteps.cs"),
            Step(NormalizedKeyword.Given, "I have an item", "ManageSteps.cs")
        ]);

        // When: Building the catalog
        var result = StepCatalogBuilder.Build(steps);

        // Then: Both file headings are present
        Assert.That(result, Does.Contain("## AuthSteps.cs"));
        Assert.That(result, Does.Contain("## ManageSteps.cs"));
    }

    [Test]
    public void Build_OrdersFileGroupsAlphabetically()
    {
        // Given: Steps declared in files out of alphabetical order
        var steps = new StepMetadataCollection();
        steps.AddRange([
            Step(NormalizedKeyword.Given, "z step", "ZebraSteps.cs"),
            Step(NormalizedKeyword.Given, "a step", "AlphaSteps.cs")
        ]);

        // When: Building the catalog
        var result = StepCatalogBuilder.Build(steps);

        // Then: AlphaSteps.cs appears before ZebraSteps.cs
        var alphaIndex = result.IndexOf("## AlphaSteps.cs", StringComparison.Ordinal);
        var zebraIndex = result.IndexOf("## ZebraSteps.cs", StringComparison.Ordinal);
        Assert.That(alphaIndex, Is.LessThan(zebraIndex));
    }

    [Test]
    public void Build_OrdersKeywordsGivenWhenThen()
    {
        // Given: Steps of all three keywords declared out of order
        var steps = new StepMetadataCollection();
        steps.AddRange([
            Step(NormalizedKeyword.Then, "the result is shown", "Steps.cs"),
            Step(NormalizedKeyword.When, "I click submit", "Steps.cs"),
            Step(NormalizedKeyword.Given, "I am on the page", "Steps.cs")
        ]);

        // When: Building the catalog
        var result = StepCatalogBuilder.Build(steps);

        // Then: Given appears before When, which appears before Then
        var givenIndex = result.IndexOf("| Given |", StringComparison.Ordinal);
        var whenIndex = result.IndexOf("| When |", StringComparison.Ordinal);
        var thenIndex = result.IndexOf("| Then |", StringComparison.Ordinal);
        Assert.That(givenIndex, Is.LessThan(whenIndex));
        Assert.That(whenIndex, Is.LessThan(thenIndex));
    }

    [Test]
    public void Build_OrdersStepsAlphabeticallyWithinKeyword()
    {
        // Given: Two Given steps out of alphabetical order
        var steps = new StepMetadataCollection();
        steps.AddRange([
            Step(NormalizedKeyword.Given, "zebra exists", "Steps.cs"),
            Step(NormalizedKeyword.Given, "apple exists", "Steps.cs")
        ]);

        // When: Building the catalog
        var result = StepCatalogBuilder.Build(steps);

        // Then: "apple exists" appears before "zebra exists"
        var appleIndex = result.IndexOf("apple exists", StringComparison.Ordinal);
        var zebraIndex = result.IndexOf("zebra exists", StringComparison.Ordinal);
        Assert.That(appleIndex, Is.LessThan(zebraIndex));
    }

    [Test]
    public void Build_AliasedSteps_ProducesSeparateEntries()
    {
        // Given: One method with multiple binding attributes (aliases), as separate StepMetadata entries
        var steps = new StepMetadataCollection();
        steps.AddRange([
            Step(NormalizedKeyword.Given, "user is logged in", "AuthSteps.cs", method: "Login"),
            Step(NormalizedKeyword.Given, "user has logged in", "AuthSteps.cs", method: "Login"),
            Step(NormalizedKeyword.When, "user logs in", "AuthSteps.cs", method: "Login")
        ]);

        // When: Building the catalog
        var result = StepCatalogBuilder.Build(steps);

        // Then: All three phrases appear as separate entries, despite sharing one method
        Assert.That(result, Does.Contain("| Given | user is logged in |"));
        Assert.That(result, Does.Contain("| Given | user has logged in |"));
        Assert.That(result, Does.Contain("| When | user logs in |"));
    }

    [Test]
    public void Build_PreservesParameterPlaceholders()
    {
        // Given: A step with a parameter placeholder
        var steps = new StepMetadataCollection();
        steps.Add(Step(NormalizedKeyword.Given, "I have {count} items", "Steps.cs"));

        // When: Building the catalog
        var result = StepCatalogBuilder.Build(steps);

        // Then: The placeholder is preserved, wrapped in backticks to stand out in the table
        Assert.That(result, Does.Contain("| Given | I have `{count}` items |"));
    }

    [Test]
    public void Build_IncludesImplementingSymbol()
    {
        // Given: A step declared by a specific class and method
        var steps = new StepMetadataCollection();
        steps.Add(Step(NormalizedKeyword.Given, "selected the first {count} items", "ManageSteps.cs", method: "SelectFirstItems", cls: "ManageSteps"));

        // When: Building the catalog
        var result = StepCatalogBuilder.Build(steps);

        // Then: The Type.MethodName symbol is included as the Implementation column
        Assert.That(result, Does.Contain("| Given | selected the first `{count}` items | `ManageSteps.SelectFirstItems` |"));
    }

    [Test]
    public void Build_EachFileGroup_HasOwnTableHeader()
    {
        // Given: Steps declared in two different files
        var steps = new StepMetadataCollection();
        steps.AddRange([
            Step(NormalizedKeyword.Given, "I am logged in", "AuthSteps.cs"),
            Step(NormalizedKeyword.Given, "I have an item", "ManageSteps.cs")
        ]);

        // When: Building the catalog
        var result = StepCatalogBuilder.Build(steps);

        // Then: The table header/separator row appears once per file
        var headerCount = result.Split("| Keyword | Step | Implementation |").Length - 1;
        Assert.That(headerCount, Is.EqualTo(2));
    }
}
