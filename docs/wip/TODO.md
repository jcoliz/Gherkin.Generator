- [ ] Maybe call "unimplemented" as "stub" instead?
- [ ] Stubs don't include parameter from scenario outline
- [ ] For tool, use example test as an integration test
- [ ] DRY @namespace/@baseclass repeated in every feature file
- [ ] Consider out-of-the-box defaults for both
- [ ] When having stubs, ensure utils is in includes. Then uncomment step definition? (Not sure if that will cause issues)

What would out of the box defaults look like?

Namespace: ProjectRoot(.{Directory})* ?
Baseclass: Namespace\..\Infrastructure.TestBase ?

So for example class:

Namespace: Gherkin.Generator.Tests.Example.Features
Baseclass: Gherkin.Generator.Tests.Example.Infrastructure.TestBase

