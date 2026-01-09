- [ ] Maybe call "unimplemented" as "stub" instead?
- [ ] For tool, use example test as an integration test
- [ ] Pack default template in with nuget package. Auto-install it to users workspace
- [ ] Unrecognized tags on scenarios should become category attributes. Note that multiple are allowed
- [ ] Stub methods need to be valid C# symbols

```c#
    /// <summary>
    /// Given 12 transactions are selected
    /// </summary>
    [Given("12 transactions are selected")]
    public async Task 12TransactionsAreSelected()
```
