using Gherkin.Generator.Utils;

namespace Gherkin.Generator.Tests.Unit;

/// <summary>
/// Tests for DataTable and DataTableRow classes.
/// </summary>
[TestFixture]
public class DataTableClassTests
{
    [Test]
    public void Constructor_ValidData_CreatesTable()
    {
        // Given: Valid headers and data rows

        // When: Creating a DataTable
        var table = new DataTable(
            ["Name", "Age", "City"],
            ["Alice", "30", "Seattle"],
            ["Bob", "25", "Portland"]
        );

        // Then: Table should be created successfully
        Assert.That(table.Headers, Has.Count.EqualTo(3));
        Assert.That(table.RowCount, Is.EqualTo(2));
        Assert.That(table.ColumnCount, Is.EqualTo(3));
    }

    [Test]
    public void Constructor_NullHeaders_ThrowsArgumentException()
    {
        // Given: Null headers

        // When: Creating a DataTable with null headers
        // Then: ArgumentException should be thrown
        var ex = Assert.Throws<ArgumentException>(() => new DataTable(null!, ["data"]));
        Assert.That(ex!.Message, Does.Contain("Headers cannot be null or empty"));
        Assert.That(ex.ParamName, Is.EqualTo("headers"));
    }

    [Test]
    public void Constructor_EmptyHeaders_ThrowsArgumentException()
    {
        // Given: Empty headers array

        // When: Creating a DataTable with empty headers
        // Then: ArgumentException should be thrown
        var ex = Assert.Throws<ArgumentException>(() => new DataTable([], ["data"]));
        Assert.That(ex!.Message, Does.Contain("Headers cannot be null or empty"));
        Assert.That(ex.ParamName, Is.EqualTo("headers"));
    }

    [Test]
    public void Constructor_MismatchedColumnCount_ThrowsArgumentException()
    {
        // Given: Data row with wrong column count

        // When: Creating a DataTable with mismatched columns
        // Then: ArgumentException should be thrown
        var ex = Assert.Throws<ArgumentException>(() => new DataTable(
            ["Name", "Age"],
            ["Alice", "30", "ExtraColumn"]
        ));
        Assert.That(ex!.Message, Does.Contain("Data row has 3 columns but expected 2"));
        Assert.That(ex.ParamName, Is.EqualTo("dataRows"));
    }

    [Test]
    public void Constructor_NoDataRows_CreatesEmptyTable()
    {
        // Given: Headers but no data rows

        // When: Creating a DataTable with no data rows
        var table = new DataTable(["Name", "Age"]);

        // Then: Table should be created with zero rows
        Assert.That(table.RowCount, Is.EqualTo(0));
        Assert.That(table.ColumnCount, Is.EqualTo(2));
        Assert.That(table.Headers, Has.Count.EqualTo(2));
    }

    [Test]
    public void Headers_ReturnsReadOnlyHeaders()
    {
        // Given: A DataTable with headers
        var table = new DataTable(
            ["Name", "Age"],
            ["Alice", "30"]
        );

        // When: Accessing Headers property
        var headers = table.Headers;

        // Then: Should return correct headers
        Assert.That(headers, Is.EqualTo(new[] { "Name", "Age" }));
        Assert.That(headers.Count, Is.EqualTo(2));
    }

    [Test]
    public void Rows_ReturnsReadOnlyRows()
    {
        // Given: A DataTable with data
        var table = new DataTable(
            ["Name", "Age"],
            ["Alice", "30"],
            ["Bob", "25"]
        );

        // When: Accessing Rows property
        var rows = table.Rows;

        // Then: Should return correct number of rows
        Assert.That(rows, Has.Count.EqualTo(2));
        Assert.That(rows[0]["Name"], Is.EqualTo("Alice"));
        Assert.That(rows[1]["Name"], Is.EqualTo("Bob"));
    }

    [Test]
    public void RowCount_ReturnsCorrectCount()
    {
        // Given: A DataTable with data
        var table = new DataTable(
            ["Name"],
            ["Alice"],
            ["Bob"],
            ["Charlie"]
        );

        // When: Accessing RowCount
        var count = table.RowCount;

        // Then: Should return correct count
        Assert.That(count, Is.EqualTo(3));
    }

