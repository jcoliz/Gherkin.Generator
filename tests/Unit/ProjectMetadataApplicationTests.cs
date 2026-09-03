using Gherkin;
using Gherkin.Generator.Lib;
using NUnit.Framework;

namespace Gherkin.Generator.Tests.Unit;

/// <summary>
/// Tests for applying project metadata defaults to CRIF.
/// </summary>
[TestFixture]
public class ProjectMetadataApplicationTests
{
    [Test]
    public void Convert_WithProjectMetadata_AppliesNamespaceDefault()
    {
        // Given: A feature without @namespace tag
        var gherkinText = """
            Feature: Shopping Cart
              Scenario: Add item
                Given I have a cart
            """;
        var gherkinDoc = ParseGherkin(gherkinText);

        // And: Project metadata with a default namespace
        var projectMetadata = new ProjectMetadata
        {
            GeneratedNamespace = "MyApp.Tests.Features",
            DefaultTestBase = new GeneratedTestBaseInfo
            {
                ClassName = "FunctionalTestBase",
                Namespace = "MyApp.Tests",
                FullName = "MyApp.Tests.FunctionalTestBase"
            }
        };

        // When: We convert the feature with project metadata
        var converter = new GherkinToCrifConverter(new StepMetadataCollection());
        var crif = converter.Convert(gherkinDoc, "ShoppingCart", projectMetadata);

        // Then: The CRIF should use the default namespace
        Assert.That(crif.Namespace, Is.EqualTo("MyApp.Tests.Features"));
    }

    [Test]
    public void Convert_WithProjectMetadata_AppliesBaseClassDefault()
    {
        // Given: A feature without @baseclass tag
        var gherkinText = """
            Feature: Shopping Cart
              Scenario: Add item
                Given I have a cart
            """;
        var gherkinDoc = ParseGherkin(gherkinText);

        // And: Project metadata with a default base class
        var projectMetadata = new ProjectMetadata
        {
            GeneratedNamespace = "MyApp.Tests.Features",
            DefaultTestBase = new GeneratedTestBaseInfo
            {
                ClassName = "FunctionalTestBase",
                Namespace = "MyApp.Tests",
                FullName = "MyApp.Tests.FunctionalTestBase"
            }
        };

        // When: We convert the feature with project metadata
        var converter = new GherkinToCrifConverter(new StepMetadataCollection());
        var crif = converter.Convert(gherkinDoc, "ShoppingCart", projectMetadata);

        // Then: The CRIF should use the default base class
        Assert.That(crif.BaseClass, Is.EqualTo("FunctionalTestBase"));

        // And: The base class namespace should be added to usings
        Assert.That(crif.Usings, Contains.Item("MyApp.Tests"));
    }

    [Test]
    public void Convert_WithExplicitNamespaceTag_OverridesProjectMetadata()
    {
        // Given: A feature with explicit @namespace tag
        var gherkinText = """
            @namespace:Explicit.Namespace
            Feature: Shopping Cart
              Scenario: Add item
                Given I have a cart
            """;
        var gherkinDoc = ParseGherkin(gherkinText);

        // And: Project metadata with a different default namespace
        var projectMetadata = new ProjectMetadata
        {
            GeneratedNamespace = "MyApp.Tests.Features",
            DefaultTestBase = new GeneratedTestBaseInfo
            {
                ClassName = "FunctionalTestBase",
                Namespace = "MyApp.Tests",
                FullName = "MyApp.Tests.FunctionalTestBase"
            }
        };

        // When: We convert the feature with project metadata
        var converter = new GherkinToCrifConverter(new StepMetadataCollection());
        var crif = converter.Convert(gherkinDoc, "ShoppingCart", projectMetadata);

        // Then: The CRIF should use the explicit namespace, not the default
        Assert.That(crif.Namespace, Is.EqualTo("Explicit.Namespace"));
    }

