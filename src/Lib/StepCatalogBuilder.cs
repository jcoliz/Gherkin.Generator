using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gherkin.Generator.Lib;

/// <summary>
/// Renders a Markdown catalog of all step bindings discovered by <see cref="StepMethodAnalyzer"/>.
/// </summary>
/// <remarks>
/// Steps are grouped by declaring source file, then by keyword (Given/When/Then), then listed
/// alphabetically by exact binding text. Aliased steps (multiple attributes on one method) each
/// appear as their own entry, since the catalog describes which phrases are available to match,
/// not which methods implement them.
/// </remarks>
public static class StepCatalogBuilder
{
    private static readonly NormalizedKeyword[] KeywordOrder =
    [
        NormalizedKeyword.Given,
        NormalizedKeyword.When,
        NormalizedKeyword.Then
    ];

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

            foreach (var keyword in KeywordOrder)
            {
                var entries = fileGroup
                    .Where(s => s.NormalizedKeyword == keyword)
                    .OrderBy(s => s.Text, StringComparer.OrdinalIgnoreCase);

                var any = false;
                foreach (var entry in entries)
                {
                    if (!any)
                    {
                        builder.AppendLine();
                        any = true;
                    }

                    builder.AppendLine($"* {keyword} {entry.Text} (`{entry.Class}.{entry.Method}`)");
                }
            }
        }

        return builder.ToString();
    }
}
