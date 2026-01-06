namespace Gherkin.Generator.Tests.Unit;

/// <summary>
/// Tests for error handling in Gherkin-to-CRIF conversion.
/// </summary>
[TestFixture]
public class ErrorHandlingTests : StepMatchingTestsBase
{
    [Test]
    public void Convert_WithMalformedUsingSyntax_ThrowsCompositeParserException()
    {
        // Given: A Gherkin feature with malformed @using syntax (space instead of colon)
        var gherkin = """
            @using Namespace
            Feature: Authentication

            Scenario: User logs in
              Given I am logged in
            """;

        // When: Attempting to parse the malformed Gherkin
        var ex = Assert.Throws<Gherkin.CompositeParserException>(() => ParseGherkin(gherkin));

        // Then: Exception should indicate tag whitespace error
        Assert.That(ex!.Message, Does.Contain("A tag may not contain whitespace"));

        // And: Exception should include line number reference
        Assert.That(ex.Message, Does.Contain("(1:1)"));
    }
}
