using Gherkin.Generator.Lib;

namespace Gherkin.Generator.Tests.Unit;

/// <summary>
/// Tests for DataTable handling in Gherkin-to-CRIF conversion.
/// </summary>
[TestFixture]
public class DataTableTests : StepMatchingTestsBase
{
    [Test]
    public void Convert_WithDataTableStep_ExtractsDataTableAndMatchesStep()
    {
        // Given: A step with DataTable parameter
        var stepMetadata = new StepMetadataCollection();
        stepMetadata.Add(new StepMetadata
        {
            NormalizedKeyword = NormalizedKeyword.Given,
            Text = "I have the following transactions",
            Method = "IHaveTheFollowingTransactions",
            Class = "TransactionSteps",
            Namespace = "YoFi.V3.Tests.Functional.Steps",
            Parameters = [new StepParameter { Type = "DataTable", Name = "table" }]
        });

        var converter = new GherkinToCrifConverter(stepMetadata);

        // And: A Gherkin feature with that step and DataTable
        var gherkin = """
            Feature: Transactions

            Scenario: Multiple transactions
              Given I have the following transactions
                | Date       | Payee      | Amount |
                | 2024-01-01 | Store A    | 100.00 |
                | 2024-01-02 | Store B    | 200.00 |
            """;
        var feature = ParseGherkin(gherkin);

        // When: Feature is converted to CRIF
        var crif = converter.Convert(feature);

        // Then: Step should be matched with correct Owner and Method
        var step = crif.Rules[0].Scenarios[0].Steps[0];
        Assert.That(step.Owner, Is.EqualTo("TransactionSteps"));
        Assert.That(step.Method, Is.EqualTo("IHaveTheFollowingTransactions"));

        // And: DataTable should be extracted
        Assert.That(step.DataTable, Is.Not.Null);
        Assert.That(step.DataTable!.Headers, Has.Count.EqualTo(3));
        Assert.That(step.DataTable.Rows, Has.Count.EqualTo(2));

        // And: Step should have DataTable variable as argument
        Assert.That(step.Arguments, Has.Count.EqualTo(1));
        Assert.That(step.Arguments[0].Value, Is.EqualTo("table1"));
    }

    [Test]
    public void Convert_WithDataTableStep_AddsClassAndNamespace()
    {
        // Given: A step with DataTable parameter
        var stepMetadata = new StepMetadataCollection();
        stepMetadata.Add(new StepMetadata
        {
            NormalizedKeyword = NormalizedKeyword.When,
            Text = "I create transactions",
            Method = "ICreateTransactions",
            Class = "TransactionSteps",
            Namespace = "YoFi.V3.Tests.Functional.Steps",
            Parameters = [new StepParameter { Type = "DataTable", Name = "transactions" }]
        });

        var converter = new GherkinToCrifConverter(stepMetadata);

        // And: A Gherkin feature with DataTable
        var gherkin = """
            Feature: Transactions

            Scenario: Create multiple
              When I create transactions
                | Payee   | Amount |
                | Store A | 100.00 |
            """;
        var feature = ParseGherkin(gherkin);

        // When: Feature is converted to CRIF
        var crif = converter.Convert(feature);

        // Then: Class should be added to Classes list
        Assert.That(crif.Classes, Contains.Item("TransactionSteps"));

        // And: Namespace should be added to Usings
        Assert.That(crif.Usings, Contains.Item("YoFi.V3.Tests.Functional.Steps"));
    }

