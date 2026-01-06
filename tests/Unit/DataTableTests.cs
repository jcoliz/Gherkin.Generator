using Gherkin.Generator.Utils;

namespace Gherkin.Generator.Tests.Unit;

[TestFixture]
public class DataTableTests
{
    [Test]
    public void Constructor_ValidHeadersAndRows_CreatesTable()
    {
        // Given: Valid headers and data rows

        // When: Creating a new DataTable
        var table = new DataTable(
            ["Username", "Role"],
            ["alice", "Owner"],
            ["bob", "Editor"]
        );

        // Then: Table should be created successfully
        Assert.That(table.RowCount, Is.EqualTo(2));
        Assert.That(table.ColumnCount, Is.EqualTo(2));
        Assert.That(table.Headers, Is.EqualTo(new[] { "Username", "Role" }));
    }

    [Test]
    public void Constructor_SingleColumn_CreatesTable()
    {
        // Given: Single column table definition

        // When: Creating a single-column table
        var table = new DataTable(
            ["Username"],
            ["alice"],
            ["bob"]
        );

        // Then: Table should be created with one column
        Assert.That(table.ColumnCount, Is.EqualTo(1));
        Assert.That(table.RowCount, Is.EqualTo(2));
    }

    [Test]
    public void Constructor_EmptyHeaders_ThrowsArgumentException()
    {
        // Given: Empty headers array

        // When: Attempting to create a table with empty headers
        // Then: ArgumentException should be thrown
        var ex = Assert.Throws<ArgumentException>(() => new DataTable(
            [],
            ["data"]
        ));
        Assert.That(ex!.ParamName, Is.EqualTo("headers"));
    }

    [Test]
    public void Constructor_NullHeaders_ThrowsArgumentException()
    {
        // Given: Null headers

        // When: Attempting to create a table with null headers
        // Then: ArgumentException should be thrown
        var ex = Assert.Throws<ArgumentException>(() => new DataTable(
            null!,
            ["data"]
        ));
        Assert.That(ex!.ParamName, Is.EqualTo("headers"));
    }

    [Test]
    public void Constructor_InconsistentColumnCount_ThrowsArgumentException()
    {
        // Given: Data rows with inconsistent column counts

        // When: Attempting to create a table with mismatched columns
        // Then: ArgumentException should be thrown
        var ex = Assert.Throws<ArgumentException>(() => new DataTable(
            ["Username", "Role"],
            ["alice", "Owner"],
            ["bob"] // Missing second column
        ));
        Assert.That(ex!.Message, Does.Contain("expected 2"));
        Assert.That(ex.ParamName, Is.EqualTo("dataRows"));
    }

    [Test]
    public void IndexerByInt_ValidIndex_ReturnsRow()
    {
        // Given: A table with multiple rows
        var table = new DataTable(
            ["Username", "Role"],
            ["alice", "Owner"],
            ["bob", "Editor"]
        );

        // When: Accessing row by index
        var row = table[0];

        // Then: Should return the correct row
        Assert.That(row["Username"], Is.EqualTo("alice"));
        Assert.That(row["Role"], Is.EqualTo("Owner"));
    }

    [Test]
    public void IndexerByInt_InvalidIndex_ThrowsIndexOutOfRangeException()
    {
        // Given: A table with 2 rows
        var table = new DataTable(
            ["Username"],
            ["alice"],
            ["bob"]
        );

        // When: Accessing invalid index
        // Then: IndexOutOfRangeException should be thrown
        Assert.Throws<ArgumentOutOfRangeException>(() => { var _ = table[5]; });
    }

    [Test]
    public void HasColumn_ExistingColumn_ReturnsTrue()
    {
        // Given: A table with specific columns
        var table = new DataTable(
            ["Username", "Role"],
            ["alice", "Owner"]
        );

        // When: Checking for existing column
        var hasColumn = table.HasColumn("Role");

        // Then: Should return true
        Assert.That(hasColumn, Is.True);
    }

