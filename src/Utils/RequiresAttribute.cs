using System;

namespace Gherkin.Generator.Utils;

/// <summary>
/// Declares that a step depends on the specified shared state before it can execute.
/// </summary>
/// <param name="name">
/// The scenario-local state name that must already be present.
/// Example: "UserLoggedIn"
/// </param>
/// <param name="description">
/// Optional human-readable description of the required state.
/// Example: "The authenticated user account used for this scenario"
/// </param>
/// <remarks>
/// This attribute is generator metadata for scenario-state analysis. It does not define a
/// Gherkin step binding, does not modify feature-file behavior, and does not configure test
/// infrastructure. It simply describes the state that must already exist before this step runs.
/// </remarks>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
public class RequiresAttribute(string name, string? description = null) : Attribute
{
    /// <summary>
    /// Gets the name of the required object.
    /// </summary>
    public string Name { get; } = name;

    /// <summary>
    /// Gets the human-readable description of the required object.
    /// </summary>
    public string? Description { get; } = description;
}