using Gherkin.Generator.Lib;

namespace Gherkin.Generator.Tests.Unit;

/// <summary>
/// Tests for background step handling in Gherkin-to-CRIF conversion.
/// </summary>
[TestFixture]
public class BackgroundStepTests : StepMatchingTestsBase
{
    [Test]
    public void Convert_WithBackground_AddsBackgroundStepClassesAndNamespaces()
    {
        // Given: Background steps
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
            }
        ]);

        var converter = new GherkinToCrifConverter(stepMetadata);

        // And: A Gherkin feature with background
        var gherkin = """
            Feature: Application

            Background:
              Given I am logged in
              And I have a workspace

            Scenario: Do something
              When I do something
            """;
        var feature = ParseGherkin(gherkin);

        // When: Feature is converted to CRIF
        var crif = converter.Convert(feature);

        // Then: Background step classes should be in Classes list
        Assert.That(crif.Classes, Contains.Item("AuthSteps"));
        Assert.That(crif.Classes, Contains.Item("WorkspaceSteps"));

        // And: Background step namespaces should be in Usings
        Assert.That(crif.Usings, Contains.Item("YoFi.V3.Tests.Functional.Steps.Auth"));
        Assert.That(crif.Usings, Contains.Item("YoFi.V3.Tests.Functional.Steps.Setup"));
    }
}
