Feature: editRecipe

  Background:
    Given there is a user named 'bob'
    And 'bob' is logged into Onebite
    And 'bob' has a recipe to edit
    And 'bob' is on the edit recipe page

  Scenario: 'bob' sees the recipe fields are prefilled
    Then the recipe name field contains 'EditTestRecipe'
    And the recipe directions field contains 'Edit test directions'
    And the recipe calories field contains '20'
    And the recipe protein field contains '30'
    And the recipe fat field contains '40'
    And the recipe carbs field contains '50'

  Scenario: 'bob' edits a recipe successfully
    And 'bob' fills in the recipe name as 'UpdatedRecipe'
    And 'bob' fills in the recipe directions as 'Updated directions'
    And 'bob' submits the recipe form
    Then 'bob' is redirected away from the edit recipe page

  Scenario: 'bob' tries to edit a recipe with no name
    And 'bob' clears the recipe name
    And 'bob' submits the recipe form
    Then 'bob' remains on the edit recipe page

  Scenario: 'bob' tries to edit a recipe with no directions
    And 'bob' clears the recipe directions
    And 'bob' submits the recipe form
    Then 'bob' remains on the edit recipe page

  Scenario: 'bob' tries to edit a recipe with no calories
    And 'bob' clears the recipe calories
    And 'bob' submits the recipe form
    Then 'bob' remains on the edit recipe page

  Scenario: 'bob' tries to edit a recipe with no protein
    And 'bob' clears the recipe protein
    And 'bob' submits the recipe form
    Then 'bob' remains on the edit recipe page

  Scenario: 'bob' tries to edit a recipe with no fat
    And 'bob' clears the recipe fat
    And 'bob' submits the recipe form
    Then 'bob' remains on the edit recipe page

  Scenario: 'bob' tries to edit a recipe with no carbs
    And 'bob' clears the recipe carbs
    And 'bob' submits the recipe form
    Then 'bob' remains on the edit recipe page

  Scenario: 'bob' tries to edit a recipe with a blank ingredient
    And 'bob' adds a blank ingredient
    And 'bob' submits the recipe form
    Then 'bob' remains on the edit recipe page