Feature: WVT-127

  Background:
     Given there is a user named 'Jack'
      And 'Jack' is logged into Onebite
      And 'Jack' has a meal created
      And the edit meal page is open

  Scenario: 'Jack' can edit a meal
    Given he is on the meal page
    When 'Jack' clicks the edit button
    Then the meal edit form is shown 

  Scenario: Meal title updates immediately when edited
    When User updates the meal title
    Then the updated meal title is shown immediately

  Scenario: Meal title is updated when the user clicks save
    When User updates the meal title
    And User saves the meal
    Then the meal is saved with the updated title

  Scenario: Recipe appears in meal recipe list after being added
    When User searches for a recipe "Oatmeal"
    And User clicks the first search result
    Then the recipe is shown in the meal recipe list

  Scenario: Recipe is saved with the meal after clicking save
    When User searches for a recipe "Oatmeal"
    And User clicks the first search result
    And User saves the meal
    Then the recipe is saved with the meal