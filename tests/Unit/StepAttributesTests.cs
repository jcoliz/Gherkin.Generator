using System.Reflection;
using Gherkin.Generator.Utils;

namespace Gherkin.Generator.Tests.Unit;

[TestFixture]
public class StepAttributesTests
{
    // Test class with step methods for attribute testing
    private class TestSteps
    {
        [Given("I have a user with name {name}")]
        public void GivenUserWithName(string name) { }

        [When("I click the {button} button")]
        public void WhenClickButton(string button) { }

        [Then("I should see {message} on the page")]
        public void ThenSeeMessage(string message) { }

        [Given("there is a product with price {price}")]
        [Given("there is an item with price {price}")]
        public void GivenProductWithPrice(decimal price) { }

        [When("I add {quantity} items to cart")]
        [When("I add {quantity} products to cart")]
        public void WhenAddToCart(int quantity) { }

        [Then("the total should be {amount}")]
        [Then("the sum should be {amount}")]
        public void ThenTotal(decimal amount) { }

        public void NoAttribute() { }
    }

    [Test]
    public void GivenAttribute_AppliedToMethod_PatternIsAccessible()
    {
        // Given: A method decorated with GivenAttribute
        var method = typeof(TestSteps).GetMethod(nameof(TestSteps.GivenUserWithName))!;

        // When: Retrieving the attribute
        var attribute = method.GetCustomAttribute<GivenAttribute>();

        // Then: Attribute should be present with correct pattern
        Assert.That(attribute, Is.Not.Null);
        Assert.That(attribute!.Pattern, Is.EqualTo("I have a user with name {name}"));
    }

    [Test]
    public void WhenAttribute_AppliedToMethod_PatternIsAccessible()
    {
        // Given: A method decorated with WhenAttribute
        var method = typeof(TestSteps).GetMethod(nameof(TestSteps.WhenClickButton))!;

        // When: Retrieving the attribute
        var attribute = method.GetCustomAttribute<WhenAttribute>();

        // Then: Attribute should be present with correct pattern
        Assert.That(attribute, Is.Not.Null);
        Assert.That(attribute!.Pattern, Is.EqualTo("I click the {button} button"));
    }

    [Test]
    public void ThenAttribute_AppliedToMethod_PatternIsAccessible()
    {
        // Given: A method decorated with ThenAttribute
        var method = typeof(TestSteps).GetMethod(nameof(TestSteps.ThenSeeMessage))!;

        // When: Retrieving the attribute
        var attribute = method.GetCustomAttribute<ThenAttribute>();

        // Then: Attribute should be present with correct pattern
        Assert.That(attribute, Is.Not.Null);
        Assert.That(attribute!.Pattern, Is.EqualTo("I should see {message} on the page"));
    }

    [Test]
    public void GivenAttribute_MultipleOnSameMethod_AllPatternsAccessible()
    {
        // Given: A method with multiple GivenAttribute decorations
        var method = typeof(TestSteps).GetMethod(nameof(TestSteps.GivenProductWithPrice))!;

        // When: Retrieving all attributes
        var attributes = method.GetCustomAttributes<GivenAttribute>().ToList();

        // Then: Should have both patterns
        Assert.That(attributes, Has.Count.EqualTo(2));
        Assert.That(attributes.Select(a => a.Pattern), Contains.Item("there is a product with price {price}"));
        Assert.That(attributes.Select(a => a.Pattern), Contains.Item("there is an item with price {price}"));
    }

    [Test]
    public void WhenAttribute_MultipleOnSameMethod_AllPatternsAccessible()
    {
        // Given: A method with multiple WhenAttribute decorations
        var method = typeof(TestSteps).GetMethod(nameof(TestSteps.WhenAddToCart))!;

        // When: Retrieving all attributes
        var attributes = method.GetCustomAttributes<WhenAttribute>().ToList();

        // Then: Should have both patterns
        Assert.That(attributes, Has.Count.EqualTo(2));
        Assert.That(attributes.Select(a => a.Pattern), Contains.Item("I add {quantity} items to cart"));
        Assert.That(attributes.Select(a => a.Pattern), Contains.Item("I add {quantity} products to cart"));
    }

    [Test]
    public void ThenAttribute_MultipleOnSameMethod_AllPatternsAccessible()
    {
        // Given: A method with multiple ThenAttribute decorations
        var method = typeof(TestSteps).GetMethod(nameof(TestSteps.ThenTotal))!;

        // When: Retrieving all attributes
        var attributes = method.GetCustomAttributes<ThenAttribute>().ToList();

        // Then: Should have both patterns
        Assert.That(attributes, Has.Count.EqualTo(2));
        Assert.That(attributes.Select(a => a.Pattern), Contains.Item("the total should be {amount}"));
        Assert.That(attributes.Select(a => a.Pattern), Contains.Item("the sum should be {amount}"));
    }

