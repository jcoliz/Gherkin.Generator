using Gherkin.Generator.Lib;
using Gherkin.Generator.Utils;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Gherkin.Generator.Tests.Unit;

public class StepMethodAnalyzerTests
{
    [Test]
    public void Analyze_WithGivenWhenThenMethods_ReturnsCompleteMetadata()
    {
        var compilation = CreateCompilation("""
            namespace Example.Steps;

            public class ShoppingSteps
            {
                [Given("I have {count} items")]
                [Requires("Cart", "shopping cart")]
                [Provides("Inventory")]
                public void AddItems(int count, string note) { }

                [WhenAttribute("I check out")]
                public void CheckOut() { }

                [Then("the total is shown")]
                public void VerifyTotal() { }
            }
            """, "ShoppingSteps.cs");

        var result = StepMethodAnalyzer.Analyze(compilation);

        Assert.That(result.All, Has.Count.EqualTo(3));
        var given = result.All.Single(s => s.NormalizedKeyword == NormalizedKeyword.Given);
        Assert.That(given.Text, Is.EqualTo("I have {count} items"));
        Assert.That(given.Method, Is.EqualTo("AddItems"));
        Assert.That(given.Class, Is.EqualTo("ShoppingSteps"));
        Assert.That(given.Namespace, Is.EqualTo("Example.Steps"));
        Assert.That(given.SourceFile, Is.EqualTo("ShoppingSteps.cs"));
        Assert.That(given.Parameters, Has.Count.EqualTo(2));
        Assert.That(given.Parameters[0].Type, Is.EqualTo("int"));
        Assert.That(given.Parameters[0].Name, Is.EqualTo("count"));
        Assert.That(given.Parameters[1].Type, Is.EqualTo("string"));
        Assert.That(given.RequiresState, Has.One.Property("Name").EqualTo("Cart"));
        Assert.That(given.RequiresState[0].Description, Is.EqualTo("shopping cart"));
        Assert.That(given.ProvidesState, Has.One.Property("Name").EqualTo("Inventory"));
        Assert.That(given.ProvidesState[0].Description, Is.Null);
        Assert.That(result.All.Any(s => s.NormalizedKeyword == NormalizedKeyword.When), Is.True);
        Assert.That(result.All.Any(s => s.NormalizedKeyword == NormalizedKeyword.Then), Is.True);
    }

    [Test]
    public void Analyze_WithUnrecognizedAttributesAndInvalidStepArguments_SkipsThem()
    {
        var compilation = CreateCompilation("""
            namespace Example;

            public class InvalidSteps
            {
                [Obsolete]
                public void NotAStep() { }

                [Given]
                public void MissingPattern() { }

                [Given(42)]
                public void NonStringPattern() { }

                [Given(Pattern)]
                public void ExpressionPattern() { }

                private const string Pattern = "not analyzed";
            }
            """, "InvalidSteps.cs");

        var result = StepMethodAnalyzer.Analyze(compilation);

        Assert.That(result.All, Is.Empty);
    }

    [Test]
    public void Analyze_WithMultipleStateAttributes_FiltersInvalidStateEntries()
    {
        var compilation = CreateCompilation("""
            namespace Example;

            public class StateSteps
            {
                [Given("valid step")]
                [Requires("ValidRequired", "required description")]
                [Requires("")]
                [Provides("ValidProvided")]
                [Provides("  ")]
                public void StateMethod() { }
            }
            """, "StateSteps.cs");

        var result = StepMethodAnalyzer.Analyze(compilation);

        var step = result.All.Single();
        Assert.That(step.RequiresState, Has.Count.EqualTo(1));
        Assert.That(step.RequiresState[0].Name, Is.EqualTo("ValidRequired"));
        Assert.That(step.RequiresState[0].Description, Is.EqualTo("required description"));
        Assert.That(step.ProvidesState, Has.Count.EqualTo(1));
        Assert.That(step.ProvidesState[0].Name, Is.EqualTo("ValidProvided"));
    }

