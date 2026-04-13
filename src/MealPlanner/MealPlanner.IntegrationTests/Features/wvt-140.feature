Feature: WVT-140

  Background:
    Given there is a user named 'Jack'
    And 'Jack' is logged into Onebite

  Scenario: 'Jack' cannot add a duplicate recipe on the create meal page
    Given 'Jack' is on the create meal page
    And 'Jack' searches for a recipe "Oatmeal"
    And 'Jack' clicks the first search result
    When 'Jack' searches for a recipe "Oatmeal"
    And 'Jack' clicks the first search result
    Then an error is shown saying the recipe is already in the meal

  Scenario: 'Jack' cannot add a duplicate recipe on the edit meal page
    Given 'Jack' has a meal with recipes created
    And 'Jack' is on the edit meal page
    When 'Jack' searches for a recipe "Oatmeal"
    And 'Jack' clicks the first search result
    Then an error is shown saying the recipe is already in the meal