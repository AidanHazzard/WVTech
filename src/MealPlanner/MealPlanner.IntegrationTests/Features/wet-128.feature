Feature: WVT-128

  Background:
    Given there is a user named 'Jack'
    And 'Jack' is logged into Onebite
    And 'Jack' has a meal with recipes created

  Scenario: 'Jack' deletes a recipe from the create meal page
    Given 'Jack' is on the create meal page
    And 'Jack' searches for a recipe "Oatmeal"
    And 'Jack' clicks the first search result
    When 'Jack' clicks the delete button on a recipe
    And 'Jack' confirms the deletion
    Then the recipe is removed from the meal immediately

  Scenario: 'Jack' can cancel deletion on create meal page
    Given 'Jack' is on the create meal page
    And 'Jack' searches for a recipe "Oatmeal"
    And 'Jack' clicks the first search result
    When 'Jack' clicks the delete button on a recipe
    And 'Jack' denies the deletion
    Then the recipe is still shown in the meal recipe list

  Scenario: 'Jack' deletes a recipe from the view meal page
    Given 'Jack' is on the view meal page
    When 'Jack' clicks the delete button on a recipe
    And 'Jack' confirms the deletion
    Then the recipe is removed from the meal immediately

  Scenario: 'Jack' can cancel deletion on view meal page
    Given 'Jack' is on the view meal page
    When 'Jack' clicks the delete button on a recipe
    And 'Jack' denies the deletion
    Then the recipe is still shown in the meal recipe list

  Scenario: 'Jack' deletes a recipe from the edit meal page
    Given 'Jack' is on the edit meal page
    When 'Jack' clicks the delete button on a recipe
    And 'Jack' confirms the deletion
    Then the recipe is removed from the meal immediately

  Scenario: 'Jack' can cancel deletion on edit meal page
    Given 'Jack' is on the edit meal page
    When 'Jack' clicks the delete button on a recipe
    And 'Jack' denies the deletion
    Then the recipe is still shown in the meal recipe list