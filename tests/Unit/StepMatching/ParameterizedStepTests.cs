using Gherkin.Generator.Lib;

namespace Gherkin.Generator.Tests.Unit;

/// <summary>
/// Tests for parameterized step matching in Gherkin-to-CRIF conversion.
/// </summary>
[TestFixture]
public class ParameterizedStepTests : StepMatchingTestsBase
{
    [Test]
    public void Convert_WithParameterizedStep_ExtractsArguments()
    {
        // Given: A step with parameters
        var stepMetadata = new StepMetadataCollection();
        stepMetadata.Add(new StepMetadata
        {
            NormalizedKeyword = NormalizedKeyword.Given,
            Text = "I have {quantity} dollars",
            Method = "IHaveDollars",
            Class = "AccountSteps",
            Namespace = "YoFi.V3.Tests.Functional.Steps",
            Parameters = [new StepParameter { Type = "int", Name = "quantity" }]
        });

        var converter = new GherkinToCrifConverter(stepMetadata);

        // And: A Gherkin feature with that step
        var gherkin = """
            Feature: Accounts

            Scenario: User has money
              Given I have 100 dollars
            """;
        var feature = ParseGherkin(gherkin);

        // When: Feature is converted to CRIF
        var crif = converter.Convert(feature);

        // Then: Step should have arguments extracted
        var step = crif.Rules[0].Scenarios[0].Steps[0];
        Assert.That(step.Arguments, Has.Count.EqualTo(1));
        Assert.That(step.Arguments[0].Value, Is.EqualTo("100"));
        Assert.That(step.Arguments[0].Last, Is.True);
    }

    [Test]
    public void Convert_WithMultipleParameters_ExtractsAllArguments()
    {
        // Given: A step with multiple parameters
        var stepMetadata = new StepMetadataCollection();
        stepMetadata.Add(new StepMetadata
        {
            NormalizedKeyword = NormalizedKeyword.Given,
            Text = "I have {quantity} dollars in {account}",
            Method = "IHaveDollarsIn",
            Class = "AccountSteps",
            Namespace = "YoFi.V3.Tests.Functional.Steps",
            Parameters = [
                new StepParameter { Type = "int", Name = "quantity" },
                new StepParameter { Type = "string", Name = "account" }
            ]
        });

        var converter = new GherkinToCrifConverter(stepMetadata);

        // And: A Gherkin feature
        var gherkin = """
            Feature: Accounts

            Scenario: User has money
              Given I have 100 dollars in Savings
            """;
        var feature = ParseGherkin(gherkin);

        // When: Feature is converted to CRIF
        var crif = converter.Convert(feature);

        // Then: Step should have all arguments
        var step = crif.Rules[0].Scenarios[0].Steps[0];
        Assert.That(step.Arguments, Has.Count.EqualTo(2));
        Assert.That(step.Arguments[0].Value, Is.EqualTo("100"));
        Assert.That(step.Arguments[0].Last, Is.False);
        Assert.That(step.Arguments[1].Value, Is.EqualTo("\"Savings\""));
        Assert.That(step.Arguments[1].Last, Is.True);
    }

    [Test]
    public void Convert_WithQuotedArgument_ExtractsWithoutQuotes()
    {
        // Given: A step with a parameter
        var stepMetadata = new StepMetadataCollection();
        stepMetadata.Add(new StepMetadata
        {
            NormalizedKeyword = NormalizedKeyword.Given,
            Text = "I have an account named {account}",
            Method = "IHaveAnAccountNamed",
            Class = "AccountSteps",
            Namespace = "YoFi.V3.Tests.Functional.Steps",
            Parameters = [new StepParameter { Type = "string", Name = "account" }]
        });

        var converter = new GherkinToCrifConverter(stepMetadata);

        // And: A Gherkin feature with quoted value
        var gherkin = """
            Feature: Accounts

            Scenario: User has account
              Given I have an account named "Ski Village"
            """;
        var feature = ParseGherkin(gherkin);

        // When: Feature is converted to CRIF
        var crif = converter.Convert(feature);

        // Then: Argument should be extracted without quotes
        var step = crif.Rules[0].Scenarios[0].Steps[0];
        Assert.That(step.Arguments, Has.Count.EqualTo(1));
        Assert.That(step.Arguments[0].Value, Is.EqualTo("\"Ski Village\""));
    }
}
