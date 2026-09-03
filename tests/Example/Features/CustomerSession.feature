Feature: Customer Session
  As a customer
  I want to review my account summary
  So that I can confirm my account is ready

This scenario demonstrates the use of Provides and Requires attributes to indicate the state of the system before and after a step is executed.
See docs\wip\PRD-REQUIRED-STATE.md

Background:
  Given the application is running
  And I am logged in as customer

Scenario: Review account summary
  Given the customer profile is loaded
  When the customer requests the account summary
  Then the account summary should show "ready"