    [Test]
    public void HasColumn_NonExistingColumn_ReturnsFalse()
    {
        // Given: A table with specific columns
        var table = new DataTable(
            ["Username", "Role"],
            ["alice", "Owner"]
        );

        // When: Checking for non-existing column
        var hasColumn = table.HasColumn("Email");

        // Then: Should return false
        Assert.That(hasColumn, Is.False);
    }

    [Test]
    public void GetColumn_ValidColumn_ReturnsAllValues()
    {
        // Given: A table with multiple rows
        var table = new DataTable(
            ["Username", "Role"],
            ["alice", "Owner"],
            ["bob", "Editor"],
            ["charlie", "Viewer"]
        );

        // When: Getting all values from Username column
        var usernames = table.GetColumn("Username");

        // Then: Should return all username values
        Assert.That(usernames, Is.EqualTo(new[] { "alice", "bob", "charlie" }));
    }

    [Test]
    public void GetColumn_InvalidColumn_ThrowsArgumentException()
    {
        // Given: A table with specific columns
        var table = new DataTable(
            ["Username", "Role"],
            ["alice", "Owner"]
        );

        // When: Getting non-existing column
        // Then: ArgumentException should be thrown
        var ex = Assert.Throws<ArgumentException>(() => table.GetColumn("Email"));
        Assert.That(ex!.Message, Does.Contain("Email"));
        Assert.That(ex.Message, Does.Contain("Available columns"));
    }

    [Test]
    public void Enumeration_LinqQueries_WorksCorrectly()
    {
        // Given: A table with multiple rows
        var table = new DataTable(
            ["Username", "Role"],
            ["alice", "Owner"],
            ["bob", "Editor"],
            ["charlie", "Owner"]
        );

        // When: Querying with LINQ
        var owners = table.Where(row => row["Role"] == "Owner").ToList();

        // Then: Should return filtered rows
        Assert.That(owners, Has.Count.EqualTo(2));
        Assert.That(owners[0]["Username"], Is.EqualTo("alice"));
        Assert.That(owners[1]["Username"], Is.EqualTo("charlie"));
    }

    [Test]
    public void Enumeration_First_ReturnsFirstRow()
    {
        // Given: A table with multiple rows
        var table = new DataTable(
            ["Username"],
            ["alice"],
            ["bob"]
        );

        // When: Getting first row
        var first = table.First();

        // Then: Should return first row
        Assert.That(first["Username"], Is.EqualTo("alice"));
    }

    [Test]
    public void Rows_Property_ReturnsReadOnlyList()
    {
        // Given: A table with rows
        var table = new DataTable(
            ["Username"],
            ["alice"],
            ["bob"]
        );

        // When: Accessing Rows property
        var rows = table.Rows;

        // Then: Should return read-only list with correct count
        Assert.That(rows, Has.Count.EqualTo(2));
        Assert.That(rows[0]["Username"], Is.EqualTo("alice"));
        Assert.That(rows[1]["Username"], Is.EqualTo("bob"));
    }

    [Test]
    public void ToString_CreatesReadableRepresentation()
    {
        // Given: A table with data
        var table = new DataTable(
            ["Name", "Age"],
            ["Alice", "30"],
            ["Bob", "25"]
        );

        // When: Converting to string
        var result = table.ToString();

        // Then: Should contain table structure information
        Assert.That(result, Does.Contain("2 rows"));
        Assert.That(result, Does.Contain("2 columns"));
        Assert.That(result, Does.Contain("Name"));
        Assert.That(result, Does.Contain("Age"));
        Assert.That(result, Does.Contain("Alice"));
        Assert.That(result, Does.Contain("Bob"));
    }
}

[TestFixture]
public class DataTableRowTests
{
    [Test]
    public void IndexerByColumnName_ValidColumn_ReturnsValue()
    {
        // Given: A table row with data
        var table = new DataTable(
            ["Username", "Role"],
            ["alice", "Owner"]
        );
        var row = table[0];

        // When: Accessing column by name
        var username = row["Username"];
        var role = row["Role"];

        // Then: Should return correct values
        Assert.That(username, Is.EqualTo("alice"));
        Assert.That(role, Is.EqualTo("Owner"));
    }

