using Gherkin.Generator.Lib;

namespace Gherkin.Generator.Tests.Unit;

/// <summary>
/// Tests for scenarios with mixed matched and unmatched steps in Gherkin-to-CRIF conversion.
/// </summary>
[TestFixture]
public class MixedMatchedAndUnmatchedStepTests
{
    [Test]
    public void Convert_WithMatchedAndUnmatchedSteps_HandlesCorrectly()
    {
        // Given: Partial step metadata
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

        // And: A Gherkin feature with matched and unmatched steps
        var gherkin = """
            Feature: Mixed

            Scenario: Partial implementation
              Given I am logged in
              When I do something unimplemented
            """;
        var feature = GherkinTestHelpers.ParseGherkin(gherkin);

        // When: Feature is converted to CRIF
        var crif = converter.Convert(feature);

        // Then: Matched step should use step class
        var step1 = crif.Rules[0].Scenarios[0].Steps[0];
        Assert.That(step1.Owner, Is.EqualTo("AuthSteps"));
        Assert.That(step1.Method, Is.EqualTo("IAmLoggedIn"));

        // And: Unmatched step should use "this"
        var step2 = crif.Rules[0].Scenarios[0].Steps[1];
        Assert.That(step2.Owner, Is.EqualTo("this"));

        // And: Only unmatched step in Unimplemented list
        Assert.That(crif.Unimplemented, Has.Count.EqualTo(1));
        Assert.That(crif.Unimplemented[0].Text, Is.EqualTo("I do something unimplemented"));
    }

    [Test]
    public void Convert_WithStepsFromDifferentNamespaces_AddsAllNamespaces()
    {
        // Given: Steps from different namespaces
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
                Text = "I navigate to the page",
                Method = "INavigateToThePage",
                Class = "NavigationSteps",
                Namespace = "YoFi.V3.Tests.Functional.Pages",
                Parameters = []
            }
        ]);

        var converter = new GherkinToCrifConverter(stepMetadata);

        // And: A Gherkin feature
        var gherkin = """
            Feature: Application

            Scenario: User workflow
              Given I am logged in
              When I navigate to the page
            """;
        var feature = GherkinTestHelpers.ParseGherkin(gherkin);

        // When: Feature is converted to CRIF
        var crif = converter.Convert(feature);

        // Then: Both namespaces should be added to Usings
        Assert.That(crif.Usings, Contains.Item("YoFi.V3.Tests.Functional.Steps"));
        Assert.That(crif.Usings, Contains.Item("YoFi.V3.Tests.Functional.Pages"));
    }

    [Test]
    public void Convert_WithMultipleStepsFromSameNamespace_AddsNamespaceOnce()
    {
        // Given: Multiple steps from same namespace
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

        // And: A Gherkin feature with both steps
        var gherkin = """
            Feature: Application

            Scenario: User workflow
              Given I am logged in
              When I create a transaction
            """;
        var feature = GherkinTestHelpers.ParseGherkin(gherkin);

        // When: Feature is converted to CRIF
        var crif = converter.Convert(feature);

        // Then: Namespace should appear only once in Usings
        Assert.That(crif.Usings.Count(ns => ns == "YoFi.V3.Tests.Functional.Steps"), Is.EqualTo(1));
    }
}
