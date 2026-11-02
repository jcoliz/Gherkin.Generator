using Gherkin.Generator.Utils;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

namespace Gherkin.Generator.Tests.Unit;

public class SharedStateGenerationTests
{
    [TestCase("\"LoggedInAs\"", "\"authenticated user\"")]
    [TestCase("ObjectStoreKeys.LoggedInAs", "ObjectStoreKeys.Description")]
    [TestCase("nameof(ObjectStoreKeys.LoggedInAs)", "ObjectStoreKeys.Description")]
    [TestCase("\"Logged\" + \"InAs\"", "\"authenticated \" + \"user\"")]
    public void Generate_WithSharedStateExpressions_EmitsComments(string name, string description)
    {
        var source = $$"""
            using Gherkin.Generator.Utils;

            namespace Example;

            public class ApplicationSteps
            {
                [Given("the application is running")]
                [Provides("ApplicationIsRunning")]
                public void ApplicationIsRunning() { }

                [Given("I am logged in")]
                [Requires({{name}}, {{description}})]
                [Provides({{name}}, {{description}})]
                [Provides("Session")]
                public void LoggedInAs() { }
            }
            """;
        var keys = """
            namespace Example;

            public static class ObjectStoreKeys
            {
                public const string LoggedInAs = "LoggedInAs";
                public const string Description = "authenticated user";
            }
            """;
        var references = ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!)
            .Split(Path.PathSeparator)
            .Select(path => MetadataReference.CreateFromFile(path))
            .Append(MetadataReference.CreateFromFile(typeof(ProvidesAttribute).Assembly.Location));
        var compilation = CSharpCompilation.Create(
            "SharedStateTests",
            [CSharpSyntaxTree.ParseText(source), CSharpSyntaxTree.ParseText(keys)],
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        Assert.That(compilation.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error), Is.Empty);

        GeneratorDriver driver = CSharpGeneratorDriver.Create(
            generators: [new GherkinSourceGenerator().AsSourceGenerator()],
            additionalTexts: [new FeatureFile()]);
        var result = driver.RunGenerators(compilation).GetRunResult();
        Assert.That(result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error), Is.Empty);
        var generated = result.GeneratedTrees
            .Single(tree => tree.FilePath.EndsWith("ShoppingCart.feature.g.cs"))
            .GetText().ToString();

        Assert.That(generated, Does.Contain("// Provides ApplicationIsRunning"));
        Assert.That(generated, Does.Contain("// Requires LoggedInAs: authenticated user"));
        Assert.That(generated, Does.Contain("// Provides LoggedInAs: authenticated user"));
        Assert.That(generated, Does.Contain("// Provides Session"));
        Assert.That(generated, Does.Contain("// * Requires LoggedInAs: authenticated user"));
        Assert.That(generated, Does.Contain("// * Provides LoggedInAs: authenticated user"));
        Assert.That(generated, Does.Contain("// * Provides Session"));
        Assert.That(generated.IndexOf("// * Provides LoggedInAs", StringComparison.Ordinal),
            Is.GreaterThan(generated.IndexOf("ApplicationSteps.LoggedInAs();", StringComparison.Ordinal)));
    }

    private sealed class FeatureFile : AdditionalText
    {
        public override string Path => "ShoppingCart.feature";

        public override SourceText GetText(CancellationToken cancellationToken = default) =>
            SourceText.From("""
                Feature: Shopping Cart
                  Background:
                    Given the application is running
                    And I am logged in
                  Scenario: Review cart
                    Given the application is running
                    And I am logged in
                """);
    }
}
