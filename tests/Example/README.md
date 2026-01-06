# Gherkin.Generator Example Project

This project demonstrates how to use Gherkin.Generator to create behavior-driven tests with automatic code generation.

## What's Included

This example implements a simple shopping cart application with comprehensive Gherkin scenarios:

- **Feature File**: [`Features/ShoppingCart.feature`](Features/ShoppingCart.feature) - Complete Gherkin scenarios covering:
  - Background setup (application startup and login)
  - Adding single and multiple items to cart
  - Removing items from cart
  - Data table usage
  - Scenario outlines with examples
  
- **Step Definitions**:
  - [`Steps/ApplicationSteps.cs`](Steps/ApplicationSteps.cs) - Application setup and authentication steps
  - [`Steps/ShoppingCartSteps.cs`](Steps/ShoppingCartSteps.cs) - Shopping cart operation steps
  
- **Test Infrastructure**:
  - [`FunctionalTestBase.cs`](FunctionalTestBase.cs) - Test base class providing test context
  
- **System Under Test**:
  - [`ShoppingCart.cs`](ShoppingCart.cs) - Simple shopping cart implementation

## Running the Tests

```bash
# Build the project
dotnet build

# Run all tests
dotnet test
```

All 4 tests should pass:
- `AddSingleItemToEmptyCart`
- `AddMultipleItems` (2 test cases from scenario outline)
- `RemoveItemFromCart`

## Generated Code

The project is configured to retain generated files for easy inspection:

```xml
<EmitCompilerGeneratedFiles>true</EmitCompilerGeneratedFiles>
<CompilerGeneratedFilesOutputPath>obj\GeneratedFiles</CompilerGeneratedFilesOutputPath>
```

After building, view the generated test code at:
- `obj/GeneratedFiles/Gherkin.Generator/Gherkin.Generator.GherkinSourceGenerator/ShoppingCart.feature.g.cs`

This shows exactly how Gherkin scenarios are translated into C# test methods.

## Key Patterns Demonstrated

1. **Feature Tags** - Setting namespace, base class, and using directives
2. **Background Steps** - Common setup that runs before each scenario
3. **Rules** - Grouping related scenarios
4. **Scenario Outlines** - Data-driven tests with example tables
5. **Data Tables** - Passing structured data to steps
6. **Multiple Step Classes** - Organizing steps by domain (Application vs ShoppingCart)
7. **Step Context** - Sharing state through the test base class

## Known Limitations

This example includes workarounds for current generator limitations (documented with TODO comments):

1. **Decimal parameters** - Generator doesn't support decimal type, so we parse from string
2. **Scenario outline substitution** - Parameters aren't substituted correctly, requiring string parameters

Both issues are tracked for future generator improvements.

## Learn More

- See [`docs/USER-GUIDE.md`](../../docs/USER-GUIDE.md) for complete documentation
- Review the generated code to understand the code generation process
- Modify the feature file and rebuild to see how changes affect generated tests
