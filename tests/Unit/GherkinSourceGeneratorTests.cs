using Gherkin.Generator.Utils;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

namespace Gherkin.Generator.Tests.Unit;

public class GherkinSourceGeneratorTests
{
    [Test]
    public void Initialize_WithValidFeatureAndCustomTemplate_GeneratesSource()
    {
        var result = RunGenerator(
            [new TextAdditionalFile("Sample.feature", "Feature: Sample\n  Scenario: Works\n    Given something works")],
            [new TextAdditionalFile("Custom.mustache", "class {{FeatureName}}Generated { }")]);

        Assert.That(result.Diagnostics.Any(d => d.Id == "GHERKIN002"), Is.False);
        Assert.That(result.GeneratedTrees.Any(t => t.FilePath.EndsWith("Sample.feature.g.cs")), Is.True);
        Assert.That(result.GeneratedTrees.Single(t => t.FilePath.EndsWith("Sample.feature.g.cs")).GetText().ToString(),
            Does.Contain("class SampleGenerated"));
    }

    [Test]
    public void Initialize_WithEmptyCustomTemplate_ReportsMissingTemplateDiagnostic()
    {
        var result = RunGenerator(
            [new TextAdditionalFile("Sample.feature", "Feature: Sample\n  Scenario: Works\n    Given something works")],
            [new TextAdditionalFile("Custom.mustache", "")]);

        Assert.That(result.Diagnostics.Any(d => d.Id == "GHERKIN001"), Is.True);
        Assert.That(result.GeneratedTrees.Any(t => t.FilePath.EndsWith(".feature.g.cs")), Is.False);
    }

    [Test]
    public void Initialize_WithMalformedFeature_ReportsCompositeParseDiagnostic()
    {
        var result = RunGenerator(
            [new TextAdditionalFile("Broken.feature", "@ malformed tag\nFeature: Broken")]);

        var diagnostic = result.Diagnostics.Single(d => d.Id == "GHERKIN003");
        Assert.That(diagnostic.GetMessage(), Does.Contain("Broken.feature"));
        Assert.That(diagnostic.GetMessage(), Does.Contain("tag may not contain whitespace"));
        Assert.That(result.GeneratedTrees.Any(t => t.FilePath.EndsWith(".feature.g.cs")), Is.False);
    }

    [Test]
    public void Initialize_WithUnimplementedStep_ReportsUnimplementedWarning()
    {
        var result = RunGenerator(
            [new TextAdditionalFile("Warning.feature", "Feature: Warning\n  Scenario: Missing binding\n    Given this step has no implementation")]);

        Assert.That(result.Diagnostics.Any(d => d.Id == "GHERKIN004" && d.Severity == DiagnosticSeverity.Warning), Is.True);
        Assert.That(result.GeneratedTrees.Any(t => t.FilePath.EndsWith("Warning.feature.g.cs")), Is.True);
    }

    [Test]
    public void Initialize_WithInvalidMustacheTemplate_ReportsFeatureGenerationError()
    {
        var result = RunGenerator(
            [new TextAdditionalFile("Sample.feature", "Feature: Sample\n  Scenario: Works\n    Given something works")],
            [new TextAdditionalFile("Custom.mustache", "{{#Unclosed")]);

        Assert.That(result.Diagnostics.Any(d => d.Id == "GHERKIN002"), Is.True);
    }

    [Test]
    public void Initialize_WithNoAdditionalFiles_LoadsEmbeddedTemplateAndGeneratesCatalog()
    {
        var result = RunGenerator([]);

        Assert.That(result.GeneratedTrees.Any(t => t.FilePath.EndsWith("StepCatalog.g.cs")), Is.True);
    }

    private static GeneratorDriverRunResult RunGenerator(
        IEnumerable<AdditionalText> additionalTexts,
        IEnumerable<AdditionalText>? extraAdditionalTexts = null)
    {
        var source = """
            using Gherkin.Generator.Utils;

            namespace Example;

            public class SampleSteps
            {
                [Given("something works")]
                public void SomethingWorks() { }
            }
            """;
        var references = ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!)
            .Split(Path.PathSeparator)
            .Select(path => MetadataReference.CreateFromFile(path))
            .Append(MetadataReference.CreateFromFile(typeof(GivenAttribute).Assembly.Location));
        var compilation = CSharpCompilation.Create(
            "GherkinSourceGeneratorTests",
            [CSharpSyntaxTree.ParseText(source)],
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var allAdditionalTexts = additionalTexts.Concat(extraAdditionalTexts ?? []);
        GeneratorDriver driver = CSharpGeneratorDriver.Create(
            generators: [new GherkinSourceGenerator().AsSourceGenerator()],
            additionalTexts: allAdditionalTexts);

        return driver.RunGenerators(compilation).GetRunResult();
    }

    private sealed class TextAdditionalFile(string path, string text) : AdditionalText
    {
        public override string Path { get; } = path;

        public override SourceText GetText(CancellationToken cancellationToken = default) =>
            SourceText.From(text);
    }
}
