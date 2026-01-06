using Gherkin.Generator.Lib;

namespace Gherkin.Generator.Tests.Unit;

/// <summary>
/// Tests for And step handling in Gherkin-to-CRIF conversion.
/// </summary>
[TestFixture]
public class AndStepTests : StepMatchingTestsBase
{
    [Test]
    public void Convert_WithAndStep_AddsClassAndNamespaceCorrectly()
    {
        // Given: Steps with Given keyword
        var stepMetadata = new StepMetadataCollection();
        stepMetadata.AddRange([
            new StepMetadata
            {
                NormalizedKeyword = NormalizedKeyword.Given,
                Text = "I am logged in",
                Method = "IAmLoggedIn",
                Class = "AuthSteps",
                Namespace = "YoFi.V3.Tests.Functional.Steps",
                Parameters = []
            },
            new StepMetadata
            {
                NormalizedKeyword = NormalizedKeyword.Given,
                Text = "I have a workspace",
                Method = "IHaveAWorkspace",
                Class = "WorkspaceSteps",
                Namespace = "YoFi.V3.Tests.Functional.Steps.Setup",
                Parameters = []
            }
        ]);

        var converter = new GherkinToCrifConverter(stepMetadata);

        // And: A Gherkin feature with And step
        var gherkin = """
            Feature: Setup

            Scenario: Initial setup
              Given I am logged in
              And I have a workspace
            """;
        var feature = ParseGherkin(gherkin);

        // When: Feature is converted to CRIF
        var crif = converter.Convert(feature);

        // Then: Both classes should be added
        Assert.That(crif.Classes, Contains.Item("AuthSteps"));
        Assert.That(crif.Classes, Contains.Item("WorkspaceSteps"));

        // And: Both namespaces should be added
        Assert.That(crif.Usings, Contains.Item("YoFi.V3.Tests.Functional.Steps"));
        Assert.That(crif.Usings, Contains.Item("YoFi.V3.Tests.Functional.Steps.Setup"));
    }

    [Test]
    public void Convert_WithMultipleAndSteps_AddsAllClassesAndNamespaces()
    {
        // Given: Multiple Given steps from different classes/namespaces
        var stepMetadata = new StepMetadataCollection();
        stepMetadata.AddRange([
            new StepMetadata
            {
                NormalizedKeyword = NormalizedKeyword.Given,
                Text = "I am logged in",
                Method = "IAmLoggedIn",
                Class = "AuthSteps",
                Namespace = "YoFi.V3.Tests.Functional.Steps.Auth",
                Parameters = []
            },
            new StepMetadata
            {
                NormalizedKeyword = NormalizedKeyword.Given,
                Text = "I have a workspace",
                Method = "IHaveAWorkspace",
                Class = "WorkspaceSteps",
                Namespace = "YoFi.V3.Tests.Functional.Steps.Setup",
                Parameters = []
            },
            new StepMetadata
            {
                NormalizedKeyword = NormalizedKeyword.Given,
                Text = "I have permissions",
                Method = "IHavePermissions",
                Class = "PermissionSteps",
                Namespace = "YoFi.V3.Tests.Functional.Steps.Auth",
                Parameters = []
            }
        ]);

        var converter = new GherkinToCrifConverter(stepMetadata);

        // And: A Gherkin feature with multiple And steps
        var gherkin = """
            Feature: Setup

            Scenario: Complete setup
              Given I am logged in
              And I have a workspace
              And I have permissions
            """;
        var feature = ParseGherkin(gherkin);

        // When: Feature is converted to CRIF
        var crif = converter.Convert(feature);

        // Then: All three classes should be added
        Assert.That(crif.Classes, Contains.Item("AuthSteps"));
        Assert.That(crif.Classes, Contains.Item("WorkspaceSteps"));
        Assert.That(crif.Classes, Contains.Item("PermissionSteps"));

        // And: Both unique namespaces should be added (Auth namespace used twice, but only appears once)
        Assert.That(crif.Usings, Contains.Item("YoFi.V3.Tests.Functional.Steps.Auth"));
        Assert.That(crif.Usings, Contains.Item("YoFi.V3.Tests.Functional.Steps.Setup"));
        Assert.That(crif.Usings.Count(ns => ns == "YoFi.V3.Tests.Functional.Steps.Auth"), Is.EqualTo(1));
    }
}
