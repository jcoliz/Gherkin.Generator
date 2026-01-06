@namespace:Gherkin.Generator.Tests.Example.Features
@baseclass:Gherkin.Generator.Tests.Example.FunctionalTestBase
Feature: Shopping Cart
  As a customer
  I want to add items to my cart
  So that I can purchase multiple items

Background:
  Given the application is running
  And I am logged in as customer

Rule: Adding Items

Scenario: Add single item to empty cart
  When I add "Widget" to the cart
  Then the cart should contain 1 item
  And the cart total should be 9.99

Scenario Outline: Add multiple items
  When I add <quantity> <item> to the cart
  Then the cart should contain <quantity> items

Examples:
  | quantity | item   |
  | 2        | Widget |
  | 5        | Gadget |

Rule: Removing Items

Scenario: Remove item from cart
  Given the cart contains:
    | Item   | Quantity |
    | Widget | 2        |
    | Gadget | 1        |
  When I remove "Widget" from the cart
  Then the cart should contain 1 item
  And the cart should not contain "Widget"

Scenario: Clear cart
  This shows an example of a scenario that is missing a step definition.

  Given the cart contains:
    | Item   | Quantity |
    | Widget | 2        |
    | Gadget | 1        |
  When I clear the cart
  Then the cart should be "empty"