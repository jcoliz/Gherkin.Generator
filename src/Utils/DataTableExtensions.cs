using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;

namespace Gherkin.Generator.Utils;

/// <summary>
/// Extension methods for DataTable to support common usage patterns.
/// </summary>
public static class DataTableExtensions
{
    /// <summary>
    /// Gets a value from a key-value table where one column contains keys and another contains values.
    /// </summary>
    /// <param name="table">The data table.</param>
    /// <param name="key">The key to search for.</param>
    /// <param name="keyColumn">The name of the column containing keys (default: "Field").</param>
    /// <param name="valueColumn">The name of the column containing values (default: "Value").</param>
    /// <returns>The value associated with the specified key.</returns>
    /// <exception cref="InvalidOperationException">Thrown when no row with the specified key is found.</exception>
    /// <example>
    /// <code>
    /// // Given a table like:
    /// // | Field    | Value           |
    /// // | Email    | user@example.com |
    /// // | Password | SecurePass123!   |
    ///
    /// var email = table.GetKeyValue("Email");
    /// var password = table.GetKeyValue("Password");
    /// </code>
    /// </example>
    public static string GetKeyValue(this DataTable table, string key, string keyColumn = "Field", string valueColumn = "Value")
    {
        var row = table.SingleOrDefault(r => r[keyColumn] == key);
        if (row == null)
        {
            throw new InvalidOperationException(
                $"No row found with {keyColumn}='{key}'. Available keys: {string.Join(", ", table.Select(r => r[keyColumn]))}");
        }
        return row[valueColumn];
    }

    /// <summary>
    /// Tries to get a value from a key-value table.
    /// </summary>
    /// <param name="table">The data table.</param>
    /// <param name="key">The key to search for.</param>
    /// <param name="value">The value if found; otherwise null.</param>
    /// <param name="keyColumn">The name of the column containing keys (default: "Field").</param>
    /// <param name="valueColumn">The name of the column containing values (default: "Value").</param>
    /// <returns>True if the key was found; otherwise, false.</returns>
    public static bool TryGetKeyValue(this DataTable table, string key, out string? value, string keyColumn = "Field", string valueColumn = "Value")
    {
        var row = table.SingleOrDefault(r => r[keyColumn] == key);
        if (row != null)
        {
            value = row[valueColumn];
            return true;
        }
        value = null;
        return false;
    }

    /// <summary>
    /// Gets all values from the first column (useful for single-column tables).
    /// </summary>
    /// <param name="table">The data table.</param>
    /// <returns>A collection of all values from the first column.</returns>
    /// <example>
    /// <code>
    /// // Given a table like:
    /// // | Username |
    /// // | alice    |
    /// // | bob      |
    ///
    /// var usernames = table.GetFirstColumn(); // ["alice", "bob"]
    /// </code>
    /// </example>
    public static IReadOnlyCollection<string> GetFirstColumn(this DataTable table)
    {
        if (table.ColumnCount == 0)
            throw new InvalidOperationException("Table has no columns");

        return table.Select(row => row[0]).ToList().AsReadOnly();
    }

    /// <summary>
    /// Converts a single-column table to a read-only collection of values.
    /// </summary>
    /// <param name="table">The data table.</param>
    /// <returns>A read-only collection of all values from the single column.</returns>
    /// <exception cref="InvalidOperationException">Thrown when table does not have exactly one column.</exception>
    /// <example>
    /// <code>
    /// // Given a table like:
    /// // | Username |
    /// // | alice    |
    /// // | bob      |
    ///
    /// var usernames = table.ToSingleColumnList(); // ["alice", "bob"]
    /// </code>
    /// </example>
    public static IReadOnlyCollection<string> ToSingleColumnList(this DataTable table)
    {
        if (table.ColumnCount != 1)
            throw new InvalidOperationException($"Table must have exactly one column, but has {table.ColumnCount}");

        return table.Select(row => row[0]).ToList().AsReadOnly();
    }

    /// <summary>
    /// Ensures that all required columns are present in the table.
    /// </summary>
    public static void RequireColumns(this DataTable table, params string[] requiredColumns)
    {
        var missing = requiredColumns.Where(c => !table.HasColumn(c)).ToArray();
        if (missing.Length == 0)
            return;

        throw new ArgumentException(
            $"Missing required column(s): {string.Join(", ", missing)}. Available columns: {string.Join(", ", table.Headers)}");
    }

    /// <summary>
    /// Gets a required string value from a row.
    /// </summary>
    public static string GetRequired(this DataTableRow row, string columnName)
    {
        if (!row.TryGetValue(columnName, out var value) || string.IsNullOrWhiteSpace(value))
            throw new ArgumentException($"Column '{columnName}' is required and must have a value.");

        // Null-forgiving operator is used because we have already checked for null or whitespace.
        return value!;
    }

    /// <summary>
    /// Gets an optional string value from a row.
    /// </summary>
    public static string? GetOptional(this DataTableRow row, string columnName)
    {
        if (!row.TryGetValue(columnName, out var value) || string.IsNullOrWhiteSpace(value))
            return null;

        return value;
    }

    /// <summary>
    /// Gets a required value from a row, converted to <typeparamref name="T"/>.
    /// </summary>
    /// <remarks>
    /// Conversion is culture-invariant and supports anything with a <see cref="TypeConverter"/>:
    /// numbers, bool, enums, Guid, DateTime, TimeSpan, etc.
    /// </remarks>
    /// <exception cref="ArgumentException">Column is missing, blank, or cannot be converted.</exception>
    public static T GetRequired<T>(this DataTableRow row, string columnName)
    {
        var value = row.GetRequired(columnName);
        return Convert<T>(value, columnName);
    }

    /// <summary>
    /// Gets an optional value from a row, converted to <typeparamref name="T"/>.
    /// </summary>
    /// <remarks>
    /// Returns null when the column is missing or blank. Use <c>GetOptional(columnName)</c>
    /// (non-generic) for optional strings.
    /// <para>
    /// This overload is intentionally constrained to value types. The generic conversion path is for
    /// scalar values such as <see cref="int"/>, <see cref="bool"/>, <see cref="Guid"/>,
    /// <see cref="DateTime"/>, and enums. String values are handled by the non-generic
    /// <c>GetOptional(string)</c> overload instead.
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentException">Value is present but cannot be converted.</exception>
    public static T? GetOptional<T>(this DataTableRow row, string columnName) where T : struct
    {
        var value = row.GetOptional(columnName);
        return value is null ? null : Convert<T>(value, columnName);
    }

    private static T Convert<T>(string value, string columnName)
    {
        // Most TypeConverters reject surrounding whitespace, so we normalize before conversion.
        // String values are handled by the non-generic overloads; this path is intended for scalar types.
        try
        {
            var converter = TypeDescriptor.GetConverter(typeof(T));
            return (T)converter.ConvertFromString(null, CultureInfo.InvariantCulture, value.Trim())!;
        }
        catch (Exception ex) when (ex is NotSupportedException or FormatException or ArgumentException or OverflowException)
        {
            throw new ArgumentException(
                $"Column '{columnName}' value '{value}' cannot be converted to {typeof(T).Name}.", ex);
        }
    }    
}