    [Test]
    public void AnalyzeProjectMetadata_WithGeneratedTestBase_ReturnsCustomNamespaceAndBaseState()
    {
        var compilation = CreateCompilation("""
            namespace Example.Tests;

            [BaseProvides("DefaultUser", "seeded user")]
            [BaseProvides("Tester")]
            [GeneratedTestBase(UseNamespace = "Example.Generated")]
            public class FunctionalTestBase { }
            """, "FunctionalTestBase.cs");

        var result = StepMethodAnalyzer.AnalyzeProjectMetadata(compilation);

        Assert.That(result.DefaultTestBase, Is.Not.Null);
        Assert.That(result.DefaultTestBase!.ClassName, Is.EqualTo("FunctionalTestBase"));
        Assert.That(result.DefaultTestBase.Namespace, Is.EqualTo("Example.Tests"));
        Assert.That(result.DefaultTestBase.FullName, Is.EqualTo("Example.Tests.FunctionalTestBase"));
        Assert.That(result.GeneratedNamespace, Is.EqualTo("Example.Generated"));
        Assert.That(result.BaseProvidesState, Has.Count.EqualTo(2));
        Assert.That(result.BaseProvidesState[0].Name, Is.EqualTo("DefaultUser"));
        Assert.That(result.BaseProvidesState[0].Description, Is.EqualTo("seeded user"));
        Assert.That(result.BaseProvidesState[1].Name, Is.EqualTo("Tester"));
        Assert.That(result.BaseProvidesState[1].Description, Is.Null);
    }

    [Test]
    public void AnalyzeProjectMetadata_WithoutUseNamespace_DefaultsToBaseNamespace()
    {
        var compilation = CreateCompilation("""
            namespace Example.Tests;

            [GeneratedTestBase]
            public class FunctionalTestBase { }
            """, "FunctionalTestBase.cs");

        var result = StepMethodAnalyzer.AnalyzeProjectMetadata(compilation);

        Assert.That(result.GeneratedNamespace, Is.EqualTo("Example.Tests"));
        Assert.That(result.DefaultTestBase!.FullName, Is.EqualTo("Example.Tests.FunctionalTestBase"));
    }

    [Test]
    public void AnalyzeProjectMetadata_WithoutGeneratedTestBase_ReturnsEmptyMetadata()
    {
        var compilation = CreateCompilation("""
            namespace Example;

            public class OrdinaryClass { }
            """, "OrdinaryClass.cs");

        var result = StepMethodAnalyzer.AnalyzeProjectMetadata(compilation);

        Assert.That(result.DefaultTestBase, Is.Null);
        Assert.That(result.GeneratedNamespace, Is.EqualTo(string.Empty));
        Assert.That(result.BaseProvidesState, Is.Empty);
    }

    [Test]
    public void Analyze_EmptyCompilation_ReturnsEmptyCollections()
    {
        var compilation = CSharpCompilation.Create(
            "Empty",
            [],
            CreateReferences(),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        Assert.That(StepMethodAnalyzer.Analyze(compilation).All, Is.Empty);
        var projectMetadata = StepMethodAnalyzer.AnalyzeProjectMetadata(compilation);
        Assert.That(projectMetadata.DefaultTestBase, Is.Null);
        Assert.That(projectMetadata.BaseProvidesState, Is.Empty);
    }

    [Test]
    public void Analyze_WithNestedClassAndParameterlessMethod_CoversNamespaceAndParameterBranches()
    {
        var compilation = CreateCompilation("""
            namespace Example.Outer;

            public class Container
            {
                public class NestedSteps
                {
                    [When("I run a parameterless action")]
                    public void Run() { }

                    [ThenAttribute("I see decimal {amount}")]
                    public void Verify(decimal amount) { }
                }
            }
            """, "NestedSteps.cs");

        var result = StepMethodAnalyzer.Analyze(compilation);

        Assert.That(result.All, Has.Count.EqualTo(2));
        Assert.That(result.All[0].Class, Is.EqualTo("NestedSteps"));
        Assert.That(result.All[1].Parameters[0].Type, Is.EqualTo("decimal"));
    }

    private static CSharpCompilation CreateCompilation(string source, string fileName)
    {
        var sourceWithUsings = "using Gherkin.Generator.Utils;\n" + source;
        return CSharpCompilation.Create(
            "StepMethodAnalyzerTests",
            [CSharpSyntaxTree.ParseText(sourceWithUsings, path: fileName)],
            CreateReferences(),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
    }

    private static IEnumerable<MetadataReference> CreateReferences()
    {
        return ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!)
            .Split(Path.PathSeparator)
            .Select(path => MetadataReference.CreateFromFile(path))
            .Append(MetadataReference.CreateFromFile(typeof(GivenAttribute).Assembly.Location));
    }
}
