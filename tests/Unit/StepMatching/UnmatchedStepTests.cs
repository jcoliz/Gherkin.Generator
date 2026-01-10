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

    [Test]
    public void Convert_ScenarioOutlineWithUnmatchedStep_MarksAsExplicitAndAddsToUnimplemented()
    {
        // Given: An empty step metadata collection
        var stepMetadata = new StepMetadataCollection();
        var converter = new GherkinToCrifConverter(stepMetadata);

        // And: A Gherkin scenario outline with unmatched steps
        var gherkin = """
            Feature: Account Management

            Scenario Outline: Create account with balance
              Given I have <amount> dollars
              When I create an account
              Then the balance should be <amount>

            Examples:
              | amount |
              | 100    |
              | 200    |
            """;
        var feature = GherkinTestHelpers.ParseGherkin(gherkin);

        // When: Feature is converted to CRIF
        var crif = converter.Convert(feature);

        // Then: All steps should be in Unimplemented list
        Assert.That(crif.Unimplemented, Has.Count.EqualTo(3));
        Assert.That(crif.Unimplemented[0].Text, Is.EqualTo("I have <amount> dollars"));
        Assert.That(crif.Unimplemented[1].Text, Is.EqualTo("I create an account"));
        Assert.That(crif.Unimplemented[2].Text, Is.EqualTo("the balance should be <amount>"));

        // And: Scenario should be marked as explicit
        var scenario = crif.Rules[0].Scenarios[0];
        Assert.That(scenario.IsExplicit, Is.True);

        // And: Explicit reason should be "steps_in_progress"
        Assert.That(scenario.ExplicitReason, Is.EqualTo("steps_in_progress"));

        // And: All steps should have Owner="this"
        Assert.That(scenario.Steps[0].Owner, Is.EqualTo("this"));
        Assert.That(scenario.Steps[1].Owner, Is.EqualTo("this"));
        Assert.That(scenario.Steps[2].Owner, Is.EqualTo("this"));

        // And: Steps with placeholders should have arguments matching the parameter
        Assert.That(scenario.Steps[0].Arguments, Has.Count.EqualTo(1), "Step 'I have <amount> dollars' should have 1 argument");
        Assert.That(scenario.Steps[0].Arguments[0].Value, Is.EqualTo("amount"), "Step argument should match parameter name");
        
        // And: Steps without placeholders should have no arguments
        Assert.That(scenario.Steps[1].Arguments, Has.Count.EqualTo(0), "Step 'I create an account' should have no arguments");
        
        // And: Step with placeholder should have argument
        Assert.That(scenario.Steps[2].Arguments, Has.Count.EqualTo(1), "Step 'the balance should be <amount>' should have 1 argument");
        Assert.That(scenario.Steps[2].Arguments[0].Value, Is.EqualTo("amount"), "Step argument should match parameter name");

        // And: Usings should include Gherkin.Generator.Utils namespace
        Assert.That(crif.Usings, Contains.Item("Gherkin.Generator.Utils"));

        // And: Scenario should still have parameters from examples
        Assert.That(scenario.Parameters, Has.Count.EqualTo(1));
        Assert.That(scenario.Parameters[0].Name, Is.EqualTo("amount"));

        // And: Test cases should be generated
        Assert.That(scenario.TestCases, Has.Count.EqualTo(2));
        Assert.That(scenario.TestCases[0], Is.EqualTo("\"100\""));
        Assert.That(scenario.TestCases[1], Is.EqualTo("\"200\""));
    }

    [Test]
    public void Convert_WithUnmatchedStepContainingInteger_DetectsIntegerAsParameter()
    {
        // Given: An empty step metadata collection
        var stepMetadata = new StepMetadataCollection();
        var converter = new GherkinToCrifConverter(stepMetadata);

        // And: A Gherkin feature with a step containing an integer
        var gherkin = """
            Feature: Shopping Cart

            Scenario: Add items to cart
              When I have 12 oranges
            """;
        var feature = GherkinTestHelpers.ParseGherkin(gherkin);

        // When: Feature is converted to CRIF
        var crif = converter.Convert(feature);

        // Then: Step should be in Unimplemented list
        Assert.That(crif.Unimplemented, Has.Count.EqualTo(1));
        Assert.That(crif.Unimplemented[0].Text, Is.EqualTo("I have 12 oranges"));
        Assert.That(crif.Unimplemented[0].Keyword, Is.EqualTo(NormalizedKeyword.When));
        Assert.That(crif.Unimplemented[0].Parameters[0].Type, Is.EqualTo("int"));
        Assert.That(crif.Unimplemented[0].Parameters[0].Name, Is.EqualTo("value1"));

        // And: Step should have detected the integer as a parameter
        var step = crif.Rules[0].Scenarios[0].Steps[0];
        Assert.That(step.Arguments, Has.Count.EqualTo(1), "Integer '12' should be detected as a parameter");
        
        // And: Parameter value should be the integer
        Assert.That(step.Arguments[0].Value, Is.EqualTo("12"));
        
        // And: Method name should be generated without the integer (IHaveOranges)
        Assert.That(step.Method, Is.EqualTo("IHaveOranges"));
    }
}
