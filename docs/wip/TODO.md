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
