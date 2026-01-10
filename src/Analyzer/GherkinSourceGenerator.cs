using System.IO;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Gherkin.Ast;
using Gherkin.Generator.Lib;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Gherkin.Generator;

/// <summary>
/// Incremental source generator that generates NUnit test code from Gherkin feature files.
/// </summary>
/// <remarks>
/// This generator:
/// - Discovers step definitions from C# source files using Roslyn analysis
/// - Processes .feature files from AdditionalFiles
/// - Uses embedded Default.mustache template by default
/// - Allows override with custom .mustache template in AdditionalFiles
/// - Generates test code automatically at build time
/// - Always produces compilable code (generates stubs for unimplemented steps)
/// </remarks>
[Generator]
public class GherkinSourceGenerator : IIncrementalGenerator
{
    private const string DiagnosticCategory = "Gherkin.Generator";
    private const string EmbeddedTemplateResourceName = "Gherkin.Generator.Templates.Default.mustache";

    /// <summary>
    /// Initializes the incremental generator pipeline.
    /// </summary>
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // 1. Collect Mustache templates from AdditionalFiles (any .mustache file)
        // If no custom template found, use embedded template
        var templateProvider = context.AdditionalTextsProvider
            .Where(file => file.Path.EndsWith(".mustache"))
            .Select((file, cancellationToken) => file.GetText(cancellationToken)?.ToString() ?? string.Empty)
            .Collect()
            .Select((templates, _) => templates.FirstOrDefault() ?? LoadEmbeddedTemplate());

        // 2. Collect .feature files from AdditionalFiles
        var featureFilesProvider = context.AdditionalTextsProvider
            .Where(file => file.Path.EndsWith(".feature"))
            .Select((file, cancellationToken) => new
            {
                FileName = System.IO.Path.GetFileNameWithoutExtension(file.Path),
                Content = file.GetText(cancellationToken)?.ToString() ?? string.Empty
            });

        // 3. Analyze compilation to discover step definitions and project metadata
        var stepMetadataProvider = context.CompilationProvider
            .Select((compilation, cancellationToken) =>
            {
                return StepMethodAnalyzer.Analyze(compilation);
            });

        var projectMetadataProvider = context.CompilationProvider
            .Select((compilation, cancellationToken) =>
            {
                return StepMethodAnalyzer.AnalyzeProjectMetadata(compilation);
            });

        // 4. Combine template, feature files, step metadata, and project metadata
        var combinedProvider = templateProvider
            .Combine(featureFilesProvider.Collect())
            .Combine(stepMetadataProvider)
            .Combine(projectMetadataProvider);

