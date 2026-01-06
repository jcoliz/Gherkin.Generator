namespace Gherkin.Generator.Tests.Example;

/// <summary>
/// Base class for functional tests with shopping cart capabilities.
/// </summary>
public class FunctionalTestBase
{
    /// <summary>
    /// Gets the shopping cart instance for the current test.
    /// </summary>
    public ShoppingCart Cart { get; private set; } = null!;

    /// <summary>
    /// Gets a value indicating whether the application is running.
    /// </summary>
    public bool IsApplicationRunning { get; private set; }

    /// <summary>
    /// Gets a value indicating whether a user is logged in.
    /// </summary>
    public bool IsLoggedIn { get; private set; }

    /// <summary>
    /// Gets the current logged-in user type.
    /// </summary>
    public string? CurrentUserType { get; private set; }

    /// <summary>
    /// Launches the application.
    /// </summary>
    public Task LaunchApplicationAsync()
    {
        IsApplicationRunning = true;
        Cart = new ShoppingCart();
        return Task.CompletedTask;
    }

    /// <summary>
    /// Logs in as a specific user type.
    /// </summary>
    /// <param name="userType">The type of user to log in as (e.g., "customer", "admin").</param>
    public Task LoginAsAsync(string userType)
    {
        IsLoggedIn = true;
        CurrentUserType = userType;
        return Task.CompletedTask;
    }

    /// <summary>
    /// Tears down the test, clearing the cart and resetting state.
    /// </summary>
    [TearDown]
    public void TearDown()
    {
        Cart?.Clear();
        IsApplicationRunning = false;
        IsLoggedIn = false;
        CurrentUserType = null;
    }
}
