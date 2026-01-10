using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Gherkin.Ast;

namespace Gherkin.Generator.Lib;

/// <summary>
/// Processes Gherkin steps, handling conversion, matching, and argument extraction.
/// </summary>
internal class StepProcessor
{
    private readonly StepMetadataCollection _stepMetadata;

    /// <summary>
    /// Initializes a new instance of the StepProcessor class.
    /// </summary>
    /// <param name="stepMetadata">Collection of step definition metadata for matching.</param>
    public StepProcessor(StepMetadataCollection stepMetadata)
    {
        _stepMetadata = stepMetadata;
    }

    /// <summary>
    /// Converts a Gherkin step to CRIF format.
    /// </summary>
    /// <param name="step">The Gherkin step to convert.</param>
    /// <returns>Step in CRIF format.</returns>
    public StepCrif ConvertStep(Step step)
    {
        var stepCrif = new StepCrif
        {
            Keyword = ParseDisplayKeyword(step.Keyword.Trim()),
            Text = step.Text,
            Owner = "this", // Default for unimplemented steps
            Method = string.Empty // Will be set during step matching
        };

        // Convert data table if present
        if (step.Argument is DataTable dataTable)
        {
            stepCrif.DataTable = ConvertDataTable(dataTable);
        }

        return stepCrif;
    }

    /// <summary>
    /// Parses a keyword string into a DisplayKeyword enum value.
    /// </summary>
    /// <param name="keyword">The keyword string (e.g., "Given", "When", "And").</param>
    /// <returns>The corresponding DisplayKeyword enum value.</returns>
    /// <exception cref="ArgumentException">Thrown when the keyword is not a valid Gherkin keyword.</exception>
    private static DisplayKeyword ParseDisplayKeyword(string keyword)
    {
        return keyword switch
        {
            "Given" => DisplayKeyword.Given,
            "When" => DisplayKeyword.When,
            "Then" => DisplayKeyword.Then,
            "And" => DisplayKeyword.And,
            "But" => DisplayKeyword.But,
            _ => throw new ArgumentException($"Unknown Gherkin keyword: '{keyword}'", nameof(keyword))
        };
    }

    /// <summary>
    /// Converts a Gherkin DataTable to CRIF format.
    /// </summary>
    /// <param name="dataTable">The Gherkin data table.</param>
    /// <returns>DataTable in CRIF format.</returns>
    private static DataTableCrif ConvertDataTable(DataTable dataTable)
    {
        var tableCrif = new DataTableCrif();

        var rows = dataTable.Rows.ToList();
        if (rows.Count > 0)
        {
            // First row is headers
            var headerRow = rows[0];
            var headerCells = headerRow.Cells.ToList();
            tableCrif.Headers = headerCells.Select((cell, index) => new HeaderCellCrif
            {
                Value = cell.Value,
                Last = index == headerCells.Count - 1
            }).ToList();

            // Remaining rows are data
            tableCrif.Rows = rows.Skip(1).Select((row, rowIndex) =>
            {
                var cells = row.Cells.ToList();
                return new DataRowCrif
                {
                    Last = rowIndex == rows.Count - 2, // rows.Count - 2 because we skipped first row
                    Cells = cells.Select((cell, cellIndex) => new DataCellCrif
                    {
                        Value = cell.Value,
                        Last = cellIndex == cells.Count - 1
                    }).ToList()
                };
            }).ToList();
        }

        return tableCrif;
    }

    /// <summary>
    /// Tracks unimplemented steps in the CRIF and returns whether any steps were unmatched.
    /// </summary>
    /// <param name="crif">The CRIF object to update.</param>
    /// <param name="steps">The steps to track.</param>
    /// <returns>True if any steps were unmatched; otherwise, false.</returns>
    public bool TrackUnimplementedSteps(FeatureCrif crif, List<StepCrif> steps)
    {
        var currentKeyword = NormalizedKeyword.Given;
        var hasUnmatchedSteps = false;

        foreach (var step in steps)
        {
            var normalizedKeyword = NormalizeKeyword(step.Keyword, ref currentKeyword);
            var matchedStep = _stepMetadata.FindMatch(normalizedKeyword, step.Text);

            if (matchedStep != null)
            {
                ProcessImplementedStep(crif, step, matchedStep);
            }
            else
            {
                ProcessUnimplementedStep(crif, step, normalizedKeyword);
                hasUnmatchedSteps = true;
            }
        }

        return hasUnmatchedSteps;
    }

    /// <summary>
    /// Normalizes And/But keywords to the current keyword context.
    /// </summary>
    /// <param name="keyword">The display keyword to normalize.</param>
    /// <param name="currentKeyword">The current keyword context (Given, When, or Then).</param>
    /// <returns>The normalized keyword.</returns>
    private static NormalizedKeyword NormalizeKeyword(DisplayKeyword keyword, ref NormalizedKeyword currentKeyword)
    {
        return keyword switch
        {
            DisplayKeyword.Given => currentKeyword = NormalizedKeyword.Given,
            DisplayKeyword.When => currentKeyword = NormalizedKeyword.When,
            DisplayKeyword.Then => currentKeyword = NormalizedKeyword.Then,
            DisplayKeyword.And or DisplayKeyword.But => currentKeyword,
            _ => currentKeyword
        };
    }

