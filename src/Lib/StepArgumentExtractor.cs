using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Gherkin.Generator.Lib;

/// <summary>
/// Extracts arguments from Gherkin step text by matching against step definition patterns.
/// </summary>
internal static class StepArgumentExtractor
{
    /// <summary>
    /// Extracts arguments by matching step text against a pattern with placeholders.
    /// </summary>
    /// <param name="pattern">Step definition pattern with {placeholder} markers.</param>
    /// <param name="text">Actual step text from Gherkin scenario.</param>
    /// <param name="parameters">List of step parameters for type information.</param>
    /// <returns>List of extracted arguments ready for method invocation.</returns>
    public static List<ArgumentCrif> ExtractArguments(string pattern, string text, List<StepParameter> parameters)
    {
        var arguments = new List<ArgumentCrif>();
        var regexPattern = BuildRegexPattern(pattern);

        try
        {
            var regex = new Regex(
                regexPattern,
                RegexOptions.IgnoreCase
            );
            var match = regex.Match(text);

            if (match.Success)
            {
                ExtractArgumentsFromMatch(match, parameters, arguments);
            }
        }
        catch
        {
            // Fallback: couldn't extract arguments
        }

        return arguments;
    }

    /// <summary>
    /// Builds a regex pattern from step definition text by replacing placeholders with capture groups.
    /// </summary>
    /// <param name="pattern">Step definition pattern with {placeholder} markers.</param>
    /// <returns>Regex pattern string.</returns>
    private static string BuildRegexPattern(string pattern)
    {
        // Replace {placeholder} with temporary markers BEFORE escaping
        var regexPattern = Regex.Replace(
            pattern,
            @"\{[^}]+\}",
            "<<<PLACEHOLDER>>>"
        );

        // Escape the pattern for regex special characters
        regexPattern = Regex.Escape(regexPattern);

        // Replace markers with capture groups
        regexPattern = regexPattern.Replace(
            "<<<PLACEHOLDER>>>",
            @"((?:""[^""]*""|\S+))"
        );

        // Add anchors for full string match
        return "^" + regexPattern + "$";
    }

    /// <summary>
    /// Extracts arguments from a successful regex match.
    /// </summary>
    /// <param name="match">The successful regex match.</param>
    /// <param name="parameters">List of step parameters.</param>
    /// <param name="arguments">List to add extracted arguments to.</param>
    private static void ExtractArgumentsFromMatch(Match match, List<StepParameter> parameters, List<ArgumentCrif> arguments)
    {
        // Groups[0] is the entire match, Groups[1..n] are capture groups
        var extractedArgs = match.Groups
            .Cast<Group>()
            .Skip(1) // Skip Groups[0] which is the full match
            .Select((group, index) => new ArgumentCrif
            {
                Value = ProcessArgumentValue(group.Value, index, parameters),
                Last = false
            });

        arguments.AddRange(extractedArgs);
    }

    /// <summary>
    /// Processes an argument value, handling scenario outline parameters and quote formatting.
    /// </summary>
    /// <param name="value">The extracted value.</param>
    /// <param name="paramIndex">The parameter index (0-based).</param>
    /// <param name="parameters">List of step parameters.</param>
    /// <returns>Processed argument value.</returns>
    private static string ProcessArgumentValue(string value, int paramIndex, List<StepParameter> parameters)
    {
        // Check if this is a Scenario Outline parameter (e.g., <amount>)
        if (value.StartsWith("<") && value.EndsWith(">"))
        {
            return value.Substring(1, value.Length - 2);
        }

        // This is a concrete value - determine if we need to add quotes
        return FormatConcreteValue(value, paramIndex, parameters);
    }

    /// <summary>
    /// Formats a concrete value by adding quotes for string types if needed.
    /// </summary>
    /// <param name="value">The value to format.</param>
    /// <param name="paramIndex">The parameter index.</param>
    /// <param name="parameters">List of step parameters.</param>
    /// <returns>Formatted value.</returns>
    private static string FormatConcreteValue(string value, int paramIndex, List<StepParameter> parameters)
    {
        if (paramIndex >= parameters.Count)
        {
            return value;
        }

        var paramType = parameters[paramIndex].Type;
        var isStringType = paramType.Equals("string", StringComparison.OrdinalIgnoreCase);

        // If it's a string type and not already quoted, add quotes
        if (isStringType && !value.StartsWith("\""))
        {
            return $"\"{value}\"";
        }

        // For non-string types, keep the value as-is (no quotes for numbers)
        return value;
    }

    /// <summary>
    /// Extracts integer parameters from step text and adds them as arguments.
    /// </summary>
    /// <param name="step">The step to process.</param>
    public static void ExtractIntegerParameters(StepCrif step)
    {
        var regex = new Regex(@"\b(\d+)\b");
        var matches = regex.Matches(step.Text);

        var integerArguments = matches
            .Cast<Match>()
            .Select(match => new ArgumentCrif
            {
                Value = match.Groups[1].Value,
                Last = false
            });

        step.Arguments.AddRange(integerArguments);
    }

    /// <summary>
    /// Extracts quoted string parameters from step text and adds them as arguments.
    /// </summary>
    /// <param name="step">The step to process.</param>
    public static void ExtractQuotedStringParameters(StepCrif step)
    {
        var regex = new Regex(@"""([^""]*)""");
        var matches = regex.Matches(step.Text);

        var stringArguments = matches
            .Cast<Match>()
            .Select(match => new ArgumentCrif
            {
                Value = match.Value, // Keep quotes in the value
                Last = false
            });

        step.Arguments.AddRange(stringArguments);
    }

    /// <summary>
    /// Extracts scenario outline placeholders from step text and adds them as arguments.
    /// </summary>
    /// <param name="step">The step to process.</param>
    public static void ExtractScenarioOutlinePlaceholders(StepCrif step)
    {
        // Find all placeholders in the format <parameterName>
        var regex = new Regex(@"<(\w+)>");
        var arguments = regex.Matches(step.Text)
            .Cast<Match>()
            .Select(match => new ArgumentCrif
            {
                Value = match.Groups[1].Value,
                Last = false
            });

        step.Arguments.AddRange(arguments);
    }
}
