using Gherkin.Generator.Lib;

namespace Gherkin.Generator.Tests.Unit;

/// <summary>
/// Tests for handling unmatched steps in Gherkin-to-CRIF conversion.
/// </summary>
[TestFixture]
public class UnmatchedStepTests : StepMatchingTestsBase
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
        var feature = ParseGherkin(gherkin);

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
}