    [Test]
    public void ColumnCount_ReturnsCorrectCount()
    {
        // Given: A DataTable with multiple columns
        var table = new DataTable(
            ["Name", "Age", "City", "Country"],
            ["Alice", "30", "Seattle", "USA"]
        );

        // When: Accessing ColumnCount
        var count = table.ColumnCount;

        // Then: Should return correct count
        Assert.That(count, Is.EqualTo(4));
    }

    [Test]
    public void HasColumn_ExistingColumn_ReturnsTrue()
    {
        // Given: A DataTable with named columns
        var table = new DataTable(
            ["Name", "Age", "City"],
            ["Alice", "30", "Seattle"]
        );

        // When: Checking for existing columns
        // Then: Should return true
        Assert.That(table.HasColumn("Name"), Is.True);
        Assert.That(table.HasColumn("Age"), Is.True);
        Assert.That(table.HasColumn("City"), Is.True);
    }

    [Test]
    public void HasColumn_NonExistingColumn_ReturnsFalse()
    {
        // Given: A DataTable with named columns
        var table = new DataTable(
            ["Name", "Age"],
            ["Alice", "30"]
        );

        // When: Checking for non-existing column
        // Then: Should return false
        Assert.That(table.HasColumn("Email"), Is.False);
        Assert.That(table.HasColumn("City"), Is.False);
    }

    [Test]
    public void GetColumn_ExistingColumn_ReturnsAllValues()
    {
        // Given: A DataTable with data
        var table = new DataTable(
            ["Name", "Age", "City"],
            ["Alice", "30", "Seattle"],
            ["Bob", "25", "Portland"],
            ["Charlie", "35", "Denver"]
        );

        // When: Getting column values
        var names = table.GetColumn("Name");
        var ages = table.GetColumn("Age");

        // Then: Should return all values from column
        Assert.That(names, Is.EqualTo(new[] { "Alice", "Bob", "Charlie" }));
        Assert.That(ages, Is.EqualTo(new[] { "30", "25", "35" }));
    }

    [Test]
    public void GetColumn_NonExistingColumn_ThrowsArgumentException()
    {
        // Given: A DataTable with data
        var table = new DataTable(
            ["Name", "Age"],
            ["Alice", "30"]
        );

        // When: Getting non-existing column
        // Then: ArgumentException should be thrown
        var ex = Assert.Throws<ArgumentException>(() => table.GetColumn("Email"));
        Assert.That(ex!.Message, Does.Contain("Column 'Email' not found"));
        Assert.That(ex.Message, Does.Contain("Available columns: Name, Age"));
        Assert.That(ex.ParamName, Is.EqualTo("columnName"));
    }

    [Test]
    public void IndexerByInt_ValidIndex_ReturnsRow()
    {
        // Given: A DataTable with data
        var table = new DataTable(
            ["Name", "Age"],
            ["Alice", "30"],
            ["Bob", "25"],
            ["Charlie", "35"]
        );

        // When: Accessing rows by index
        var row0 = table[0];
        var row1 = table[1];
        var row2 = table[2];

        // Then: Should return correct rows
        Assert.That(row0["Name"], Is.EqualTo("Alice"));
        Assert.That(row1["Name"], Is.EqualTo("Bob"));
        Assert.That(row2["Name"], Is.EqualTo("Charlie"));
    }

    [Test]
    public void IndexerByInt_InvalidIndex_ThrowsIndexOutOfRangeException()
    {
        // Given: A DataTable with 2 rows
        var table = new DataTable(
            ["Name"],
            ["Alice"],
            ["Bob"]
        );

        // When: Accessing invalid index
        // Then: IndexOutOfRangeException should be thrown
        Assert.Throws<ArgumentOutOfRangeException>(() => _ = table[2]);
        Assert.Throws<ArgumentOutOfRangeException>(() => _ = table[-1]);
    }

    [Test]
    public void GetEnumerator_IteratesAllRows()
    {
        // Given: A DataTable with data
        var table = new DataTable(
            ["Name", "Age"],
            ["Alice", "30"],
            ["Bob", "25"]
        );

        // When: Iterating over rows
        var names = new List<string>();
        foreach (var row in table)
        {
            names.Add(row["Name"]);
        }

        // Then: Should iterate all rows
        Assert.That(names, Is.EqualTo(new[] { "Alice", "Bob" }));
    }

