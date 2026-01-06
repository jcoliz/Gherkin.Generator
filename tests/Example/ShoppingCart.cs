namespace Gherkin.Generator.Tests.Example;

/// <summary>
/// Simple shopping cart implementation for demonstration purposes.
/// </summary>
public class ShoppingCart
{
    private readonly Dictionary<string, CartItem> _items = new();

    /// <summary>
    /// Gets the total number of items in the cart.
    /// </summary>
    public int ItemCount => _items.Values.Sum(x => x.Quantity);

    /// <summary>
    /// Gets the total price of all items in the cart.
    /// </summary>
    public decimal Total => _items.Values.Sum(x => x.Price * x.Quantity);

    /// <summary>
    /// Adds an item to the cart.
    /// </summary>
    /// <param name="item">The name of the item to add.</param>
    /// <param name="price">The price of the item.</param>
    public void AddItem(string item, decimal price = 9.99m)
    {
        if (_items.TryGetValue(item, out var existing))
        {
            _items[item] = existing with { Quantity = existing.Quantity + 1 };
        }
        else
        {
            _items[item] = new CartItem(item, price, 1);
        }
    }

    /// <summary>
    /// Removes all instances of an item from the cart.
    /// </summary>
    /// <param name="item">The name of the item to remove.</param>
    public void RemoveItem(string item)
    {
        _items.Remove(item);
    }

    /// <summary>
    /// Checks if the cart contains a specific item.
    /// </summary>
    /// <param name="item">The name of the item to check.</param>
    /// <returns>True if the cart contains the item, false otherwise.</returns>
    public bool Contains(string item)
    {
        return _items.ContainsKey(item);
    }

    /// <summary>
    /// Clears all items from the cart.
    /// </summary>
    public void Clear()
    {
        _items.Clear();
    }
}

/// <summary>
/// Represents an item in the shopping cart.
/// </summary>
/// <param name="Name">The name of the item.</param>
/// <param name="Price">The price per unit.</param>
/// <param name="Quantity">The quantity in the cart.</param>
public record CartItem(string Name, decimal Price, int Quantity);
