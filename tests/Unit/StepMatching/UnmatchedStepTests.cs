using Gherkin.Generator.Lib;

namespace Gherkin.Generator.Tests.Unit;

/// <summary>
/// Tests for handling unmatched steps in Gherkin-to-CRIF conversion.
/// </summary>
[TestFixture]
public class UnmatchedStepTests
{
    [Test]
    public void Convert_WithUnmatchedStep_StaysInUnimplementedList()
    {
        // Given: An empty step metadata collection
        var stepMetadata = new StepMetadataCollection();
        var converter = new GherkinToCrifConverter(stepMetadata);

        // And: A Gherkin feature
        var gherkin = """
            Feature: Authentication

            Scenario: User logs in
              Given I am logged in
            """;
        var feature = GherkinTestHelpers.ParseGherkin(gherkin);

        // When: Feature is converted to CRIF
        var crif = converter.Convert(feature);

        // Then: Step should be in Unimplemented list
        Assert.That(crif.Unimplemented, Has.Count.EqualTo(1));
        Assert.That(crif.Unimplemented[0].Text, Is.EqualTo("I am logged in"));
        Assert.That(crif.Unimplemented[0].Keyword, Is.EqualTo(NormalizedKeyword.Given));

        // And: Step should have Owner="this"
        var step = crif.Rules[0].Scenarios[0].Steps[0];
        Assert.That(step.Owner, Is.EqualTo("this"));
    }

    [Test]
    public void Convert_WithUnmatchedStep_MarksScenarioAsExplicit()
    {
        // Given: An empty step metadata collection
        var stepMetadata = new StepMetadataCollection();
        var converter = new GherkinToCrifConverter(stepMetadata);

        // And: A Gherkin feature with an unmatched step
        var gherkin = """
            Feature: Authentication

            Scenario: User logs in
              Given I am logged in
            """;
        var feature = GherkinTestHelpers.ParseGherkin(gherkin);

        // When: Feature is converted to CRIF
        var crif = converter.Convert(feature);

        // Then: Scenario should be marked as explicit
        var scenario = crif.Rules[0].Scenarios[0];
        Assert.That(scenario.IsExplicit, Is.True);

        // And: Explicit reason should be "steps_in_progress"
        Assert.That(scenario.ExplicitReason, Is.EqualTo("steps_in_progress"));
    }

    [Test]
    public void Convert_WithUnmatchedStepAndExistingExplicitTag_KeepsOriginalExplicitStatus()
    {
        // Given: An empty step metadata collection
        var stepMetadata = new StepMetadataCollection();
        var converter = new GherkinToCrifConverter(stepMetadata);

        // And: A Gherkin feature with an unmatched step and @explicit tag (no reason)
        var gherkin = """
            Feature: Authentication

            @explicit
            Scenario: User logs in
              Given I am logged in
            """;
        var feature = GherkinTestHelpers.ParseGherkin(gherkin);

        // When: Feature is converted to CRIF
        var crif = converter.Convert(feature);

        // Then: Scenario should remain marked as explicit
        var scenario = crif.Rules[0].Scenarios[0];
        Assert.That(scenario.IsExplicit, Is.True);

        // And: Explicit reason should remain null (not overwritten with "steps_in_progress")
        Assert.That(scenario.ExplicitReason, Is.Null);
    }

    [Test]
    public void Convert_WithUnmatchedSteps_IncludesUtilsNamespaceInUsings()
    {
        // Given: An empty step metadata collection
        var stepMetadata = new StepMetadataCollection();
        var converter = new GherkinToCrifConverter(stepMetadata);

        // And: A Gherkin feature with unmatched steps
        var gherkin = """
            Feature: Shopping Cart

            Scenario: Add item to cart
              Given I have an empty cart
              When I add an item
              Then the cart should contain 1 item
            """;
        var feature = GherkinTestHelpers.ParseGherkin(gherkin);

        // When: Feature is converted to CRIF
        var crif = converter.Convert(feature);

        // Then: Usings should include Gherkin.Generator.Utils namespace
        Assert.That(crif.Usings, Contains.Item("Gherkin.Generator.Utils"));

        // And: All three steps should be in Unimplemented list
        Assert.That(crif.Unimplemented, Has.Count.EqualTo(3));
    }

    [Test]
    public void Convert_WithUnmatchedStepsAndDataTable_IncludesUtilsNamespaceOnlyOnce()
    {
        // Given: An empty step metadata collection
        var stepMetadata = new StepMetadataCollection();
        var converter = new GherkinToCrifConverter(stepMetadata);

        // And: A Gherkin feature with unmatched steps and a DataTable
        var gherkin = """
            Feature: Shopping Cart

            Scenario: Add item with details
              Given I have an empty cart
              When I add an item with the following details:
                | Field | Value |
                | Name  | Apple |
                | Price | 1.50  |
              Then the cart should contain 1 item
            """;
        var feature = GherkinTestHelpers.ParseGherkin(gherkin);

        // When: Feature is converted to CRIF
        var crif = converter.Convert(feature);

        // Then: Usings should include Gherkin.Generator.Utils namespace exactly once
        var utilsCount = crif.Usings.Count(ns => ns == "Gherkin.Generator.Utils");
        Assert.That(utilsCount, Is.EqualTo(1), "Utils namespace should appear exactly once");

        // And: All three steps should be in Unimplemented list
        Assert.That(crif.Unimplemented, Has.Count.EqualTo(3));
    }
}
