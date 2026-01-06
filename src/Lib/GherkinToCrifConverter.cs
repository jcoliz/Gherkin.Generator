using System;
using System.Collections.Generic;
using System.Linq;
using Gherkin.Ast;

namespace Gherkin.Generator.Lib;

/// <summary>
/// Converts a Gherkin feature document into a Code-Ready Intermediate Form (CRIF)
/// for test generation, with step matching against available step definitions.
/// </summary>
/// <param name="stepMetadata">Collection of step definition metadata for matching.</param>
public class GherkinToCrifConverter(StepMetadataCollection stepMetadata)
{
    /// <summary>
    /// Converts a Gherkin feature document to a CRIF object.
    /// </summary>
    /// <param name="feature">Parsed Gherkin feature document.</param>
    /// <returns>Code-Ready Intermediate Form ready for template rendering.</returns>
    public FeatureCrif Convert(GherkinDocument feature)
    {
        return Convert(feature, string.Empty);
    }

    /// <summary>
    /// Converts a Gherkin feature document to a CRIF object with a specified filename.
    /// </summary>
    /// <param name="feature">Parsed Gherkin feature document.</param>
    /// <param name="fileName">Name of the feature file without extension (e.g., "BankImport").</param>
    /// <returns>Code-Ready Intermediate Form ready for template rendering.</returns>
    public FeatureCrif Convert(GherkinDocument feature, string fileName)
    {
        var crif = new FeatureCrif
        {
            FileName = fileName
        };

        if (feature.Feature != null)
        {
            ProcessFeatureMetadata(feature.Feature, crif);
            ProcessFeatureTags(feature.Feature.Tags, crif);
            ProcessBackground(feature.Feature, crif);
            ProcessFeatureChildren(feature.Feature.Children, crif);
        }

        return crif;
    }

