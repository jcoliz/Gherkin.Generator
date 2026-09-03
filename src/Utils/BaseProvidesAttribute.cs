using System;

namespace Gherkin.Generator.Utils;

/// <summary>
/// Declares scenario-start shared state that is provided by test infrastructure.
/// </summary>
/// <param name="name">
/// The scenario-local state name that exists before generated steps execute.
/// Example: "DefaultUser"
/// </param>
/// <param name="description">
/// Optional human-readable description of the base-provided state.
/// Example: "Default seeded test user used for functional login"
/// </param>
/// <remarks>
/// This attribute is generator metadata used to annotate generated Requires comments.
/// It does not create runtime state and does not define a Gherkin step binding.
/// </remarks>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
public class BaseProvidesAttribute(string name, string? description = null) : Attribute
{
    /// <summary>
    /// Gets the name of the base-provided object.
    /// </summary>
    public string Name { get; } = name;

    /// <summary>
    /// Gets the human-readable description of the base-provided object.
    /// </summary>
    public string? Description { get; } = description;
}