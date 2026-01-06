using Gherkin.Ast;

namespace Gherkin.Generator.Tests.Unit;

/// <summary>
/// Static helper methods for parsing Gherkin text in tests.
/// </summary>
public static class GherkinTestHelpers
{
    /// <summary>
    /// Helper method to parse Gherkin text into a GherkinDocument.
    /// </summary>
    /// <param name="gherkinText">The Gherkin text to parse.</param>
    /// <returns>A parsed GherkinDocument.</returns>
    public static GherkinDocument ParseGherkin(string gherkinText)
    {
        var parser = new Parser();
        var reader = new StringReader(gherkinText);
        return parser.Parse(reader);
    }
}