    /// <summary>
    /// Processes feature metadata including name and description.
    /// </summary>
    /// <param name="feature">The Gherkin feature.</param>
    /// <param name="crif">The CRIF object to populate.</param>
    private static void ProcessFeatureMetadata(Feature feature, FeatureCrif crif)
    {
        crif.FeatureName = feature.Name;

        if (feature.Description != null)
        {
            var lines = feature.Description.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in lines)
            {
                crif.DescriptionLines.Add(line.Trim());
            }
        }
    }

    /// <summary>
    /// Processes feature-level tags for namespace, base class, and using directives.
    /// </summary>
    /// <param name="tags">Collection of feature tags.</param>
    /// <param name="crif">The CRIF object to populate.</param>
    private static void ProcessFeatureTags(IEnumerable<Tag> tags, FeatureCrif crif)
    {
        foreach (var tag in tags)
        {
            if (tag.Name.StartsWith("@namespace:"))
            {
                ProcessNamespaceTag(tag, crif);
            }
            else if (tag.Name.StartsWith("@baseclass:"))
            {
                ProcessBaseClassTag(tag, crif);
            }
            else if (tag.Name.StartsWith("@using:"))
            {
                ProcessUsingTag(tag, crif);
            }
        }
    }

    /// <summary>
    /// Processes a namespace tag.
    /// </summary>
    /// <param name="tag">The namespace tag.</param>
    /// <param name="crif">The CRIF object to populate.</param>
    private static void ProcessNamespaceTag(Tag tag, FeatureCrif crif)
    {
        crif.Namespace = tag.Name.Substring("@namespace:".Length);
    }

    /// <summary>
    /// Processes a base class tag, extracting namespace if present.
    /// </summary>
    /// <param name="tag">The base class tag.</param>
    /// <param name="crif">The CRIF object to populate.</param>
    private static void ProcessBaseClassTag(Tag tag, FeatureCrif crif)
    {
        var baseClassValue = tag.Name.Substring("@baseclass:".Length);
        var lastDotIndex = baseClassValue.LastIndexOf('.');

        if (lastDotIndex >= 0)
        {
            var ns = baseClassValue.Substring(0, lastDotIndex);
            crif.BaseClass = baseClassValue.Substring(lastDotIndex + 1);
            if (!crif.Usings.Contains(ns))
            {
                crif.Usings.Add(ns);
            }
        }
        else
        {
            crif.BaseClass = baseClassValue;
        }
    }

    /// <summary>
    /// Processes a using directive tag.
    /// </summary>
    /// <param name="tag">The using tag.</param>
    /// <param name="crif">The CRIF object to populate.</param>
    private static void ProcessUsingTag(Tag tag, FeatureCrif crif)
    {
        var usingValue = tag.Name.Substring("@using:".Length);
        if (!crif.Usings.Contains(usingValue))
        {
            crif.Usings.Add(usingValue);
        }
    }

    /// <summary>
    /// Processes the background section if present.
    /// </summary>
    /// <param name="feature">The Gherkin feature.</param>
    /// <param name="crif">The CRIF object to populate.</param>
    private void ProcessBackground(Feature feature, FeatureCrif crif)
    {
        var background = feature.Children.OfType<Background>().FirstOrDefault();
        if (background != null)
        {
            crif.Background = ConvertBackground(background);
            TrackUnimplementedSteps(crif, crif.Background.Steps);
        }
    }

    /// <summary>
    /// Processes feature children including rules and scenarios.
    /// </summary>
    /// <param name="children">Collection of feature children.</param>
    /// <param name="crif">The CRIF object to populate.</param>
    private void ProcessFeatureChildren(IEnumerable<IHasLocation> children, FeatureCrif crif)
    {
        RuleCrif? defaultRule = null;

        foreach (var child in children)
        {
            if (child is Rule rule)
            {
                ProcessRule(rule, crif);
            }
            else if (child is Scenario scenario)
            {
                defaultRule ??= CreateDefaultRule(crif);
                ProcessScenarioInRule(scenario, defaultRule, crif);
            }
        }
    }

    /// <summary>
    /// Processes a rule and its scenarios.
    /// </summary>
    /// <param name="rule">The rule to process.</param>
    /// <param name="crif">The CRIF object to populate.</param>
    private void ProcessRule(Rule rule, FeatureCrif crif)
    {
        var ruleCrif = new RuleCrif
        {
            Name = rule.Name,
            Description = rule.Description ?? string.Empty
        };

        foreach (var ruleChild in rule.Children)
        {
            if (ruleChild is Scenario scenario)
            {
                ProcessScenarioInRule(scenario, ruleCrif, crif);
            }
        }

        crif.Rules.Add(ruleCrif);
    }

    /// <summary>
    /// Creates a default rule for scenarios not under an explicit rule.
    /// </summary>
    /// <param name="crif">The CRIF object to populate.</param>
    /// <returns>The created default rule.</returns>
    private static RuleCrif CreateDefaultRule(FeatureCrif crif)
    {
        var defaultRule = new RuleCrif
        {
            Name = "All scenarios",
            Description = string.Empty
        };
        crif.Rules.Add(defaultRule);
        return defaultRule;
    }

    /// <summary>
    /// Processes a scenario within a rule.
    /// </summary>
    /// <param name="scenario">The scenario to process.</param>
    /// <param name="ruleCrif">The rule to add the scenario to.</param>
    /// <param name="crif">The CRIF object for tracking unimplemented steps.</param>
    private void ProcessScenarioInRule(Scenario scenario, RuleCrif ruleCrif, FeatureCrif crif)
    {
        var scenarioCrif = ConvertScenario(scenario);
        ruleCrif.Scenarios.Add(scenarioCrif);
        TrackUnimplementedSteps(crif, scenarioCrif.Steps);
    }

    private BackgroundCrif ConvertBackground(Background background)
    {
        var backgroundCrif = new BackgroundCrif();

        var tableCounter = 1;
        foreach (var step in background.Steps)
        {
            var stepCrif = ConvertStep(step);

            // Assign data table variable name if present
            if (stepCrif.DataTable != null)
            {
                stepCrif.DataTable.VariableName = $"table{tableCounter}";
                tableCounter++;
            }

            backgroundCrif.Steps.Add(stepCrif);
        }

        return backgroundCrif;
    }

    private ScenarioCrif ConvertScenario(Scenario scenario)
    {
        var scenarioCrif = new ScenarioCrif
        {
            Name = scenario.Name,
            Method = ConvertToMethodName(scenario.Name)
        };

        // Extract scenario description as remarks
        if (!string.IsNullOrWhiteSpace(scenario.Description))
        {
            var lines = scenario.Description.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
            scenarioCrif.Remarks = new RemarksCrif
            {
                Lines = lines.Select(l => l.Trim()).ToList()
            };
        }

        // Check for @explicit tag
        foreach (var tag in scenario.Tags)
        {
            if (tag.Name == "@explicit")
            {
                scenarioCrif.ExplicitTag = true;
                break;
            }
        }

        // Handle Scenario Outline examples
        if (scenario.Examples != null && scenario.Examples.Any())
        {
            var examples = scenario.Examples.First();
            var headerRow = examples.TableHeader;
            var dataRows = examples.TableBody;

            // Extract parameters from header
            foreach (var cell in headerRow.Cells)
            {
                scenarioCrif.Parameters.Add(new ParameterCrif
                {
                    Type = "string", // Default to string for unimplemented steps
                    Name = cell.Value,
                    Last = false // Will be set after all are added
                });
            }

            // Set Last flag on final parameter
            if (scenarioCrif.Parameters.Any())
            {
                scenarioCrif.Parameters[scenarioCrif.Parameters.Count - 1].Last = true;
            }

            // Generate test cases from data rows
            foreach (var dataRow in dataRows)
            {
                var values = dataRow.Cells.Select(c => $"\"{c.Value}\"");
                scenarioCrif.TestCases.Add(string.Join(", ", values));
            }
        }

        // Convert steps
        var tableCounter = 1;
        foreach (var step in scenario.Steps)
        {
            var stepCrif = ConvertStep(step);

            // Assign data table variable name if present
            if (stepCrif.DataTable != null)
            {
                stepCrif.DataTable.VariableName = $"table{tableCounter}";
                tableCounter++;
            }

            scenarioCrif.Steps.Add(stepCrif);
        }

        return scenarioCrif;
    }

    private StepCrif ConvertStep(Gherkin.Ast.Step step)
    {
        var stepCrif = new StepCrif
        {
            Keyword = step.Keyword.Trim(),
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

    private DataTableCrif ConvertDataTable(DataTable dataTable)
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

    private static string ConvertToMethodName(string name)
    {
        // Convert scenario name to PascalCase method name
        var words = name.Split(new[] { ' ', '-', '_' }, StringSplitOptions.RemoveEmptyEntries);
        var result = string.Join("", words.Select(w =>
            char.ToUpper(w[0]) + (w.Length > 1 ? w.Substring(1) : "")));

        // Remove any remaining special characters
        result = new string(result.Where(c => char.IsLetterOrDigit(c)).ToArray());

        return result;
    }

    private void TrackUnimplementedSteps(FeatureCrif crif, List<StepCrif> steps)
    {
        var currentKeyword = NormalizedKeyword.Given;

        foreach (var step in steps)
        {
            var normalizedKeyword = NormalizeKeyword(step.Keyword, ref currentKeyword);
            var matchedStep = stepMetadata.FindMatch(normalizedKeyword, step.Text);

            if (matchedStep != null)
            {
                ProcessImplementedStep(crif, step, matchedStep);
            }
            else
            {
                ProcessUnimplementedStep(crif, step, normalizedKeyword);
            }
        }
    }

    /// <summary>
    /// Normalizes And/But keywords to the current keyword context.
    /// </summary>
    /// <param name="keyword">The keyword to normalize.</param>
    /// <param name="currentKeyword">The current keyword context (Given, When, or Then).</param>
    /// <returns>The normalized keyword.</returns>
    private static NormalizedKeyword NormalizeKeyword(string keyword, ref NormalizedKeyword currentKeyword)
    {
        if (keyword == "And" || keyword == "But")
        {
            return currentKeyword;
        }

        if (keyword == "Given")
        {
            currentKeyword = NormalizedKeyword.Given;
            return NormalizedKeyword.Given;
        }

        if (keyword == "When")
        {
            currentKeyword = NormalizedKeyword.When;
            return NormalizedKeyword.When;
        }

        if (keyword == "Then")
        {
            currentKeyword = NormalizedKeyword.Then;
            return NormalizedKeyword.Then;
        }

        return currentKeyword;
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
            var arguments = ExtractArguments(matchedStep.Text, step.Text, textParameters);
            foreach (var arg in arguments)
            {
                step.Arguments.Add(arg);
            }
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

        step.Method = ConvertToMethodName(step.Text);

        if (step.DataTable != null)
        {
            step.Arguments.Add(new ArgumentCrif
            {
                Value = step.DataTable.VariableName,
                Last = true
            });
        }
    }

    /// <summary>
    /// Adds step to unimplemented list if not already present.
    /// </summary>
    /// <param name="crif">The CRIF object to update.</param>
    /// <param name="step">The step to add.</param>
    /// <param name="normalizedKeyword">The normalized keyword.</param>
    private void AddToUnimplementedList(FeatureCrif crif, StepCrif step, NormalizedKeyword normalizedKeyword)
    {
        var existingUnimplemented = crif.Unimplemented.FirstOrDefault(u =>
            u.Text == step.Text && u.Keyword == normalizedKeyword);

        if (existingUnimplemented == null)
        {
            var unimplementedStep = new UnimplementedStepCrif
            {
                Keyword = normalizedKeyword,
                Text = step.Text,
                Method = ConvertToMethodName(step.Text),
                Parameters = []
            };

            if (step.DataTable != null)
            {
                unimplementedStep.Parameters.Add(new ParameterCrif
                {
                    Type = "DataTable",
                    Name = "table",
                    Last = true
                });
            }

            crif.Unimplemented.Add(unimplementedStep);
        }
    }

    private static List<ArgumentCrif> ExtractArguments(string pattern, string text, List<StepParameter> parameters)
    {
        var arguments = new List<ArgumentCrif>();
        var regexPattern = BuildRegexPattern(pattern);

        try
        {
            var regex = new System.Text.RegularExpressions.Regex(
                regexPattern,
                System.Text.RegularExpressions.RegexOptions.IgnoreCase
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
        var regexPattern = System.Text.RegularExpressions.Regex.Replace(
            pattern,
            @"\{[^}]+\}",
            "<<<PLACEHOLDER>>>"
        );

        // Escape the pattern for regex special characters
        regexPattern = System.Text.RegularExpressions.Regex.Escape(regexPattern);

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
    private static void ExtractArgumentsFromMatch(System.Text.RegularExpressions.Match match, List<StepParameter> parameters, List<ArgumentCrif> arguments)
    {
        // Groups[0] is the entire match, Groups[1..n] are capture groups
        var extractedArgs = match.Groups
            .Cast<System.Text.RegularExpressions.Group>()
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
}

/// <summary>
/// Collection of step definition metadata extracted from step classes.
/// </summary>
public class StepMetadataCollection
{
    private readonly List<StepMetadata> _steps = new();

    /// <summary>
    /// Adds step metadata to the collection.
    /// </summary>
    /// <param name="metadata">Step definition metadata to add.</param>
    public void Add(StepMetadata metadata)
    {
        _steps.Add(metadata);
    }

    /// <summary>
    /// Adds multiple step metadata items to the collection.
    /// </summary>
    /// <param name="metadataItems">Collection of step definition metadata to add.</param>
    public void AddRange(IEnumerable<StepMetadata> metadataItems)
    {
        _steps.AddRange(metadataItems);
    }

    /// <summary>
    /// Finds a step definition matching the given Gherkin step.
    /// </summary>
    /// <param name="normalizedKeyword">Normalized keyword (Given, When, or Then).</param>
    /// <param name="stepText">Step text from Gherkin scenario.</param>
    /// <returns>Matching step metadata, or null if no match found.</returns>
    public StepMetadata? FindMatch(NormalizedKeyword normalizedKeyword, string stepText)
    {
        // Filter by keyword first
        var candidates = _steps.Where(s => s.NormalizedKeyword == normalizedKeyword);

        // Try to find exact match first (no parameters)
        var exactMatch = candidates
            .Where(c => c.Parameters.Count == 0)
            .FirstOrDefault(c => c.Text.Equals(stepText, StringComparison.OrdinalIgnoreCase));

        if (exactMatch != null)
        {
            return exactMatch;
        }

        // Try to find match with placeholders
        return candidates
            .Where(c => c.Parameters.Count > 0)
            .FirstOrDefault(c => MatchesWithPlaceholders(c.Text, stepText));
    }

    private static bool MatchesWithPlaceholders(string pattern, string text)
    {
        // Build a regex pattern from the step definition text
        // Replace {placeholder} with a pattern that matches:
        // - Single words (no spaces): \S+
        // - Quoted phrases (can contain spaces): "[^"]*"
        // The pattern should match either quoted text OR non-whitespace

        // First, replace placeholders in the original pattern BEFORE escaping
        var regexPattern = System.Text.RegularExpressions.Regex.Replace(
            pattern,
            @"\{[^}]+\}",  // Match {placeholder} pattern
            "<<<PLACEHOLDER>>>"  // Temporary placeholder marker
        );

        // Now escape the pattern for regex
        regexPattern = System.Text.RegularExpressions.Regex.Escape(regexPattern);

        // Replace our markers with the actual regex pattern
        regexPattern = regexPattern.Replace(
            "<<<PLACEHOLDER>>>",
            @"(?:""[^""]*""|\S+)"
        );

        // Add anchors for full string match
        regexPattern = "^" + regexPattern + "$";

        try
        {
            var regex = new System.Text.RegularExpressions.Regex(
                regexPattern,
                System.Text.RegularExpressions.RegexOptions.IgnoreCase
            );
            return regex.IsMatch(text);
        }
        catch
        {
            return false;
        }
    }
}

/// <summary>
/// Metadata for a single step definition method.
/// </summary>
public class StepMetadata
{
    /// <summary>
    /// Normalized keyword (Given, When, or Then).
    /// </summary>
    public NormalizedKeyword NormalizedKeyword { get; set; }

    /// <summary>
    /// Step text pattern with placeholders (e.g., "I have {quantity} cars in my {place}").
    /// </summary>
    public string Text { get; set; } = string.Empty;

    /// <summary>
    /// Method name as defined in the step class.
    /// </summary>
    public string Method { get; set; } = string.Empty;

    /// <summary>
    /// Class name containing the step definition.
    /// </summary>
    public string Class { get; set; } = string.Empty;

    /// <summary>
    /// Namespace of the class.
    /// </summary>
    public string Namespace { get; set; } = string.Empty;

    /// <summary>
    /// Method parameters with types and names.
    /// </summary>
    public List<StepParameter> Parameters { get; set; } = [];
}

/// <summary>
/// Represents a parameter in a step definition method.
/// </summary>
public class StepParameter
{
    /// <summary>
    /// Parameter type (e.g., "string", "int", "DataTable").
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Parameter name.
    /// </summary>
    public string Name { get; set; } = string.Empty;
}
