using System;

namespace Gherkin.Generator.Utils;

/// <summary>
/// Declares that a step establishes the specified shared state after it executes.
/// </summary>
/// <param name="name">
/// The scenario-local state name that this step makes available.
/// Example: "UserLoggedIn"
/// </param>
/// <param name="description">
/// Optional human-readable description of the provided state.
/// Example: "The authenticated user account used for this scenario"
/// </param>
/// <remarks>
/// This attribute is generator metadata for scenario-state analysis. It does not define a
/// Gherkin step binding, does not modify feature-file behavior, and does not configure test
/// infrastructure. It simply describes the state produced by this step so later steps can
/// depend on it in execution order.
/// </remarks>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
public class ProvidesAttribute(string name, string? description = null) : Attribute
{
    /// <summary>
    /// Gets the name of the provided object.
    /// </summary>
    public string Name { get; } = name;

    /// <summary>
    /// Gets the human-readable description of the provided object.
    /// </summary>
    public string? Description { get; } = description;
}