    [Test]
    public void Convert_WithExplicitBaseClassTag_OverridesProjectMetadata()
    {
        // Given: A feature with explicit @baseclass tag
        var gherkinText = """
            @baseclass:Explicit.Namespace.ExplicitBase
            Feature: Shopping Cart
              Scenario: Add item
                Given I have a cart
            """;
        var gherkinDoc = ParseGherkin(gherkinText);

        // And: Project metadata with a different default base class
        var projectMetadata = new ProjectMetadata
        {
            GeneratedNamespace = "MyApp.Tests.Features",
            DefaultTestBase = new GeneratedTestBaseInfo
            {
                ClassName = "FunctionalTestBase",
                Namespace = "MyApp.Tests",
                FullName = "MyApp.Tests.FunctionalTestBase"
            }
        };

        // When: We convert the feature with project metadata
        var converter = new GherkinToCrifConverter(new StepMetadataCollection());
        var crif = converter.Convert(gherkinDoc, "ShoppingCart", projectMetadata);

        // Then: The CRIF should use the explicit base class, not the default
        Assert.That(crif.BaseClass, Is.EqualTo("ExplicitBase"));

        // And: The explicit namespace should be in usings
        Assert.That(crif.Usings, Contains.Item("Explicit.Namespace"));
    }

    [Test]
    public void Convert_WithoutProjectMetadata_LeavesFieldsEmpty()
    {
        // Given: A feature without any tags
        var gherkinText = """
            Feature: Shopping Cart
              Scenario: Add item
                Given I have a cart
            """;
        var gherkinDoc = ParseGherkin(gherkinText);

        // When: We convert the feature without project metadata
        var converter = new GherkinToCrifConverter(new StepMetadataCollection());
        var crif = converter.Convert(gherkinDoc, "ShoppingCart", null);

        // Then: Namespace and BaseClass should be empty
        Assert.That(crif.Namespace, Is.Empty);
        Assert.That(crif.BaseClass, Is.Empty);
    }

    [Test]
    public void Convert_WithBaseProvidesState_MarksMatchingRequiresAsProvidedByBase()
    {
        // Given: A feature with a matched step
        var gherkinText = """
            Feature: Shopping Cart
              Scenario: Add item
                Given I have a cart
            """;
        var gherkinDoc = ParseGherkin(gherkinText);

        // And: Step metadata with multiple requirements
        var stepMetadata = new StepMetadataCollection();
        stepMetadata.Add(new StepMetadata
        {
            NormalizedKeyword = NormalizedKeyword.Given,
            Text = "I have a cart",
            Method = "GivenIHaveACart",
            Class = "CartSteps",
            Namespace = "MyApp.Tests.Steps",
            Parameters = [],
            RequiresState =
            [
                new SharedStateCrif { Name = "DefaultUser", Description = "default seeded test user" },
                new SharedStateCrif { Name = "SessionToken", Description = "authenticated session" }
            ],
            ProvidesState = []
        });

        // And: Project metadata with one base-provided state
        var projectMetadata = new ProjectMetadata
        {
            BaseProvidesState =
            [
                new SharedStateCrif { Name = "DefaultUser", Description = "default seeded test user" }
            ]
        };

        // When: We convert the feature with project metadata
        var converter = new GherkinToCrifConverter(stepMetadata);
        var crif = converter.Convert(gherkinDoc, "ShoppingCart", projectMetadata);
        var requires = crif.Rules[0].Scenarios[0].Steps[0].RequiresState;

        // Then: Matching requires state should be marked as base-provided
        Assert.That(requires.First(r => r.Name == "DefaultUser").ProvidedByBase, Is.True);

        // And: Non-matching requires state should not be marked
        Assert.That(requires.First(r => r.Name == "SessionToken").ProvidedByBase, Is.False);
    }

    private static Gherkin.Ast.GherkinDocument ParseGherkin(string gherkinText)
    {
        var parser = new Parser();
        var reader = new StringReader(gherkinText);
        return parser.Parse(reader);
    }
}