    [Test]
    public void IndexerByColumnName_InvalidColumn_ThrowsArgumentException()
    {
        // Given: A table row with specific columns
        var table = new DataTable(
            ["Username", "Role"],
            ["alice", "Owner"]
        );
        var row = table[0];

        // When: Accessing non-existing column
        // Then: ArgumentException should be thrown
        var ex = Assert.Throws<ArgumentException>(() => { var _ = row["Email"]; });
        Assert.That(ex!.Message, Does.Contain("Email"));
        Assert.That(ex.Message, Does.Contain("Available columns"));
    }

    [Test]
    public void IndexerByInt_ValidIndex_ReturnsValue()
    {
        // Given: A table row with data
        var table = new DataTable(
            ["Username", "Role"],
            ["alice", "Owner"]
        );
        var row = table[0];

        // When: Accessing column by index
        var value0 = row[0];
        var value1 = row[1];

        // Then: Should return correct values
        Assert.That(value0, Is.EqualTo("alice"));
        Assert.That(value1, Is.EqualTo("Owner"));
    }

    [Test]
    public void IndexerByInt_InvalidIndex_ThrowsIndexOutOfRangeException()
    {
        // Given: A table row with 2 columns
        var table = new DataTable(
            ["Username", "Role"],
            ["alice", "Owner"]
        );
        var row = table[0];

        // When: Accessing invalid index
        // Then: IndexOutOfRangeException should be thrown
        Assert.Throws<ArgumentOutOfRangeException>(() => { var _ = row[5]; });
    }

    [Test]
    public void HasColumn_ExistingColumn_ReturnsTrue()
    {
        // Given: A table row with specific columns
        var table = new DataTable(
            ["Username", "Role"],
            ["alice", "Owner"]
        );
        var row = table[0];

        // When: Checking for existing column
        var hasColumn = row.HasColumn("Role");

        // Then: Should return true
        Assert.That(hasColumn, Is.True);
    }

    [Test]
    public void HasColumn_NonExistingColumn_ReturnsFalse()
    {
        // Given: A table row with specific columns
        var table = new DataTable(
            ["Username", "Role"],
            ["alice", "Owner"]
        );
        var row = table[0];

        // When: Checking for non-existing column
        var hasColumn = row.HasColumn("Email");

        // Then: Should return false
        Assert.That(hasColumn, Is.False);
    }

    [Test]
    public void TryGetValue_ExistingColumn_ReturnsTrue()
    {
        // Given: A table row with data
        var table = new DataTable(
            ["Username", "Role"],
            ["alice", "Owner"]
        );
        var row = table[0];

        // When: Trying to get existing column value
        var success = row.TryGetValue("Username", out var value);

        // Then: Should return true and the value
        Assert.That(success, Is.True);
        Assert.That(value, Is.EqualTo("alice"));
    }

    [Test]
    public void TryGetValue_NonExistingColumn_ReturnsFalse()
    {
        // Given: A table row with specific columns
        var table = new DataTable(
            ["Username", "Role"],
            ["alice", "Owner"]
        );
        var row = table[0];

        // When: Trying to get non-existing column value
        var success = row.TryGetValue("Email", out var value);

        // Then: Should return false and null value
        Assert.That(success, Is.False);
        Assert.That(value, Is.Null);
    }

    [Test]
    public void Values_Property_ReturnsAllValues()
    {
        // Given: A table row with data
        var table = new DataTable(
            ["Username", "Role", "Status"],
            ["alice", "Owner", "Active"]
        );
        var row = table[0];

        // When: Accessing Values property
        var values = row.Values;

        // Then: Should return all values in order
        Assert.That(values, Is.EqualTo(new[] { "alice", "Owner", "Active" }));
    }

    [Test]
    public void ToString_CreatesReadableRepresentation()
    {
        // Given: A table row with data
        var table = new DataTable(
            ["Username", "Role"],
            ["alice", "Owner"]
        );
        var row = table[0];

        // When: Converting to string
        var result = row.ToString();

        // Then: Should contain values separated by pipe
        Assert.That(result, Does.Contain("alice"));
        Assert.That(result, Does.Contain("Owner"));
        Assert.That(result, Does.Contain("|"));
    }
}
