Feature: WVT-143

  Background:
    Given there is a user named 'Jack'
    And 'Jack' is logged into Onebite
    And 'Jack' has a recipe created

  Scenario: 'Jack' can click on their recipe to view it
    Given 'Jack' is on the recipe page
    When 'Jack' clicks on their recipe
    Then 'Jack' is taken to the recipe detail page