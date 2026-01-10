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
    private readonly StepProcessor _stepProcessor = new(stepMetadata);

    /// <summary>
    /// Converts a Gherkin feature document to a CRIF object.
    /// </summary>
    /// <param name="feature">Parsed Gherkin feature document.</param>
    /// <returns>Code-Ready Intermediate Form ready for template rendering.</returns>
    public FeatureCrif Convert(GherkinDocument feature)
    {
        return Convert(feature, string.Empty, null);
    }

    /// <summary>
    /// Converts a Gherkin feature document to a CRIF object with a specified filename.
    /// </summary>
    /// <param name="feature">Parsed Gherkin feature document.</param>
    /// <param name="fileName">Name of the feature file without extension (e.g., "BankImport").</param>
    /// <returns>Code-Ready Intermediate Form ready for template rendering.</returns>
    public FeatureCrif Convert(GherkinDocument feature, string fileName)
    {
        return Convert(feature, fileName, null);
    }

    /// <summary>
    /// Converts a Gherkin feature document to a CRIF object with a specified filename and project metadata.
    /// </summary>
    /// <param name="feature">Parsed Gherkin feature document.</param>
    /// <param name="fileName">Name of the feature file without extension (e.g., "BankImport").</param>
    /// <param name="projectMetadata">Project metadata for default namespace and base class.</param>
    /// <returns>Code-Ready Intermediate Form ready for template rendering.</returns>
    public FeatureCrif Convert(GherkinDocument feature, string fileName, ProjectMetadata? projectMetadata)
    {
        var crif = new FeatureCrif
        {
            FileName = fileName
        };

        if (feature.Feature != null)
        {
            ProcessFeatureMetadata(feature.Feature, crif);
            TagProcessor.ProcessFeatureTags(feature.Feature.Tags, crif);
            TagProcessor.ApplyProjectDefaults(crif, projectMetadata);
            ProcessBackground(feature.Feature, crif);
            ProcessFeatureChildren(feature.Feature.Children, crif);
            AddUtilsNamespaceIfNeeded(crif);
        }

        return crif;
    }

    /// <summary>
    /// Adds Gherkin.Generator.Utils namespace if the feature contains any DataTables or unimplemented steps.
    /// </summary>
    /// <param name="crif">The CRIF object to update.</param>
    private static void AddUtilsNamespaceIfNeeded(FeatureCrif crif)
    {
        var needsUtils = false;

        // Check background steps for DataTables
        if (crif.Background != null)
        {
            needsUtils = crif.Background.Steps.Any(s => s.DataTable != null);
        }

        // Check scenario steps for DataTables
        if (!needsUtils)
        {
            needsUtils = crif.Rules
                .SelectMany(r => r.Scenarios)
                .SelectMany(s => s.Steps)
                .Any(s => s.DataTable != null);
        }

        // Check for unimplemented steps
        if (!needsUtils)
        {
            needsUtils = crif.Unimplemented.Count > 0;
        }

        // Add Utils namespace if needed
        if (needsUtils && !crif.Usings.Contains("Gherkin.Generator.Utils"))
        {
            crif.Usings.Add("Gherkin.Generator.Utils");
        }
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
            _ = _stepProcessor.TrackUnimplementedSteps(crif, crif.Background.Steps);
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
        
        var hasUnmatchedSteps = _stepProcessor.TrackUnimplementedSteps(crif, scenarioCrif.Steps);
        
        // Mark scenario as explicit if it has unmatched steps
        if (hasUnmatchedSteps && !scenarioCrif.IsExplicit)
        {
            scenarioCrif.IsExplicit = true;
            scenarioCrif.ExplicitReason = "steps_in_progress";
        }
    }

    /// <summary>
    /// Converts a Gherkin background to CRIF format.
    /// </summary>
    /// <param name="background">The Gherkin background.</param>
    /// <returns>Background in CRIF format.</returns>
    private BackgroundCrif ConvertBackground(Background background)
    {
        var backgroundCrif = new BackgroundCrif();

        var tableCounter = 1;
        foreach (var step in background.Steps)
        {
            var stepCrif = _stepProcessor.ConvertStep(step);

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

    /// <summary>
    /// Converts a Gherkin scenario to CRIF format.
    /// </summary>
    /// <param name="scenario">The Gherkin scenario.</param>
    /// <returns>Scenario in CRIF format.</returns>
    private ScenarioCrif ConvertScenario(Scenario scenario)
    {
        var scenarioCrif = new ScenarioCrif
        {
            Name = scenario.Name,
            Method = StepProcessor.ConvertToMethodName(scenario.Name)
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

        // Check for @explicit tag with optional reason
        var explicitTag = scenario.Tags.FirstOrDefault(t => t.Name == "@explicit" || t.Name.StartsWith("@explicit:"));
        if (explicitTag != null)
        {
            scenarioCrif.IsExplicit = true;
            if (explicitTag.Name.StartsWith("@explicit:"))
            {
                scenarioCrif.ExplicitReason = explicitTag.Name.Substring("@explicit:".Length);
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
            var stepCrif = _stepProcessor.ConvertStep(step);

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
