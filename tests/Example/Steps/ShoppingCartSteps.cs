using Gherkin.Generator.Utils;

namespace Gherkin.Generator.Tests.Example.Steps;

/// <summary>
/// Step definitions for shopping cart scenarios.
/// </summary>
public class ShoppingCartSteps
{
    private readonly FunctionalTestBase _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="ShoppingCartSteps"/> class.
    /// </summary>
    /// <param name="context">The test context providing access to the shopping cart.</param>
    public ShoppingCartSteps(FunctionalTestBase context)
    {
        _context = context;
    }

    /// <summary>
    /// Adds a single item to the cart.
    /// </summary>
    /// <param name="item">The name of the item to add.</param>
    [When("I add {item} to the cart")]
    public async Task AddToCart(string item)
    {
        _context.Cart.AddItem(item);
        await Task.CompletedTask;
    }

    /// <summary>
    /// Adds multiple items to the cart.
    /// </summary>
    /// <param name="quantity">The number of items to add.</param>
    /// <param name="item">The name of the item to add.</param>
    [When("I add {quantity} {item} to the cart")]
    public async Task AddMultipleToCart(string quantity, string item)
    {
        // TODO: Scenario outlines only accept string parameters currently
        // Would be better to accept int parameters directly
        var count = int.Parse(quantity);
        for (int i = 0; i < count; i++)
        {
            _context.Cart.AddItem(item);
        }
        await Task.CompletedTask;
    }

    /// <summary>
    /// Removes an item from the cart.
    /// </summary>
    /// <param name="item">The name of the item to remove.</param>
    [When("I remove {item} from the cart")]
    public async Task RemoveFromCart(string item)
    {
        _context.Cart.RemoveItem(item);
        await Task.CompletedTask;
    }

    /// <summary>
    /// When I clear the cart
    /// </summary>
    [When("I clear the cart")]
    public async Task IClearTheCart()
    {
        _context.Cart.Clear();
        await Task.CompletedTask;
    }

    /// <summary>
    /// Verifies the cart contains the expected number of items.
    /// </summary>
    /// <param name="quantity">The expected number of items.</param>
    [Then("the cart should contain {quantity} item")]
    [Then("the cart should contain {quantity} items")]
    public async Task CartShouldContainItems(string quantity)
    {
        // TODO: Generator has issue with scenario outline parameter substitution
        var expectedCount = int.Parse(quantity);
        Assert.That(_context.Cart.ItemCount, Is.EqualTo(expectedCount));
        await Task.CompletedTask;
    }

    /// <summary>
    /// Verifies the cart total matches the expected amount.
    /// </summary>
    /// <remarks>
    /// Uses string parameter to avoid having to force gherkin-writing user to specify decimal literals
    /// e.g. this allows "9.99" instead of "9.99m".
    /// </remarks>
    /// <param name="total">The expected total.</param>
    [Then("the cart total should be {total}")]
    public async Task CartTotalShouldBe(string total)
    {
        // TODO: Could be an interesting feature to have the generator convert to decimal automatically
        // when it detects a decimal parameter type from the method signature.
        var expectedTotal = decimal.Parse(total);
        Assert.That(_context.Cart.Total, Is.EqualTo(expectedTotal));
        await Task.CompletedTask;
    }

    /// <summary>
    /// Verifies the cart does not contain a specific item.
    /// </summary>
    /// <param name="item">The name of the item that should not be in the cart.</param>
    [Then("the cart should not contain {item}")]
    public async Task CartShouldNotContainItem(string item)
    {
        Assert.That(_context.Cart.Contains(item), Is.False);
        await Task.CompletedTask;
    }

    /// <summary>
    /// Sets up the cart with specific items and quantities.
    /// </summary>
    /// <param name="table">A data table with Item and Quantity columns.</param>
    [Given("the cart contains:")]
    public async Task CartContains(DataTable table)
    {
        foreach (var row in table.Rows)
        {
            var item = row["Item"];
            var quantity = int.Parse(row["Quantity"]);
            
            for (int i = 0; i < quantity; i++)
            {
                _context.Cart.AddItem(item);
            }
        }
        await Task.CompletedTask;
    }

}
