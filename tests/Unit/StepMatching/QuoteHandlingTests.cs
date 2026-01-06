using Gherkin.Generator.Lib;

namespace Gherkin.Generator.Tests.Unit;

/// <summary>
/// Tests for quote handling in parameter extraction for Gherkin-to-CRIF conversion.
/// </summary>
[TestFixture]
public class QuoteHandlingTests
{
    [Test]
    public void Convert_WithMultipleUnquotedParameters_PreservesQuotesOnAllArguments()
    {
        // Given: A step with multiple string parameters
        var stepMetadata = new StepMetadataCollection();
        stepMetadata.Add(new StepMetadata
        {
            NormalizedKeyword = NormalizedKeyword.Then,
            Text = "I should see {expectedValue} as the {fieldName}",
            Method = "ThenIShouldSeeValueAsField",
            Class = "TransactionDetailsSteps",
            Namespace = "YoFi.V3.Tests.Functional.Steps",
            Parameters = [
                new StepParameter { Type = "string", Name = "expectedValue" },
                new StepParameter { Type = "string", Name = "fieldName" }
            ]
        });

        var converter = new GherkinToCrifConverter(stepMetadata);

        // And: A Gherkin feature with step containing quoted and unquoted values
        var gherkin = """
            Feature: Transaction Details

            Scenario: View transaction field
              Then I should see "Chase Visa" as the Source
            """;
        var feature = GherkinTestHelpers.ParseGherkin(gherkin);

        // When: Feature is converted to CRIF
        var crif = converter.Convert(feature);

        // Then: Step should be matched
        var step = crif.Rules[0].Scenarios[0].Steps[0];
        Assert.That(step.Owner, Is.EqualTo("TransactionDetailsSteps"));
        Assert.That(step.Method, Is.EqualTo("ThenIShouldSeeValueAsField"));

        // And: Both arguments should have quotes preserved
        Assert.That(step.Arguments, Has.Count.EqualTo(2));
        Assert.That(step.Arguments[0].Value, Is.EqualTo("\"Chase Visa\""), "First argument should have quotes preserved");
        Assert.That(step.Arguments[1].Value, Is.EqualTo("\"Source\""), "Second argument should be quoted even if not quoted in Gherkin");
    }
}