    [Test]
    public void ToString_FormatsTableCorrectly()
    {
        // Given: A DataTable with data
        var table = new DataTable(
            ["Name", "Age"],
            ["Alice", "30"],
            ["Bob", "25"]
        );

        // When: Converting to string
        var str = table.ToString();

        // Then: Should format table with headers and rows
        Assert.That(str, Does.Contain("DataTable (2 rows x 2 columns)"));
        Assert.That(str, Does.Contain("| Name | Age |"));
        Assert.That(str, Does.Contain("| Alice | 30 |"));
        Assert.That(str, Does.Contain("| Bob | 25 |"));
    }

    // DataTableRow Tests

    [Test]
    public void DataTableRow_Constructor_ValidData_CreatesRow()
    {
        // Given: Valid headers and values
        var headers = new[] { "Name", "Age", "City" };
        var values = new[] { "Alice", "30", "Seattle" };

        // When: Creating a DataTableRow (through DataTable)
        var table = new DataTable(headers, values);
        var row = table[0];

        // Then: Row should be created successfully
        Assert.That(row.Values, Has.Count.EqualTo(3));
    }

    [Test]
    public void DataTableRow_IndexerByString_ExistingColumn_ReturnsValue()
    {
        // Given: A DataTableRow with data
        var table = new DataTable(
            ["Name", "Age", "City"],
            ["Alice", "30", "Seattle"]
        );
        var row = table[0];

        // When: Accessing values by column name
        var name = row["Name"];
        var age = row["Age"];
        var city = row["City"];

        // Then: Should return correct values
        Assert.That(name, Is.EqualTo("Alice"));
        Assert.That(age, Is.EqualTo("30"));
        Assert.That(city, Is.EqualTo("Seattle"));
    }

    [Test]
    public void DataTableRow_IndexerByString_NonExistingColumn_ThrowsArgumentException()
    {
        // Given: A DataTableRow with data
        var table = new DataTable(
            ["Name", "Age"],
            ["Alice", "30"]
        );
        var row = table[0];

        // When: Accessing non-existing column
        // Then: ArgumentException should be thrown
        var ex = Assert.Throws<ArgumentException>(() => _ = row["Email"]);
        Assert.That(ex!.Message, Does.Contain("Column 'Email' not found"));
        Assert.That(ex.Message, Does.Contain("Available columns: Name, Age"));
        Assert.That(ex.ParamName, Is.EqualTo("columnName"));
    }

    [Test]
    public void DataTableRow_IndexerByInt_ValidIndex_ReturnsValue()
    {
        // Given: A DataTableRow with data
        var table = new DataTable(
            ["Name", "Age", "City"],
            ["Alice", "30", "Seattle"]
        );
        var row = table[0];

        // When: Accessing values by column index
        var value0 = row[0];
        var value1 = row[1];
        var value2 = row[2];

        // Then: Should return correct values
        Assert.That(value0, Is.EqualTo("Alice"));
        Assert.That(value1, Is.EqualTo("30"));
        Assert.That(value2, Is.EqualTo("Seattle"));
    }

    [Test]
    public void DataTableRow_IndexerByInt_InvalidIndex_ThrowsIndexOutOfRangeException()
    {
        // Given: A DataTableRow with 2 columns
        var table = new DataTable(
            ["Name", "Age"],
            ["Alice", "30"]
        );
        var row = table[0];

        // When: Accessing invalid index
        // Then: IndexOutOfRangeException should be thrown
        Assert.Throws<ArgumentOutOfRangeException>(() => _ = row[2]);
        Assert.Throws<ArgumentOutOfRangeException>(() => _ = row[-1]);
    }

    [Test]
    public void DataTableRow_HasColumn_ExistingColumn_ReturnsTrue()
    {
        // Given: A DataTableRow with columns
        var table = new DataTable(
            ["Name", "Age", "City"],
            ["Alice", "30", "Seattle"]
        );
        var row = table[0];

        // When: Checking for existing columns
        // Then: Should return true
        Assert.That(row.HasColumn("Name"), Is.True);
        Assert.That(row.HasColumn("Age"), Is.True);
        Assert.That(row.HasColumn("City"), Is.True);
    }

    [Test]
    public void DataTableRow_HasColumn_NonExistingColumn_ReturnsFalse()
    {
        // Given: A DataTableRow with columns
        var table = new DataTable(
            ["Name", "Age"],
            ["Alice", "30"]
        );
        var row = table[0];

        // When: Checking for non-existing column
        // Then: Should return false
        Assert.That(row.HasColumn("Email"), Is.False);
        Assert.That(row.HasColumn("City"), Is.False);
    }

