using System.Globalization;
using System.Text;

namespace Gherkin.Generator.Lib;

internal static class CSharpStringLiteral
{
    public static string Format(string value)
    {
        var result = new StringBuilder("\"");
        foreach (var character in value)
        {
            result.Append(character switch
            {
                '"' => "\\\"",
                '\\' => "\\\\",
                '\r' => "\\r",
                '\n' => "\\n",
                '\t' => "\\t",
                _ when char.IsControl(character) || character is '\u2028' or '\u2029'
                    => "\\u" + ((int)character).ToString("x4", CultureInfo.InvariantCulture),
                _ => character.ToString()
            });
        }

        return result.Append('"').ToString();
    }
}
