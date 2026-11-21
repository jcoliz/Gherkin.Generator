using System.Globalization;
using System.Text;

namespace Gherkin.Generator.Lib;

internal static class CSharpStringLiteral
{
    private static readonly Dictionary<char, string> EscapeSequences = new()
    {
        ['"'] = "\\\"",
        ['\\'] = "\\\\",
        ['\r'] = "\\r",
        ['\n'] = "\\n",
        ['\t'] = "\\t"
    };

    public static string Format(string value)
    {
        var result = new StringBuilder("\"");
        foreach (var character in value)
        {
            result.Append(FormatCharacter(character));
        }

        return result.Append('"').ToString();
    }

    private static string FormatCharacter(char character)
    {
        if (EscapeSequences.TryGetValue(character, out var escapeSequence))
        {
            return escapeSequence;
        }

        if (RequiresUnicodeEscape(character))
        {
            return "\\u" + ((int)character).ToString("x4", CultureInfo.InvariantCulture);
        }

        return character.ToString();
    }

    private static bool RequiresUnicodeEscape(char character) =>
        char.IsControl(character) || character is '\u2028' or '\u2029';
}