    [Test]
    public void DataTableRow_TryGetValue_ExistingColumn_ReturnsTrue()
    {
        // Given: A DataTableRow with data
        var table = new DataTable(
            ["Name", "Age", "City"],
            ["Alice", "30", "Seattle"]
        );
        var row = table[0];

        // When: Trying to get existing column value
        var nameSuccess = row.TryGetValue("Name", out var name);
        var ageSuccess = row.TryGetValue("Age", out var age);

        // Then: Should return true and the value
        Assert.That(nameSuccess, Is.True);
        Assert.That(name, Is.EqualTo("Alice"));
        Assert.That(ageSuccess, Is.True);
        Assert.That(age, Is.EqualTo("30"));
    }

    [Test]
    public void DataTableRow_TryGetValue_NonExistingColumn_ReturnsFalse()
    {
        // Given: A DataTableRow with data
        var table = new DataTable(
            ["Name", "Age"],
            ["Alice", "30"]
        );
        var row = table[0];

        // When: Trying to get non-existing column value
        var success = row.TryGetValue("Email", out var value);

        // Then: Should return false and null value
        Assert.That(success, Is.False);
        Assert.That(value, Is.Null);
    }

    [Test]
    public void DataTableRow_Values_ReturnsAllValues()
    {
        // Given: A DataTableRow with data
        var table = new DataTable(
            ["Name", "Age", "City"],
            ["Alice", "30", "Seattle"]
        );
        var row = table[0];

        // When: Accessing Values property
        var values = row.Values;

        // Then: Should return all values in order
        Assert.That(values, Has.Count.EqualTo(3));
        Assert.That(values[0], Is.EqualTo("Alice"));
        Assert.That(values[1], Is.EqualTo("30"));
        Assert.That(values[2], Is.EqualTo("Seattle"));
    }

    [Test]
    public void DataTableRow_ToString_FormatsRowCorrectly()
    {
        // Given: A DataTableRow with data
        var table = new DataTable(
            ["Name", "Age", "City"],
            ["Alice", "30", "Seattle"]
        );
        var row = table[0];

        // When: Converting to string
        var str = row.ToString();

        // Then: Should format values separated by pipes
        Assert.That(str, Is.EqualTo("Alice | 30 | Seattle"));
    }

    [Test]
    public void DataTable_LinqOperations_WorkCorrectly()
    {
        // Given: A DataTable with multiple rows
        var table = new DataTable(
            ["Name", "Age", "City"],
            ["Alice", "30", "Seattle"],
            ["Bob", "25", "Portland"],
            ["Charlie", "35", "Seattle"]
        );

        // When: Using LINQ operations
        var seattleResidents = table.Where(row => row["City"] == "Seattle").ToList();
        var names = table.Select(row => row["Name"]).ToList();
        var firstRow = table.First();

        // Then: LINQ should work correctly
        Assert.That(seattleResidents, Has.Count.EqualTo(2));
        Assert.That(seattleResidents[0]["Name"], Is.EqualTo("Alice"));
        Assert.That(seattleResidents[1]["Name"], Is.EqualTo("Charlie"));
        Assert.That(names, Is.EqualTo(new[] { "Alice", "Bob", "Charlie" }));
        Assert.That(firstRow["Name"], Is.EqualTo("Alice"));
    }

    [Test]
    public void DataTable_SingleColumnTable_WorksCorrectly()
    {
        // Given: A single-column table
        var table = new DataTable(
            ["Username"],
            ["alice"],
            ["bob"],
            ["charlie"]
        );

        // When: Accessing data
        var usernames = table.GetColumn("Username");
        var firstUser = table[0]["Username"];

        // Then: Should work correctly
        Assert.That(table.ColumnCount, Is.EqualTo(1));
        Assert.That(table.RowCount, Is.EqualTo(3));
        Assert.That(usernames, Is.EqualTo(new[] { "alice", "bob", "charlie" }));
        Assert.That(firstUser, Is.EqualTo("alice"));
    }

    [Test]
    public void DataTable_EmptyStringValues_WorksCorrectly()
    {
        // Given: A table with empty string values
        var table = new DataTable(
            ["Name", "OptionalField"],
            ["Alice", ""],
            ["Bob", ""]
        );

        // When: Accessing empty values
        var optionalValues = table.GetColumn("OptionalField");

        // Then: Should handle empty strings correctly
        Assert.That(optionalValues, Has.All.EqualTo(""));
        Assert.That(table[0]["OptionalField"], Is.EqualTo(""));
    }
}
