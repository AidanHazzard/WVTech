Feature: Recipe Search Tag Filter

  Background:
    Given there is a user named 'Jack'
    And 'Jack' is logged into Onebite

  Scenario: Tag filter dropdown is visible on the recipe search page
    Given 'Jack' is on the create meal page
    Then the tag filter dropdown is visible

  Scenario: Filtering by tag combined with a name search narrows results
    Given 'Jack' has a recipe tagged 'TestBreakfast' named 'Oatmeal Bowl'
    And 'Jack' has a recipe tagged 'TestBreakfast' named 'Scrambled Eggs'
    And 'Jack' is on the create meal page
    When 'Jack' selects 'TestBreakfast' from the tag filter
    And 'Jack' searches for a recipe 'Oatmeal'
    Then 'Oatmeal Bowl' appears in the recipe search results
    And 'Scrambled Eggs' does not appear in the recipe search results