    /// <summary>
    /// Processes an implemented step by populating its metadata and arguments.
    /// </summary>
    /// <param name="crif">The CRIF object to update.</param>
    /// <param name="step">The step to process.</param>
    /// <param name="matchedStep">The matched step metadata.</param>
    private static void ProcessImplementedStep(FeatureCrif crif, StepCrif step, StepMetadata matchedStep)
    {
        step.Owner = matchedStep.Class;
        step.Method = matchedStep.Method;

        AddClassAndNamespace(crif, matchedStep);
        ExtractStepArguments(step, matchedStep);
        AddDataTableArgument(step, matchedStep);
        MarkLastArgument(step);
    }

    /// <summary>
    /// Adds class and namespace to CRIF if not already present.
    /// </summary>
    /// <param name="crif">The CRIF object to update.</param>
    /// <param name="matchedStep">The matched step metadata.</param>
    private static void AddClassAndNamespace(FeatureCrif crif, StepMetadata matchedStep)
    {
        if (!crif.Classes.Contains(matchedStep.Class))
        {
            crif.Classes.Add(matchedStep.Class);
        }
        if (!crif.Usings.Contains(matchedStep.Namespace))
        {
            crif.Usings.Add(matchedStep.Namespace);
        }
    }

    /// <summary>
    /// Extracts and adds arguments from text parameters to the step.
    /// </summary>
    /// <param name="step">The step to add arguments to.</param>
    /// <param name="matchedStep">The matched step metadata.</param>
    private static void ExtractStepArguments(StepCrif step, StepMetadata matchedStep)
    {
        var textParameters = matchedStep.Parameters.Where(p => p.Type != "DataTable").ToList();
        if (textParameters.Count > 0)
        {
            var arguments = StepArgumentExtractor.ExtractArguments(matchedStep.Text, step.Text, textParameters);
            step.Arguments.AddRange(arguments);
        }
    }

    /// <summary>
    /// Adds DataTable variable as argument if step has DataTable parameter.
    /// </summary>
    /// <param name="step">The step to add the argument to.</param>
    /// <param name="matchedStep">The matched step metadata.</param>
    private static void AddDataTableArgument(StepCrif step, StepMetadata matchedStep)
    {
        if (matchedStep.Parameters.Any(p => p.Type == "DataTable") && step.DataTable != null)
        {
            step.Arguments.Add(new ArgumentCrif
            {
                Value = step.DataTable.VariableName,
                Last = false
            });
        }
    }

    /// <summary>
    /// Marks the last argument in the step's argument list.
    /// </summary>
    /// <param name="step">The step to update.</param>
    private static void MarkLastArgument(StepCrif step)
    {
        if (step.Arguments.Count > 0)
        {
            step.Arguments[step.Arguments.Count - 1].Last = true;
        }
    }

    /// <summary>
    /// Processes an unimplemented step by tracking it in the CRIF.
    /// </summary>
    /// <param name="crif">The CRIF object to update.</param>
    /// <param name="step">The step to process.</param>
    /// <param name="normalizedKeyword">The normalized keyword.</param>
    private void ProcessUnimplementedStep(FeatureCrif crif, StepCrif step, NormalizedKeyword normalizedKeyword)
    {
        AddToUnimplementedList(crif, step, normalizedKeyword);

        // Generate method name with integers and quoted strings removed
        step.Method = ConvertToMethodNameWithoutParameters(step.Text);

        // Extract scenario outline placeholders (e.g., <amount>) and add as arguments
        StepArgumentExtractor.ExtractScenarioOutlinePlaceholders(step);

        // Extract integers and add as arguments
        StepArgumentExtractor.ExtractIntegerParameters(step);

        // Extract quoted strings and add as arguments
        StepArgumentExtractor.ExtractQuotedStringParameters(step);

        if (step.DataTable != null)
        {
            step.Arguments.Add(new ArgumentCrif
            {
                Value = step.DataTable.VariableName,
                Last = false
            });
        }

        // Mark the last argument
        MarkLastArgument(step);
    }

    /// <summary>
    /// Converts step text to a method name, removing integers and quoted strings.
    /// </summary>
    /// <param name="text">The step text.</param>
    /// <returns>PascalCase method name without integers or quoted strings.</returns>
    private static string ConvertToMethodNameWithoutParameters(string text)
    {
        // Remove integers and quoted strings from the text before converting to method name
        var textWithoutParameters = Regex.Replace(text, @"\b\d+\b", "");
        textWithoutParameters = Regex.Replace(textWithoutParameters, @"""[^""]*""", "");
        return ConvertToMethodName(textWithoutParameters);
    }