    [Test]
    public void Convert_WithMixedParametersAndDataTable_ExtractsBothCorrectly()
    {
        // Given: A step with both regular parameter and DataTable
        var stepMetadata = new StepMetadataCollection();
        stepMetadata.Add(new StepMetadata
        {
            NormalizedKeyword = NormalizedKeyword.Given,
            Text = "I have {count} transactions with data",
            Method = "IHaveTransactionsWithData",
            Class = "TransactionSteps",
            Namespace = "YoFi.V3.Tests.Functional.Steps",
            Parameters = [
                new StepParameter { Type = "int", Name = "count" },
                new StepParameter { Type = "DataTable", Name = "table" }
            ]
        });

        var converter = new GherkinToCrifConverter(stepMetadata);

        // And: A Gherkin feature with parameter and DataTable
        var gherkin = """
            Feature: Transactions

            Scenario: Multiple transactions
              Given I have 5 transactions with data
                | Field  | Value |
                | Type   | Credit |
            """;
        var feature = ParseGherkin(gherkin);

        // When: Feature is converted to CRIF
        var crif = converter.Convert(feature);

        // Then: Step should be matched
        var step = crif.Rules[0].Scenarios[0].Steps[0];
        Assert.That(step.Owner, Is.EqualTo("TransactionSteps"));
        Assert.That(step.Method, Is.EqualTo("IHaveTransactionsWithData"));

        // And: Both regular argument and DataTable variable should be present
        Assert.That(step.Arguments, Has.Count.EqualTo(2));
        Assert.That(step.Arguments[0].Value, Is.EqualTo("5"));
        Assert.That(step.Arguments[0].Last, Is.False);
        Assert.That(step.Arguments[1].Value, Is.EqualTo("table1"));
        Assert.That(step.Arguments[1].Last, Is.True);

        // And: DataTable should be extracted
        Assert.That(step.DataTable, Is.Not.Null);
        Assert.That(step.DataTable!.Headers, Has.Count.EqualTo(2));
    }

    [Test]
    public void Convert_WithMultipleDataTableSteps_GeneratesUniqueVariableNames()
    {
        // Given: Multiple steps with DataTable parameters
        var stepMetadata = new StepMetadataCollection();
        stepMetadata.AddRange([
            new StepMetadata
            {
                NormalizedKeyword = NormalizedKeyword.Given,
                Text = "I have transactions",
                Method = "IHaveTransactions",
                Class = "TransactionSteps",
                Namespace = "YoFi.V3.Tests.Functional.Steps",
                Parameters = [new StepParameter { Type = "DataTable", Name = "transactions" }]
            },
            new StepMetadata
            {
                NormalizedKeyword = NormalizedKeyword.Given,
                Text = "I have payees",
                Method = "IHavePayees",
                Class = "PayeeSteps",
                Namespace = "YoFi.V3.Tests.Functional.Steps",
                Parameters = [new StepParameter { Type = "DataTable", Name = "payees" }]
            }
        ]);

        var converter = new GherkinToCrifConverter(stepMetadata);

        // And: A Gherkin feature with multiple DataTable steps
        var gherkin = """
            Feature: Data Setup

            Scenario: Setup test data
              Given I have transactions
                | Date       | Amount |
                | 2024-01-01 | 100.00 |
              And I have payees
                | Name    | Category |
                | Store A | Shopping |
            """;
        var feature = ParseGherkin(gherkin);

        // When: Feature is converted to CRIF
        var crif = converter.Convert(feature);

        // Then: Each DataTable should have a unique variable name
        var step1 = crif.Rules[0].Scenarios[0].Steps[0];
        var step2 = crif.Rules[0].Scenarios[0].Steps[1];
        Assert.That(step1.DataTable!.VariableName, Is.EqualTo("table1"));
        Assert.That(step2.DataTable!.VariableName, Is.EqualTo("table2"));

        // And: Arguments should reference the correct table variables
        Assert.That(step1.Arguments[0].Value, Is.EqualTo("table1"));
        Assert.That(step2.Arguments[0].Value, Is.EqualTo("table2"));
    }

