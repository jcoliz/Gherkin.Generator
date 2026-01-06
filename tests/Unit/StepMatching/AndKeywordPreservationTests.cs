using Gherkin.Generator.Lib;

namespace Gherkin.Generator.Tests.Unit;

/// <summary>
/// Tests for And keyword preservation in Gherkin-to-CRIF conversion.
/// </summary>
[TestFixture]
public class AndKeywordPreservationTests : StepMatchingTestsBase
{
    [Test]
    public void Convert_WithAndStep_PreservesAndKeywordInCrif()
    {
        // Given: Steps with Given keyword
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
                NormalizedKeyword = NormalizedKeyword.Given,
                Text = "I have a workspace",
                Method = "IHaveAWorkspace",
                Class = "WorkspaceSteps",
                Namespace = "YoFi.V3.Tests.Functional.Steps.Setup",
                Parameters = []
            }
        ]);

        var converter = new GherkinToCrifConverter(stepMetadata);

        // And: A Gherkin feature with And step
        var gherkin = """
            Feature: Setup

            Scenario: Initial setup
              Given I am logged in
              And I have a workspace
            """;
        var feature = ParseGherkin(gherkin);

        // When: Feature is converted to CRIF
        var crif = converter.Convert(feature);

        // Then: First step should have "Given" keyword
        var step1 = crif.Rules[0].Scenarios[0].Steps[0];
        Assert.That(step1.Keyword, Is.EqualTo(DisplayKeyword.Given));

        // And: Second step should preserve "And" keyword in CRIF
        var step2 = crif.Rules[0].Scenarios[0].Steps[1];
        Assert.That(step2.Keyword, Is.EqualTo(DisplayKeyword.And));

        // And: Both steps should be matched correctly
        Assert.That(step1.Owner, Is.EqualTo("AuthSteps"));
        Assert.That(step2.Owner, Is.EqualTo("WorkspaceSteps"));
    }

    [Test]
    public void Convert_WithThenAndAndSteps_PreservesAndKeywordsAndMatchesCorrectly()
    {
        // Given: Steps with Then keyword
        var stepMetadata = new StepMetadataCollection();
        stepMetadata.AddRange([
            new StepMetadata
            {
                NormalizedKeyword = NormalizedKeyword.Then,
                Text = "the user should be logged in",
                Method = "TheUserShouldBeLoggedIn",
                Class = "AuthSteps",
                Namespace = "YoFi.V3.Tests.Functional.Steps",
                Parameters = []
            },
            new StepMetadata
            {
                NormalizedKeyword = NormalizedKeyword.Then,
                Text = "the session should be active",
                Method = "TheSessionShouldBeActive",
                Class = "SessionSteps",
                Namespace = "YoFi.V3.Tests.Functional.Steps",
                Parameters = []
            },
            new StepMetadata
            {
                NormalizedKeyword = NormalizedKeyword.Then,
                Text = "the user should have access",
                Method = "TheUserShouldHaveAccess",
                Class = "AccessSteps",
                Namespace = "YoFi.V3.Tests.Functional.Steps",
                Parameters = []
            }
        ]);

        var converter = new GherkinToCrifConverter(stepMetadata);

        // And: A Gherkin feature with Then/And/And steps
        var gherkin = """
            Feature: Authentication

            Scenario: User verification
              Given I am on the login page
              When I log in
              Then the user should be logged in
              And the session should be active
              And the user should have access
            """;
        var feature = ParseGherkin(gherkin);

        // When: Feature is converted to CRIF
        var crif = converter.Convert(feature);

        // Then: First assertion step should have "Then" keyword
        var step1 = crif.Rules[0].Scenarios[0].Steps[2];
        Assert.That(step1.Keyword, Is.EqualTo(DisplayKeyword.Then));
        Assert.That(step1.Owner, Is.EqualTo("AuthSteps"));
        Assert.That(step1.Method, Is.EqualTo("TheUserShouldBeLoggedIn"));

        // And: Second step should preserve "And" keyword but match as "Then"
        var step2 = crif.Rules[0].Scenarios[0].Steps[3];
        Assert.That(step2.Keyword, Is.EqualTo(DisplayKeyword.And));
        Assert.That(step2.Owner, Is.EqualTo("SessionSteps"));
        Assert.That(step2.Method, Is.EqualTo("TheSessionShouldBeActive"));

        // And: Third step should also preserve "And" keyword and match as "Then"
        var step3 = crif.Rules[0].Scenarios[0].Steps[4];
        Assert.That(step3.Keyword, Is.EqualTo(DisplayKeyword.And));
        Assert.That(step3.Owner, Is.EqualTo("AccessSteps"));
        Assert.That(step3.Method, Is.EqualTo("TheUserShouldHaveAccess"));

        // And: All three classes should be in the classes list
        Assert.That(crif.Classes, Contains.Item("AuthSteps"));
        Assert.That(crif.Classes, Contains.Item("SessionSteps"));
        Assert.That(crif.Classes, Contains.Item("AccessSteps"));
    }

    [Test]
    public void Convert_WithMixedMatchedAndUnmatchedSteps_IncludesAllStepsInCrif()
    {
        // Given: Only Then step definitions
        var stepMetadata = new StepMetadataCollection();
        stepMetadata.AddRange([
            new StepMetadata
            {
                NormalizedKeyword = NormalizedKeyword.Then,
                Text = "the user should be logged in",
                Method = "TheUserShouldBeLoggedIn",
                Class = "AuthSteps",
                Namespace = "YoFi.V3.Tests.Functional.Steps",
                Parameters = []
            },
            new StepMetadata
            {
                NormalizedKeyword = NormalizedKeyword.Then,
                Text = "the session should be active",
                Method = "TheSessionShouldBeActive",
                Class = "SessionSteps",
                Namespace = "YoFi.V3.Tests.Functional.Steps",
                Parameters = []
            }
        ]);

        var converter = new GherkinToCrifConverter(stepMetadata);

        // And: A Gherkin feature with Given/When/Then/And steps
        var gherkin = """
            Feature: Authentication

            Scenario: User verification
              Given I am on the login page
              When I log in
              Then the user should be logged in
              And the session should be active
            """;
        var feature = ParseGherkin(gherkin);

        // When: Feature is converted to CRIF
        var crif = converter.Convert(feature);

        // Then: All 4 steps should be in the CRIF
        Assert.That(crif.Rules[0].Scenarios[0].Steps, Has.Count.EqualTo(4));

        // And: First Given step should be unimplemented
        var givenStep = crif.Rules[0].Scenarios[0].Steps[0];
        Assert.That(givenStep.Keyword, Is.EqualTo(DisplayKeyword.Given));
        Assert.That(givenStep.Text, Is.EqualTo("I am on the login page"));
        Assert.That(givenStep.Owner, Is.EqualTo("this"));

        // And: When step should be unimplemented
        var whenStep = crif.Rules[0].Scenarios[0].Steps[1];
        Assert.That(whenStep.Keyword, Is.EqualTo(DisplayKeyword.When));
        Assert.That(whenStep.Text, Is.EqualTo("I log in"));
        Assert.That(whenStep.Owner, Is.EqualTo("this"));

        // And: Then step should be matched
        var thenStep = crif.Rules[0].Scenarios[0].Steps[2];
        Assert.That(thenStep.Keyword, Is.EqualTo(DisplayKeyword.Then));
        Assert.That(thenStep.Owner, Is.EqualTo("AuthSteps"));
        Assert.That(thenStep.Method, Is.EqualTo("TheUserShouldBeLoggedIn"));

        // And: And step should preserve "And" keyword and be matched
        var andStep = crif.Rules[0].Scenarios[0].Steps[3];
        Assert.That(andStep.Keyword, Is.EqualTo(DisplayKeyword.And));
        Assert.That(andStep.Owner, Is.EqualTo("SessionSteps"));
        Assert.That(andStep.Method, Is.EqualTo("TheSessionShouldBeActive"));

        // And: Unimplemented steps should be tracked
        Assert.That(crif.Unimplemented, Has.Count.EqualTo(2));
        Assert.That(crif.Unimplemented.Any(u => u.Keyword == NormalizedKeyword.Given && u.Text == "I am on the login page"), Is.True);
        Assert.That(crif.Unimplemented.Any(u => u.Keyword == NormalizedKeyword.When && u.Text == "I log in"), Is.True);

        // And: Only matched step classes should be in the classes list
        Assert.That(crif.Classes, Has.Count.EqualTo(2));
        Assert.That(crif.Classes, Contains.Item("AuthSteps"));
        Assert.That(crif.Classes, Contains.Item("SessionSteps"));
    }
}
