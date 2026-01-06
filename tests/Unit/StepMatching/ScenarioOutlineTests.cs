using Gherkin.Generator.Lib;

namespace Gherkin.Generator.Tests.Unit;

/// <summary>
/// Tests for scenario outline handling in Gherkin-to-CRIF conversion.
/// </summary>
[TestFixture]
public class ScenarioOutlineTests
{
    [Test]
    public void Convert_ScenarioOutlineWithMatchedStep_ExtractsParametersAndMatchesSteps()
    {
        // Given: A step with parameter matching
        var stepMetadata = new StepMetadataCollection();
        stepMetadata.Add(new StepMetadata
        {
            NormalizedKeyword = NormalizedKeyword.Given,
            Text = "I have {amount} dollars",
            Method = "IHaveDollars",
            Class = "AccountSteps",
            Namespace = "YoFi.V3.Tests.Functional.Steps",
            Parameters = [new StepParameter { Type = "int", Name = "amount" }]
        });

        var converter = new GherkinToCrifConverter(stepMetadata);

        // And: A Gherkin scenario outline with examples
        var gherkin = """
            Feature: Account Management

            Rule: Account Balance

            Scenario Outline: Create account with balance
              Given I have <amount> dollars

            Examples:
              | amount |
              | 100    |
              | 200    |
            """;
        var feature = GherkinTestHelpers.ParseGherkin(gherkin);

        // When: Feature is converted to CRIF
        var crif = converter.Convert(feature);

        // Then: Scenario should have parameters from examples
        var scenario = crif.Rules[0].Scenarios[0];
        Assert.That(scenario.Parameters, Has.Count.EqualTo(1));
        Assert.That(scenario.Parameters[0].Name, Is.EqualTo("amount"));

        // And: Test cases should be generated
        Assert.That(scenario.TestCases, Has.Count.EqualTo(2));
        Assert.That(scenario.TestCases[0], Is.EqualTo("\"100\""));
        Assert.That(scenario.TestCases[1], Is.EqualTo("\"200\""));

        // And: Step should be matched with Owner and Method
        var step = crif.Rules[0].Scenarios[0].Steps[0];
        Assert.That(step.Owner, Is.EqualTo("AccountSteps"));
        Assert.That(step.Method, Is.EqualTo("IHaveDollars"));

        // And: Step should have parameterized argument (not concrete value)
        Assert.That(step.Arguments, Has.Count.EqualTo(1));
        Assert.That(step.Arguments[0].Value, Is.EqualTo("amount"));

        // And: Class should be added to Classes list
        Assert.That(crif.Classes, Contains.Item("AccountSteps"));

        // And: Namespace should be added to Usings
        Assert.That(crif.Usings, Contains.Item("YoFi.V3.Tests.Functional.Steps"));
    }

    [Test]
    public void Convert_ScenarioOutlineWithMultipleParameters_MatchesStepAndExtractsAllParameters()
    {
        // Given: A step with multiple parameters
        var stepMetadata = new StepMetadataCollection();
        stepMetadata.Add(new StepMetadata
        {
            NormalizedKeyword = NormalizedKeyword.Given,
            Text = "I have {amount} dollars in {account}",
            Method = "IHaveDollarsIn",
            Class = "AccountSteps",
            Namespace = "YoFi.V3.Tests.Functional.Steps",
            Parameters = [
                new StepParameter { Type = "int", Name = "amount" },
                new StepParameter { Type = "string", Name = "account" }
            ]
        });

        var converter = new GherkinToCrifConverter(stepMetadata);

        // And: A Gherkin scenario outline with multiple parameters
        var gherkin = """
            Feature: Account Management

            Rule: Multi-Account Operations

            Scenario Outline: Transfer between accounts
              Given I have <amount> dollars in <account>

            Examples:
              | amount | account  |
              | 100    | Savings  |
              | 200    | Checking |
            """;
        var feature = GherkinTestHelpers.ParseGherkin(gherkin);

        // When: Feature is converted to CRIF
        var crif = converter.Convert(feature);

        // Then: Scenario should have all parameters
        var scenario = crif.Rules[0].Scenarios[0];
        Assert.That(scenario.Parameters, Has.Count.EqualTo(2));
        Assert.That(scenario.Parameters[0].Name, Is.EqualTo("amount"));
        Assert.That(scenario.Parameters[1].Name, Is.EqualTo("account"));

        // And: Test cases should include all parameter values
        Assert.That(scenario.TestCases, Has.Count.EqualTo(2));
        Assert.That(scenario.TestCases[0], Is.EqualTo("\"100\", \"Savings\""));
        Assert.That(scenario.TestCases[1], Is.EqualTo("\"200\", \"Checking\""));

        // And: Step should be matched
        var step = crif.Rules[0].Scenarios[0].Steps[0];
        Assert.That(step.Owner, Is.EqualTo("AccountSteps"));
        Assert.That(step.Method, Is.EqualTo("IHaveDollarsIn"));

        // And: Step should have parameterized arguments
        Assert.That(step.Arguments, Has.Count.EqualTo(2));
        Assert.That(step.Arguments[0].Value, Is.EqualTo("amount"));
        Assert.That(step.Arguments[0].Last, Is.False);
        Assert.That(step.Arguments[1].Value, Is.EqualTo("account"));
        Assert.That(step.Arguments[1].Last, Is.True);
    }
}
