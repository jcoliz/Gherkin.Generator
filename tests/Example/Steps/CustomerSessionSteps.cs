using Gherkin.Generator.Utils;

namespace Gherkin.Generator.Tests.Example.Steps;

/// <summary>
/// Example step definitions showing scenario-local state metadata.
/// </summary>
public class CustomerSessionSteps
{
    private readonly FunctionalTestBase _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="CustomerSessionSteps"/> class.
    /// </summary>
    /// <param name="context">The functional test context.</param>
    public CustomerSessionSteps(FunctionalTestBase context)
    {
        _context = context;
    }

    /// <summary>
    /// Loads the customer profile for the active scenario.
    /// </summary>
    [Given("the customer profile is loaded")]
    [Requires("DefaultUser", "Default seeded user context available before scenario steps")]
    [Provides("CustomerProfile", "The customer profile loaded for this scenario")]
    public async Task LoadCustomerProfile()
    {
        Assert.That(_context.DefaultUser, Is.Not.Null.And.Not.Empty);
        _context.CurrentUserType = _context.CurrentUserType ?? _context.DefaultUser;
        await Task.CompletedTask;
    }

    /// <summary>
    /// Requests the summary for the customer account.
    /// </summary>
    [When("the customer requests the account summary")]
    [Requires("CustomerProfile", "The customer profile must already be loaded")]
    [Provides("AccountSummary", "The account summary produced for the current customer")]
    public async Task RequestAccountSummary()
    {
        await Task.CompletedTask;
    }

    /// <summary>
    /// Verifies that the account summary shows the expected state.
    /// </summary>
    /// <param name="expectedStatus">The expected summary status.</param>
    [Then("the account summary should show {expectedStatus}")]
    [Requires("AccountSummary", "The generated account summary must already exist")]
    public async Task AccountSummaryShouldShow(string expectedStatus)
    {
        Assert.That(expectedStatus, Is.EqualTo("ready"));
        await Task.CompletedTask;
    }
}
