using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Gherkin.Generator.Lib;

/// <summary>
/// Renders a Markdown catalog of all step bindings discovered by <see cref="StepMethodAnalyzer"/>.
/// </summary>
/// <remarks>
/// Steps are grouped by declaring source file into one table per file, with rows ordered by
/// keyword (Given/When/Then) then alphabetically by exact binding text. Aliased steps (multiple
/// attributes on one method) each appear as their own row, since the catalog describes which
/// phrases are available to match, not which methods implement them.
/// </remarks>
public static class StepCatalogBuilder
{
    private static readonly NormalizedKeyword[] KeywordOrder =
    [
        NormalizedKeyword.Given,
        NormalizedKeyword.When,
        NormalizedKeyword.Then
    ];

    private static readonly Regex PlaceholderPattern = new(@"\{[^}]+\}", RegexOptions.Compiled);

    /// <summary>
    /// Builds the Markdown step catalog from the given step metadata collection.
    /// </summary>
    /// <param name="steps">All step bindings discovered in the compilation.</param>
    /// <returns>Markdown text for the step catalog.</returns>
    public static string Build(StepMetadataCollection steps)
    {
        var builder = new StringBuilder();
        builder.AppendLine("# Step Catalog");

        var fileGroups = steps.All
            .GroupBy(s => s.SourceFile)
            .OrderBy(g => g.Key, StringComparer.OrdinalIgnoreCase);

        foreach (var fileGroup in fileGroups)
        {
            builder.AppendLine();
            builder.AppendLine($"## {fileGroup.Key}");
            builder.AppendLine();
            builder.AppendLine("| Keyword | Step | Implementation |");
            builder.AppendLine("| --- | --- | --- |");

            foreach (var keyword in KeywordOrder)
            {
                var entries = fileGroup
                    .Where(s => s.NormalizedKeyword == keyword)
                    .OrderBy(s => s.Text, StringComparer.OrdinalIgnoreCase);

                foreach (var entry in entries)
                {
                    var stepText = HighlightPlaceholders(entry.Text);
                    builder.AppendLine($"| {keyword} | {stepText} | `{entry.Class}.{entry.Method}` |");
                }
            }
        }

        return builder.ToString();
    }

    /// <summary>
    /// Wraps each <c>{placeholder}</c> in the step text with backticks so parameters stand out in the table.
    /// </summary>
    /// <param name="text">Raw step binding text.</param>
    /// <returns>Step text with placeholders wrapped in backticks.</returns>
    private static string HighlightPlaceholders(string text) =>
        PlaceholderPattern.Replace(text, m => $"`{m.Value}`");
}