    [Test]
    public void Convert_WithUnmatchedDataTableStep_StaysInUnimplementedList()
    {
        // Given: An empty step metadata collection
        var stepMetadata = new StepMetadataCollection();
        var converter = new GherkinToCrifConverter(stepMetadata);

        // And: A Gherkin feature with unmatched DataTable step
        var gherkin = """
            Feature: Transactions

            Scenario: Test
              Given I have the following data
                | Field | Value |
                | Name  | Test  |
            """;
        var feature = ParseGherkin(gherkin);

        // When: Feature is converted to CRIF
        var crif = converter.Convert(feature);

        // Then: Step should be in Unimplemented list
        Assert.That(crif.Unimplemented, Has.Count.EqualTo(1));
        Assert.That(crif.Unimplemented[0].Text, Is.EqualTo("I have the following data"));

        // And: Step should still have DataTable extracted
        var step = crif.Rules[0].Scenarios[0].Steps[0];
        Assert.That(step.DataTable, Is.Not.Null);

        // And: Step should have Owner="this" for unimplemented step
        Assert.That(step.Owner, Is.EqualTo("this"));
    }

    [Test]
    public void Convert_WithUnmatchedDataTableStep_AddsDataTableVariableAsArgument()
    {
        // Given: An empty step metadata collection (no step matching)
        var stepMetadata = new StepMetadataCollection();
        var converter = new GherkinToCrifConverter(stepMetadata);

        // And: A Gherkin feature with unmatched DataTable step
        var gherkin = """
            Feature: Bank Import

            Scenario: Import transactions with external IDs
              Given I have some other transactions with external IDs:
                | ExternalId | Date       | Payee       | Amount  |
                | 2024010701 | 2024-01-07 | Gas Station | -89.99  |
            """;
        var feature = ParseGherkin(gherkin);

        // When: Feature is converted to CRIF
        var crif = converter.Convert(feature);

        // Then: Step should be in Unimplemented list
        Assert.That(crif.Unimplemented, Has.Count.EqualTo(1));
        Assert.That(crif.Unimplemented[0].Text, Is.EqualTo("I have some other transactions with external IDs:"));

        // And: Unimplemented step should have DataTable parameter in signature
        var unimplementedStep = crif.Unimplemented[0];
        Assert.That(unimplementedStep.Parameters, Has.Count.EqualTo(1));
        Assert.That(unimplementedStep.Parameters[0].Type, Is.EqualTo("DataTable"));
        Assert.That(unimplementedStep.Parameters[0].Name, Is.EqualTo("table"));

        // And: Step should have DataTable extracted
        var step = crif.Rules[0].Scenarios[0].Steps[0];
        Assert.That(step.DataTable, Is.Not.Null);
        Assert.That(step.DataTable!.VariableName, Is.EqualTo("table1"));

        // And: Step should have DataTable variable as argument
        Assert.That(step.Arguments, Has.Count.EqualTo(1));
        Assert.That(step.Arguments[0].Value, Is.EqualTo("table1"));
        Assert.That(step.Arguments[0].Last, Is.True);

        // And: Step should have Owner="this" for unimplemented step
        Assert.That(step.Owner, Is.EqualTo("this"));
    }

    [Test]
    public void Convert_WithDataTableInBackground_ExtractsDataTableAndMatches()
    {
        // Given: A step with DataTable parameter
        var stepMetadata = new StepMetadataCollection();
        stepMetadata.Add(new StepMetadata
        {
            NormalizedKeyword = NormalizedKeyword.Given,
            Text = "I have test data",
            Method = "IHaveTestData",
            Class = "DataSteps",
            Namespace = "YoFi.V3.Tests.Functional.Steps",
            Parameters = [new StepParameter { Type = "DataTable", Name = "data" }]
        });

        var converter = new GherkinToCrifConverter(stepMetadata);

        // And: A Gherkin feature with DataTable in Background
        var gherkin = """
            Feature: Data Tests

            Background:
              Given I have test data
                | Key   | Value |
                | Test1 | Val1  |

            Scenario: Test scenario
              When I run tests
            """;
        var feature = ParseGherkin(gherkin);

        // When: Feature is converted to CRIF
        var crif = converter.Convert(feature);

        // Then: Background step should be matched
        var bgStep = crif.Background!.Steps[0];
        Assert.That(bgStep.Owner, Is.EqualTo("DataSteps"));
        Assert.That(bgStep.Method, Is.EqualTo("IHaveTestData"));

        // And: DataTable should be extracted in background
        Assert.That(bgStep.DataTable, Is.Not.Null);
        Assert.That(bgStep.DataTable!.Headers, Has.Count.EqualTo(2));

        // And: Class should be added from background step
        Assert.That(crif.Classes, Contains.Item("DataSteps"));
    }

