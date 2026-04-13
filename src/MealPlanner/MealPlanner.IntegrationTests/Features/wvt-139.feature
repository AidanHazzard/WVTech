Feature: WVT-139

  Background:
    Given there is a user named 'Jack'
    And 'Jack' is logged into Onebite
    And 'Jack' has a recipe created

  Scenario: 'Jack' can delete a recipe from the recipe page
    Given 'Jack' is on the recipe page
    When 'Jack' clicks the delete button on their recipe
    And 'Jack' confirms the deletion
    Then the recipe is removed from the recipe list

  Scenario: 'Jack' can cancel deletion of a recipe
    Given 'Jack' is on the recipe page
    When 'Jack' clicks the delete button on their recipe
    And 'Jack' denies the deletion
    Then the recipe is still shown in the recipe list