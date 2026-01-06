using Gherkin.Generator.Utils;

namespace Gherkin.Generator.Tests.Unit;

[TestFixture]
public class DataTableExtensionsTests
{
    [Test]
    public void GetKeyValue_ExistingKey_ReturnsValue()
    {
        // Given: A key-value table with standard Field/Value columns
        var table = new DataTable(
            ["Field", "Value"],
            ["Email", "user@example.com"],
            ["Password", "SecurePass123!"]
        );

        // When: Getting value for existing key
        var email = table.GetKeyValue("Email");
        var password = table.GetKeyValue("Password");

        // Then: Should return correct values
        Assert.That(email, Is.EqualTo("user@example.com"));
        Assert.That(password, Is.EqualTo("SecurePass123!"));
    }

    [Test]
    public void GetKeyValue_CustomColumns_ReturnsValue()
    {
        // Given: A key-value table with custom column names
        var table = new DataTable(
            ["Name", "Data"],
            ["Setting1", "Value1"],
            ["Setting2", "Value2"]
        );

        // When: Getting value with custom column names
        var value = table.GetKeyValue("Setting1", keyColumn: "Name", valueColumn: "Data");

        // Then: Should return correct value
        Assert.That(value, Is.EqualTo("Value1"));
    }

    [Test]
    public void GetKeyValue_NonExistingKey_ThrowsInvalidOperationException()
    {
        // Given: A key-value table
        var table = new DataTable(
            ["Field", "Value"],
            ["Email", "user@example.com"]
        );

        // When: Getting value for non-existing key
        // Then: InvalidOperationException should be thrown
        var ex = Assert.Throws<InvalidOperationException>(() => table.GetKeyValue("NonExistent"));
        Assert.That(ex!.Message, Does.Contain("NonExistent"));
        Assert.That(ex.Message, Does.Contain("Available keys"));
        Assert.That(ex.Message, Does.Contain("Email"));
    }

    [Test]
    public void TryGetKeyValue_ExistingKey_ReturnsTrue()
    {
        // Given: A key-value table
        var table = new DataTable(
            ["Field", "Value"],
            ["Email", "user@example.com"],
            ["Password", "SecurePass123!"]
        );

        // When: Trying to get existing key value
        var success = table.TryGetKeyValue("Email", out var value);

        // Then: Should return true and the value
        Assert.That(success, Is.True);
        Assert.That(value, Is.EqualTo("user@example.com"));
    }

    [Test]
    public void TryGetKeyValue_NonExistingKey_ReturnsFalse()
    {
        // Given: A key-value table
        var table = new DataTable(
            ["Field", "Value"],
            ["Email", "user@example.com"]
        );

        // When: Trying to get non-existing key value
        var success = table.TryGetKeyValue("NonExistent", out var value);

        // Then: Should return false and null value
        Assert.That(success, Is.False);
        Assert.That(value, Is.Null);
    }

    [Test]
    public void TryGetKeyValue_CustomColumns_ReturnsTrue()
    {
        // Given: A key-value table with custom column names
        var table = new DataTable(
            ["Name", "Data"],
            ["Setting1", "Value1"],
            ["Setting2", "Value2"]
        );

        // When: Trying to get value with custom column names
        var success = table.TryGetKeyValue("Setting2", out var value, keyColumn: "Name", valueColumn: "Data");

        // Then: Should return true and the value
        Assert.That(success, Is.True);
        Assert.That(value, Is.EqualTo("Value2"));
    }

    [Test]
    public void GetFirstColumn_MultiColumnTable_ReturnsFirstColumnValues()
    {
        // Given: A table with multiple columns
        var table = new DataTable(
            ["Username", "Role", "Status"],
            ["alice", "Owner", "Active"],
            ["bob", "Editor", "Inactive"],
            ["charlie", "Viewer", "Active"]
        );

        // When: Getting first column values
        var firstColumn = table.GetFirstColumn();

        // Then: Should return all values from first column
        Assert.That(firstColumn, Is.EqualTo(new[] { "alice", "bob", "charlie" }));
    }

    [Test]
    public void GetFirstColumn_SingleColumnTable_ReturnsAllValues()
    {
        // Given: A single-column table
        var table = new DataTable(
            ["Username"],
            ["alice"],
            ["bob"]
        );

        // When: Getting first column values
        var firstColumn = table.GetFirstColumn();

        // Then: Should return all values
        Assert.That(firstColumn, Is.EqualTo(new[] { "alice", "bob" }));
    }