    [Test]
    public void Convert_WithDataTableInBackground_AddsClassAndNamespace()
    {
        // Given: A step with DataTable parameter
        var stepMetadata = new StepMetadataCollection();
        stepMetadata.Add(new StepMetadata
        {
            NormalizedKeyword = NormalizedKeyword.Given,
            Text = "I have the following users",
            Method = "IHaveTheFollowingUsers",
            Class = "UserSteps",
            Namespace = "YoFi.V3.Tests.Functional.Steps",
            Parameters = [new StepParameter { Type = "DataTable", Name = "users" }]
        });

        var converter = new GherkinToCrifConverter(stepMetadata);

        // And: A Gherkin feature with DataTable in Background
        var gherkin = """
            Feature: User Management

            Background:
              Given I have the following users
                | Username | Email              | Role  |
                | alice    | alice@example.com  | Admin |
                | bob      | bob@example.com    | User  |

            Scenario: User operations
              When I perform user operations
            """;
        var feature = ParseGherkin(gherkin);

        // When: Feature is converted to CRIF
        var crif = converter.Convert(feature);

        // Then: Background step should be matched with correct Owner and Method
        var bgStep = crif.Background!.Steps[0];
        Assert.That(bgStep.Owner, Is.EqualTo("UserSteps"));
        Assert.That(bgStep.Method, Is.EqualTo("IHaveTheFollowingUsers"));

        // And: DataTable should be extracted with correct structure
        Assert.That(bgStep.DataTable, Is.Not.Null);
        Assert.That(bgStep.DataTable!.Headers, Has.Count.EqualTo(3));
        Assert.That(bgStep.DataTable.Headers[0].Value, Is.EqualTo("Username"));
        Assert.That(bgStep.DataTable.Headers[1].Value, Is.EqualTo("Email"));
        Assert.That(bgStep.DataTable.Headers[2].Value, Is.EqualTo("Role"));
        Assert.That(bgStep.DataTable.Rows, Has.Count.EqualTo(2));

        // And: Step should have DataTable variable as argument
        Assert.That(bgStep.Arguments, Has.Count.EqualTo(1));
        Assert.That(bgStep.Arguments[0].Value, Is.EqualTo("table1"));

        // And: Class should be added to Classes list
        Assert.That(crif.Classes, Contains.Item("UserSteps"));

        // And: Namespace should be added to Usings
        Assert.That(crif.Usings, Contains.Item("YoFi.V3.Tests.Functional.Steps"));
    }

    [Test]
    public void Convert_WithDataTable_AddsUtilsNamespaceToUsings()
    {
        // Given: A step with DataTable parameter
        var stepMetadata = new StepMetadataCollection();
        stepMetadata.Add(new StepMetadata
        {
            NormalizedKeyword = NormalizedKeyword.Given,
            Text = "I have the following data",
            Method = "IHaveTheFollowingData",
            Class = "DataSteps",
            Namespace = "MyApp.Tests.Functional.Steps",
            Parameters = [new StepParameter { Type = "DataTable", Name = "table" }]
        });

        var converter = new GherkinToCrifConverter(stepMetadata);

        // And: A Gherkin feature with a DataTable
        var gherkin = """
            Feature: Data Management

            Scenario: Process data
              Given I have the following data
                | Field1 | Field2 |
                | Value1 | Value2 |
            """;
        var feature = ParseGherkin(gherkin);

        // When: Feature is converted to CRIF
        var crif = converter.Convert(feature);

        // Then: Usings should contain Gherkin.Generator.Utils
        Assert.That(crif.Usings, Contains.Item("Gherkin.Generator.Utils"));

        // And: Step-specific namespace should also be present
        Assert.That(crif.Usings, Contains.Item("MyApp.Tests.Functional.Steps"));
    }
}
