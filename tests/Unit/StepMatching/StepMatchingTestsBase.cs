using Gherkin.Ast;

namespace Gherkin.Generator.Tests.Unit;

/// <summary>
/// Base class for step matching integration tests.
/// Provides common helper methods for parsing Gherkin text.
/// </summary>
public abstract class StepMatchingTestsBase
{
    /// <summary>
    /// Helper method to parse Gherkin text into a GherkinDocument.
    /// </summary>
    /// <param name="gherkinText">The Gherkin text to parse.</param>
    /// <returns>A parsed GherkinDocument.</returns>
    protected static GherkinDocument ParseGherkin(string gherkinText)
    {
        var parser = new Parser();
        var reader = new StringReader(gherkinText);
        return parser.Parse(reader);
    }
}
