using Gherkin.Generator.Lib;

namespace Gherkin.Generator.Tests.Unit;

/// <summary>
/// Tests for class and namespace management in Gherkin-to-CRIF conversion.
/// </summary>
[TestFixture]
public class ClassAndNamespaceManagementTests : StepMatchingTestsBase
{
    [Test]
    public void Convert_WithMultipleStepsFromSameClass_AddsClassOnce()
    {
        // Given: Multiple steps from same class
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
                NormalizedKeyword = NormalizedKeyword.When,
                Text = "I log out",
                Method = "ILogOut",
                Class = "AuthSteps",
                Namespace = "YoFi.V3.Tests.Functional.Steps",
                Parameters = []
            }
        ]);

        var converter = new GherkinToCrifConverter(stepMetadata);

        // And: A Gherkin feature with both steps
        var gherkin = """
            Feature: Authentication

            Scenario: User session
              Given I am logged in
              When I log out
            """;
        var feature = ParseGherkin(gherkin);

        // When: Feature is converted to CRIF
        var crif = converter.Convert(feature);

        // Then: Class should appear only once
        Assert.That(crif.Classes.Count(c => c == "AuthSteps"), Is.EqualTo(1));
    }

    [Test]
    public void Convert_WithStepsFromDifferentClasses_AddsAllClasses()
    {
        // Given: Steps from different classes
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
                NormalizedKeyword = NormalizedKeyword.When,
                Text = "I create a transaction",
                Method = "ICreateATransaction",
                Class = "TransactionSteps",
                Namespace = "YoFi.V3.Tests.Functional.Steps",
                Parameters = []
            }
        ]);

        var converter = new GherkinToCrifConverter(stepMetadata);

        // And: A Gherkin feature
        var gherkin = """
            Feature: Application

            Scenario: User workflow
              Given I am logged in
              When I create a transaction
            """;
        var feature = ParseGherkin(gherkin);

        // When: Feature is converted to CRIF
        var crif = converter.Convert(feature);

        // Then: Both classes should be added
        Assert.That(crif.Classes, Contains.Item("AuthSteps"));
        Assert.That(crif.Classes, Contains.Item("TransactionSteps"));
    }
}