    /// <summary>
    /// Converts a name to PascalCase method name.
    /// </summary>
    /// <param name="name">The name to convert.</param>
    /// <returns>PascalCase method name.</returns>
    public static string ConvertToMethodName(string name)
    {
        // Convert scenario name to PascalCase method name
        var words = name.Split(new[] { ' ', '-', '_' }, StringSplitOptions.RemoveEmptyEntries);
        var result = string.Join("", words.Select(w =>
            char.ToUpper(w[0]) + (w.Length > 1 ? w.Substring(1) : "")));

        // Remove any remaining special characters
        result = new string(result.Where(c => char.IsLetterOrDigit(c)).ToArray());

        return result;
    }

    /// <summary>
    /// Adds step to unimplemented list if not already present.
    /// </summary>
    /// <param name="crif">The CRIF object to update.</param>
    /// <param name="step">The step to add.</param>
    /// <param name="normalizedKeyword">The normalized keyword.</param>
    private void AddToUnimplementedList(FeatureCrif crif, StepCrif step, NormalizedKeyword normalizedKeyword)
    {
        // Generate pattern text by replacing integers and quoted strings with placeholders
        var patternText = GeneratePatternText(step.Text);
        
        var existingUnimplemented = crif.Unimplemented.FirstOrDefault(u =>
            u.Text == patternText && u.Keyword == normalizedKeyword);

        if (existingUnimplemented == null)
        {
            // Generate method name from pattern text (with placeholders removed)
            var textForMethodName = Regex.Replace(patternText, @"\{[^}]+\}", "");
            
            var unimplementedStep = new UnimplementedStepCrif
            {
                Keyword = normalizedKeyword,
                Text = patternText,
                Method = ConvertToMethodName(textForMethodName),
                Parameters = []
            };

            // Extract all parameters in the order they appear in the text
            var placeholderRegex = new Regex(@"<(\w+)>");
            var integerRegex = new Regex(@"\b(\d+)\b");
            var stringRegex = new Regex(@"""[^""]*""");

            // Collect all matches with their positions
            var allMatches = new List<(int Position, string Type, string Value)>();
            
            // Add scenario outline placeholders
            foreach (Match match in placeholderRegex.Matches(step.Text))
            {
                allMatches.Add((match.Index, "placeholder", match.Groups[1].Value));
            }
            
            // Add integers
            var integerMatches = integerRegex.Matches(step.Text).Cast<Match>().ToList();
            for (int i = 0; i < integerMatches.Count; i++)
            {
                allMatches.Add((integerMatches[i].Index, "int", $"value{i + 1}"));
            }
            
            // Add quoted strings
            var stringMatches = stringRegex.Matches(step.Text).Cast<Match>().ToList();
            for (int i = 0; i < stringMatches.Count; i++)
            {
                allMatches.Add((stringMatches[i].Index, "string", $"string{i + 1}"));
            }
            
            // Sort by position and create parameters
            foreach (var match in allMatches.OrderBy(m => m.Position))
            {
                unimplementedStep.Parameters.Add(new ParameterCrif
                {
                    Type = match.Type == "placeholder" ? "string" : match.Type,
                    Name = match.Value,
                    Last = false
                });
            }

            if (step.DataTable != null)
            {
                unimplementedStep.Parameters.Add(new ParameterCrif
                {
                    Type = "DataTable",
                    Name = "table",
                    Last = false
                });
            }

            // Mark the last parameter
            if (unimplementedStep.Parameters.Count > 0)
            {
                unimplementedStep.Parameters[unimplementedStep.Parameters.Count - 1].Last = true;
            }

            crif.Unimplemented.Add(unimplementedStep);
        }
    }

    /// <summary>
    /// Generates pattern text by replacing integers and quoted strings with placeholders.
    /// </summary>
    /// <param name="text">The step text.</param>
    /// <returns>Pattern text with placeholders.</returns>
    private static string GeneratePatternText(string text)
    {
        var patternText = text;
        
        // Replace integers with placeholders
        var integerRegex = new Regex(@"\b(\d+)\b");
        var integerMatches = integerRegex.Matches(text).Cast<Match>().ToList();
        for (int i = integerMatches.Count - 1; i >= 0; i--)
        {
            var match = integerMatches[i];
            patternText = patternText.Substring(0, match.Index)
                + $"{{value{i + 1}}}"
                + patternText.Substring(match.Index + match.Length);
        }
        
        // Replace quoted strings with placeholders
        var stringRegex = new Regex(@"""[^""]*""");
        var stringMatches = stringRegex.Matches(patternText).Cast<Match>().ToList();
        for (int i = stringMatches.Count - 1; i >= 0; i--)
        {
            var match = stringMatches[i];
            patternText = patternText.Substring(0, match.Index)
                + $"{{string{i + 1}}}"
                + patternText.Substring(match.Index + match.Length);
        }
        
        return patternText;
    }
}
