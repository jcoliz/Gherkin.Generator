using Gherkin.Generator.Utils;

namespace Gherkin.Generator.Tests.Example.Steps;

/// <summary>
/// Step definitions for application setup and authentication scenarios.
/// </summary>
public class ApplicationSteps
{
    private readonly FunctionalTestBase _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="ApplicationSteps"/> class.
    /// </summary>
    /// <param name="context">The test context providing access to application state.</param>
    public ApplicationSteps(FunctionalTestBase context)
    {
        _context = context;
    }

    /// <summary>
    /// Launches the application for testing.
    /// </summary>
    [Given("the application is running")]
    public async Task ApplicationIsRunning()
    {
        Console.WriteLine("test: {0}", TestContext.CurrentContext.Test.Name);
        await _context.LaunchApplicationAsync();
    }

    /// <summary>
    /// Logs in as a specific type of user.
    /// </summary>
    /// <param name="userType">The type of user to log in as.</param>
    [Given("I am logged in as {userType}")]
    public async Task LoggedInAs(string userType)
    {
        await _context.LoginAsAsync(userType);
    }

    /// <summary>
    /// A step that is never used in any scenario, included to test that unused steps do not appear in the generated catalog.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    [Given("a step that is never used")]
    public async Task AStepThatIsNeverUsed()
    {
        throw new NotImplementedException("This step is never used in any scenario and should not appear in the catalog.");
    }
}