    [Test]
    public void StepAttributes_MethodWithoutAttribute_ReturnsNull()
    {
        // Given: A method without step attributes
        var method = typeof(TestSteps).GetMethod(nameof(TestSteps.NoAttribute))!;

        // When: Retrieving step attributes
        var givenAttr = method.GetCustomAttribute<GivenAttribute>();
        var whenAttr = method.GetCustomAttribute<WhenAttribute>();
        var thenAttr = method.GetCustomAttribute<ThenAttribute>();

        // Then: All should be null
        Assert.That(givenAttr, Is.Null);
        Assert.That(whenAttr, Is.Null);
        Assert.That(thenAttr, Is.Null);
    }

    [Test]
    public void GivenAttribute_InheritsFromAttribute()
    {
        // Given: GivenAttribute type

        // When: Checking inheritance
        var isAttribute = typeof(Attribute).IsAssignableFrom(typeof(GivenAttribute));

        // Then: Should inherit from Attribute
        Assert.That(isAttribute, Is.True);
    }

    [Test]
    public void WhenAttribute_InheritsFromAttribute()
    {
        // Given: WhenAttribute type

        // When: Checking inheritance
        var isAttribute = typeof(Attribute).IsAssignableFrom(typeof(WhenAttribute));

        // Then: Should inherit from Attribute
        Assert.That(isAttribute, Is.True);
    }

    [Test]
    public void ThenAttribute_InheritsFromAttribute()
    {
        // Given: ThenAttribute type

        // When: Checking inheritance
        var isAttribute = typeof(Attribute).IsAssignableFrom(typeof(ThenAttribute));

        // Then: Should inherit from Attribute
        Assert.That(isAttribute, Is.True);
    }

    [Test]
    public void GivenAttribute_AttributeUsage_AllowsMultiple()
    {
        // Given: GivenAttribute type

        // When: Getting AttributeUsage
        var usage = typeof(GivenAttribute).GetCustomAttribute<AttributeUsageAttribute>();

        // Then: Should allow multiple
        Assert.That(usage, Is.Not.Null);
        Assert.That(usage!.AllowMultiple, Is.True);
        Assert.That(usage.ValidOn, Is.EqualTo(AttributeTargets.Method));
    }

    [Test]
    public void WhenAttribute_AttributeUsage_AllowsMultiple()
    {
        // Given: WhenAttribute type

        // When: Getting AttributeUsage
        var usage = typeof(WhenAttribute).GetCustomAttribute<AttributeUsageAttribute>();

        // Then: Should allow multiple
        Assert.That(usage, Is.Not.Null);
        Assert.That(usage!.AllowMultiple, Is.True);
        Assert.That(usage.ValidOn, Is.EqualTo(AttributeTargets.Method));
    }

    [Test]
    public void ThenAttribute_AttributeUsage_AllowsMultiple()
    {
        // Given: ThenAttribute type

        // When: Getting AttributeUsage
        var usage = typeof(ThenAttribute).GetCustomAttribute<AttributeUsageAttribute>();

        // Then: Should allow multiple
        Assert.That(usage, Is.Not.Null);
        Assert.That(usage!.AllowMultiple, Is.True);
        Assert.That(usage.ValidOn, Is.EqualTo(AttributeTargets.Method));
    }

    [Test]
    public void StepAttributes_CanBeUsedToFindAllStepMethods()
    {
        // Given: A type with step methods
        var type = typeof(TestSteps);

        // When: Finding all methods with step attributes
        var givenMethods = type.GetMethods()
            .Where(m => m.GetCustomAttributes<GivenAttribute>().Any())
            .ToList();
        var whenMethods = type.GetMethods()
            .Where(m => m.GetCustomAttributes<WhenAttribute>().Any())
            .ToList();
        var thenMethods = type.GetMethods()
            .Where(m => m.GetCustomAttributes<ThenAttribute>().Any())
            .ToList();

        // Then: Should find the correct number of methods
        Assert.That(givenMethods, Has.Count.EqualTo(2));
        Assert.That(whenMethods, Has.Count.EqualTo(2));
        Assert.That(thenMethods, Has.Count.EqualTo(2));
    }

    [Test]
    public void StepAttributes_PatternWithMultiplePlaceholders_IsPreserved()
    {
        // Given: An attribute with multiple placeholders
        var attribute = new GivenAttribute("I have {count} items with price {price} each");

        // When: Accessing the pattern
        var pattern = attribute.Pattern;

        // Then: Pattern should be preserved exactly
        Assert.That(pattern, Is.EqualTo("I have {count} items with price {price} each"));
    }

    [Test]
    public void StepAttributes_PatternWithoutPlaceholders_IsValid()
    {
        // Given: Attributes without placeholders
        var givenAttr = new GivenAttribute("I am logged in");
        var whenAttr = new WhenAttribute("I click submit");
        var thenAttr = new ThenAttribute("the page should load");

        // When: Accessing patterns
        var givenPattern = givenAttr.Pattern;
        var whenPattern = whenAttr.Pattern;
        var thenPattern = thenAttr.Pattern;

        // Then: Patterns should be preserved
        Assert.That(givenPattern, Is.EqualTo("I am logged in"));
        Assert.That(whenPattern, Is.EqualTo("I click submit"));
        Assert.That(thenPattern, Is.EqualTo("the page should load"));
    }
}
