Feature: View Switching
  As a customer
  I want to keep my search when changing views
  So that I can continue where I left off

Scenario Outline: Retains search when changing views
  Given an existing item
  And user is on the <View> view
  And searched for the item
  When switching to the <Target> view
  Then Results contains the item

Examples:
  | Target  | View   |
  | Prepare | Browse |