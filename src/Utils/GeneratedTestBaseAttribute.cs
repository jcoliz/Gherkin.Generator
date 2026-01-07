using System;

namespace Gherkin.Generator.Utils;

/// <summary>
/// Marks a class as the default base class for generated Gherkin tests.
/// </summary>
/// <remarks>
/// This attribute helps the source generator automatically determine default values
/// for the @namespace and @baseclass tags in feature files. When applied to a test base class,
/// feature files can omit these tags and inherit the defaults.
/// </remarks>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public class GeneratedTestBaseAttribute : Attribute
{
    /// <summary>
    /// Gets or sets the namespace for generated test classes.
    /// </summary>
    /// <remarks>
    /// If not specified, defaults to the namespace of the class decorated with this attribute.
    /// Example: If the base class is in "MyApp.Tests", generated tests will also be in "MyApp.Tests".
    /// </remarks>
    public string? UseNamespace { get; set; }
}
