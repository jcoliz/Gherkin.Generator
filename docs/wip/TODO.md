- [ ] Maybe call "unimplemented" as "stub" instead?
- [ ] For tool, use example test as an integration test
- [ ] Unrecognized tags on scenarios should become category attributes. Note that multiple are allowed
- [X] @hidden on a scenario causes it to be not generated
- [X] Stub methods need to be valid C# symbols

```c#
    /// <summary>
    /// Given 12 transactions are selected
    /// </summary>
    [Given("12 transactions are selected")]
    public async Task 12TransactionsAreSelected()
```

Maybe this is fixed now? The following gherkin

```gherkin
Scenario: Generate recommendations with empty cart
  Given 12 items in the cart
  Then the cart should contain 12 items
```

...generates the following code, which does compile

```c#
    /// <summary>
    /// Given {value1} items in the cart
    /// </summary>
    [Given("{value1} items in the cart")]
    public async Task ItemsInTheCart(int value1)
    {
        throw new NotImplementedException();
    }
```

- [X] Bug: Stubs extracting int or string params need to account for them in the text and in the method name

This step:

```gherkin
  Then the cart should be "empty"
```

produced this erroneous stub implementation, when the step was (correctly) not found

```c#
/// <summary>
/// Then the cart should be "empty"
/// </summary>
[Then("the cart should be &quot;empty&quot;")]
public async Task TheCartShouldBeempty(string string1)
{
    throw new NotImplementedException();
}
```

should have been:

```c#
/// <summary>
/// Then the cart should be {string1}
/// </summary>
[Then("the cart should be {string1}")]
public async Task TheCartShouldBe(string string1)
{
    throw new NotImplementedException();
}
```

- [ ] For steps with no namespace (global), don't add a using
- [ ] For steps with no namespace (global), warn the user
- [ ] Can we pack in the default template and install it in user's Templates folder? Also when they update, it would bring the latest. They could use it, or they could copy to a new file and customize it.

- [ ] Bug: Don't parse numbers inside of strings

The following unimplemented step: "Then the Aisle suggestions include "Aisle 01"

Produced the following step call inside the test

        // Then the Aisle suggestions include "Aisle 01"
        await this.TheAisleSuggestionsInclude(01, "Aisle 01");

And produced this unimplemented step:

    /// <summary>
    /// Then the Aisle suggestions include {string1}
    /// </summary>
    [Then("the Aisle suggestions include {string1}")]
    public async Task TheAisleSuggestionsInclude(string string1, int value1)
    {
        throw new NotImplementedException();
    }

Expected:

        // Then the Aisle suggestions include "Aisle 01"
        await this.TheAisleSuggestionsInclude("Aisle 01");

And:

    public async Task TheAisleSuggestionsInclude(string string1)
    {
        throw new NotImplementedException();
    }

Note that the SUMMARY and the ATTRIBUTE usage are correct.