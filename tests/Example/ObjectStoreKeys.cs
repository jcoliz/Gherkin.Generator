namespace Gherkin.Generator.Tests.Example;

/// <summary>
/// Defines the keys used for storing and retrieving objects in the scenario context.
/// </summary>
public static class ObjectStoreKeys
{
    /// <summary>
    /// The default user context available before any scenario steps are executed.
    /// </summary>
    public const string DefaultUser = "Default-User";

    /// <summary>
    /// The customer profile loaded for this scenario.
    /// </summary>
    public const string CustomerProfile = "Customer-Profile";

    /// <summary>
    /// The generated account summary must already exist.
    /// </summary>
    public const string AccountSummary = "Account-Summary";    
}