        // 5. Generate source for each feature file
        context.RegisterSourceOutput(combinedProvider, (spc, source) =>
        {
            var (((template, featureFiles), stepMetadata), projectMetadata) = source;

            // Template should always be available (either from AdditionalFiles or embedded)
            // But if somehow it's still empty, report an error
            if (string.IsNullOrEmpty(template))
            {
                spc.ReportDiagnostic(Diagnostic.Create(
                    new DiagnosticDescriptor(
                        "GHERKIN001",
                        "Missing Mustache Template",
                        "Failed to load template. This should not happen as an embedded template is included.",
                        DiagnosticCategory,
                        DiagnosticSeverity.Error,
                        isEnabledByDefault: true),
                    Microsoft.CodeAnalysis.Location.None));
                return;
            }

            // Process each feature file
            foreach (var featureFile in featureFiles)
            {
                try
                {
                    GenerateTestForFeature(spc, featureFile.FileName, featureFile.Content, template, stepMetadata, projectMetadata);
                }
                catch (System.Exception ex)
                {
                    spc.ReportDiagnostic(Diagnostic.Create(
                        new DiagnosticDescriptor(
                            "GHERKIN002",
                            "Feature Generation Error",
                            $"Error generating test for {featureFile.FileName}: {ex.Message}",
                            DiagnosticCategory,
                            DiagnosticSeverity.Error,
                            isEnabledByDefault: true),
                        Microsoft.CodeAnalysis.Location.None));
                }
            }
        });
    }

    /// <summary>
    /// Generates test code for a single feature file.
    /// </summary>
    private void GenerateTestForFeature(
        SourceProductionContext context,
        string fileName,
        string featureContent,
        string template,
        StepMetadataCollection stepMetadata,
        ProjectMetadata projectMetadata)
    {
        // 1. Parse Gherkin feature
        var parser = new Parser();
        GherkinDocument gherkinDocument;

        try
        {
            var reader = new System.IO.StringReader(featureContent);
            gherkinDocument = parser.Parse(reader);
        }
        catch (CompositeParserException ex)
        {
            // Extract all parser errors from the composite exception
            var errorMessages = new System.Collections.Generic.List<string>();
            foreach (var parserError in ex.Errors)
            {
                errorMessages.Add(parserError.Message);
            }
            var detailedMessage = string.Join(" | ", errorMessages);

            context.ReportDiagnostic(Diagnostic.Create(
                new DiagnosticDescriptor(
                    "GHERKIN003",
                    "Gherkin Parse Error",
                    "Error parsing {0}.feature: {1}",
                    DiagnosticCategory,
                    DiagnosticSeverity.Error,
                    isEnabledByDefault: true),
                Microsoft.CodeAnalysis.Location.None,
                fileName,
                detailedMessage));
            return;
        }
        catch (System.Exception ex)
        {
            // Handle other unexpected exceptions
            context.ReportDiagnostic(Diagnostic.Create(
                new DiagnosticDescriptor(
                    "GHERKIN003",
                    "Gherkin Parse Error",
                    "Error parsing {0}.feature: {1}",
                    DiagnosticCategory,
                    DiagnosticSeverity.Error,
                    isEnabledByDefault: true),
                Microsoft.CodeAnalysis.Location.None,
                fileName,
                ex.Message));
            return;
        }

        // 2. Convert Gherkin to CRIF
        var converter = new GherkinToCrifConverter(stepMetadata);
        var crif = converter.Convert(gherkinDocument, fileName, projectMetadata);

        // 3. Report warnings for unimplemented steps (optional)
        if (crif.Unimplemented.Any())
        {
            context.ReportDiagnostic(Diagnostic.Create(
                new DiagnosticDescriptor(
                    "GHERKIN004",
                    "Unimplemented Steps",
                    $"Feature '{fileName}' has {crif.Unimplemented.Count} unimplemented step(s). Stub implementations generated in output file.",
                    DiagnosticCategory,
                    DiagnosticSeverity.Warning,
                    isEnabledByDefault: true),
                Microsoft.CodeAnalysis.Location.None));
        }

        // 4. Generate JSON representation of CRIF for testing/debugging
        var jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };
        var crifJson = JsonSerializer.Serialize(crif, jsonOptions);
        var commentedJson = $"/*\n{crifJson}\n*/";
        var jsonSourceText = SourceText.From(commentedJson, Encoding.UTF8);
        context.AddSource($"{fileName}.crif.json.cs", jsonSourceText);

        // 5. Generate C# code from CRIF using template
        var generatedCode = FunctionalTestGenerator.GenerateString(template, crif);

        // 6. Add generated source to compilation
        var sourceText = SourceText.From(generatedCode, Encoding.UTF8);
        context.AddSource($"{fileName}.feature.g.cs", sourceText);
    }

    /// <summary>
    /// Loads the embedded Default.mustache template from the assembly resources.
    /// </summary>
    /// <returns>The template content as a string.</returns>
    private static string LoadEmbeddedTemplate()
    {
        var assembly = typeof(GherkinSourceGenerator).Assembly;
        using var stream = assembly.GetManifestResourceStream(EmbeddedTemplateResourceName);
        
        if (stream == null)
        {
            throw new InvalidOperationException(
                $"Embedded template resource '{EmbeddedTemplateResourceName}' not found in assembly.");
        }

        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}
