using Gherkin.Generator.Lib;

namespace Gherkin.Generator.Tests.Unit;

/// <summary>
/// Tests for basic step matching functionality in Gherkin-to-CRIF conversion.
/// </summary>
[TestFixture]
public class BasicStepMatchingTests : StepMatchingTestsBase
{
    [Test]
    public void Convert_WithMatchedStep_EmitsOwnerAndMethod()
    {
        // Given: A step metadata collection with a step definition
        var stepMetadata = new StepMetadataCollection();
        stepMetadata.Add(new StepMetadata
        {
            NormalizedKeyword = NormalizedKeyword.Given,
            Text = "I am logged in",
            Method = "IAmLoggedIn",
            Class = "AuthSteps",
            Namespace = "YoFi.V3.Tests.Functional.Steps",
            Parameters = []
        });

        var converter = new GherkinToCrifConverter(stepMetadata);

        // And: A Gherkin feature with that step
        var gherkin = """
            Feature: Authentication

            Rule: Login

            Scenario: User logs in
              Given I am logged in
            """;
        var feature = ParseGherkin(gherkin);

        // When: Feature is converted to CRIF
        var crif = converter.Convert(feature);

        // Then: Step should have Owner and Method from matched step
        var step = crif.Rules[0].Scenarios[0].Steps[0];
        Assert.That(step.Owner, Is.EqualTo("AuthSteps"));
        Assert.That(step.Method, Is.EqualTo("IAmLoggedIn"));
    }

    [Test]
    public void Convert_WithMatchedStep_AddsClassToClassesList()
    {
        // Given: A step metadata collection
        var stepMetadata = new StepMetadataCollection();
        stepMetadata.Add(new StepMetadata
        {
            NormalizedKeyword = NormalizedKeyword.Given,
            Text = "I am logged in",
            Method = "IAmLoggedIn",
            Class = "AuthSteps",
            Namespace = "YoFi.V3.Tests.Functional.Steps",
            Parameters = []
        });

        var converter = new GherkinToCrifConverter(stepMetadata);

        // And: A Gherkin feature
        var gherkin = """
            Feature: Authentication

            Scenario: User logs in
              Given I am logged in
            """;
        var feature = ParseGherkin(gherkin);

        // When: Feature is converted to CRIF
        var crif = converter.Convert(feature);

        // Then: Class should be added to Classes list
        Assert.That(crif.Classes, Contains.Item("AuthSteps"));
    }

    [Test]
    public void Convert_WithMatchedStep_AddsNamespaceToUsings()
    {
        // Given: A step metadata collection
        var stepMetadata = new StepMetadataCollection();
        stepMetadata.Add(new StepMetadata
        {
            NormalizedKeyword = NormalizedKeyword.Given,
            Text = "I am logged in",
            Method = "IAmLoggedIn",
            Class = "AuthSteps",
            Namespace = "YoFi.V3.Tests.Functional.Steps",
            Parameters = []
        });

        var converter = new GherkinToCrifConverter(stepMetadata);

        // And: A Gherkin feature
        var gherkin = """
            Feature: Authentication

            Scenario: User logs in
              Given I am logged in
            """;
        var feature = ParseGherkin(gherkin);

        // When: Feature is converted to CRIF
        var crif = converter.Convert(feature);

        // Then: Namespace should be added to Usings
        Assert.That(crif.Usings, Contains.Item("YoFi.V3.Tests.Functional.Steps"));
    }
}