    [Test]
    public void GetFirstColumn_EmptyTable_ReturnsEmptyCollection()
    {
        // Given: An empty table (no data rows)
        var table = new DataTable(
            ["Username"]
        );

        // When: Getting first column values
        var firstColumn = table.GetFirstColumn();

        // Then: Should return empty collection
        Assert.That(firstColumn, Is.Empty);
    }

    [Test]
    public void ToSingleColumnList_SingleColumnTable_ReturnsAllValues()
    {
        // Given: A single-column table
        var table = new DataTable(
            ["Username"],
            ["alice"],
            ["bob"],
            ["charlie"]
        );

        // When: Converting to single column list
        var usernames = table.ToSingleColumnList();

        // Then: Should return all values
        Assert.That(usernames, Is.EqualTo(new[] { "alice", "bob", "charlie" }));
    }

    [Test]
    public void ToSingleColumnList_MultiColumnTable_ThrowsInvalidOperationException()
    {
        // Given: A table with multiple columns
        var table = new DataTable(
            ["Username", "Role"],
            ["alice", "Owner"]
        );

        // When: Attempting to convert multi-column table to single column list
        // Then: InvalidOperationException should be thrown
        var ex = Assert.Throws<InvalidOperationException>(() => table.ToSingleColumnList());
        Assert.That(ex!.Message, Does.Contain("exactly one column"));
        Assert.That(ex.Message, Does.Contain("has 2"));
    }

    [Test]
    public void ToSingleColumnList_EmptyTable_ReturnsEmptyCollection()
    {
        // Given: An empty single-column table
        var table = new DataTable(
            ["Username"]
        );

        // When: Converting to single column list
        var usernames = table.ToSingleColumnList();

        // Then: Should return empty collection
        Assert.That(usernames, Is.Empty);
    }

    [Test]
    public void GetKeyValue_MultipleRowsWithSameKey_ThrowsInvalidOperationException()
    {
        // Given: A table with duplicate keys
        var table = new DataTable(
            ["Field", "Value"],
            ["Email", "first@example.com"],
            ["Email", "second@example.com"]
        );

        // When: Attempting to get value for duplicate key
        // Then: InvalidOperationException should be thrown (from SingleOrDefault)
        Assert.Throws<InvalidOperationException>(() => table.GetKeyValue("Email"));
    }

    [Test]
    public void Extensions_WorkWithLinqChaining()
    {
        // Given: A table suitable for GetFirstColumn
        var table = new DataTable(
            ["Username", "Role"],
            ["alice", "Owner"],
            ["bob", "Editor"],
            ["charlie", "Owner"]
        );

        // When: Chaining LINQ with extension method
        var firstColumn = table.GetFirstColumn();
        var sortedUsernames = firstColumn.OrderBy(u => u).ToList();

        // Then: Should work correctly
        Assert.That(sortedUsernames, Is.EqualTo(new[] { "alice", "bob", "charlie" }));
    }

    [Test]
    public void GetKeyValue_UsedInTestScenario_ProvidesCleanApi()
    {
        // Given: A typical Gherkin test data table for user credentials
        var credentials = new DataTable(
            ["Field", "Value"],
            ["Username", "testuser@example.com"],
            ["Password", "Test123!"],
            ["Role", "Administrator"]
        );

        // When: Extracting credentials using extension method
        var username = credentials.GetKeyValue("Username");
        var password = credentials.GetKeyValue("Password");
        var role = credentials.GetKeyValue("Role");

        // Then: Should provide clean, readable access to test data
        Assert.That(username, Is.EqualTo("testuser@example.com"));
        Assert.That(password, Is.EqualTo("Test123!"));
        Assert.That(role, Is.EqualTo("Administrator"));
    }

    [Test]
    public void ToSingleColumnList_UsedInTestScenario_ProvidesCleanApi()
    {
        // Given: A typical Gherkin test data table with list of items
        var usernames = new DataTable(
            ["Username"],
            ["alice"],
            ["bob"],
            ["charlie"]
        );

        // When: Converting to list for test validation
        var list = usernames.ToSingleColumnList();

        // Then: Should provide clean, type-safe collection
        Assert.That(list, Has.Count.EqualTo(3));
        Assert.That(list, Does.Contain("alice"));
        Assert.That(list, Does.Contain("bob"));
        Assert.That(list, Does.Contain("charlie"));
    }
}
