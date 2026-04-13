Feature: addRecipe

  Background:
    Given there is a user named 'bob'
    And 'bob' is logged into Onebite
    And 'bob' is on the create recipe page

  Scenario: 'bob' adds a successful recipe
    And 'bob' fills in the recipe name as 'TestRecipe'
    And 'bob' fills in the recipe directions as 'Test directions'
    And 'bob' fills in the recipe calories as '20'
    And 'bob' fills in the recipe protein as '30'
    And 'bob' fills in the recipe fat as '40'
    And 'bob' fills in the recipe carbs as '50'
    And 'bob' submits the recipe form
    Then 'bob' is redirected away from the create recipe page

  Scenario: 'bob' tries to add a recipe with no name
    And 'bob' fills in the recipe directions as 'Test directions'
    And 'bob' fills in the recipe calories as '20'
    And 'bob' fills in the recipe protein as '30'
    And 'bob' fills in the recipe fat as '40'
    And 'bob' fills in the recipe carbs as '50'
    And 'bob' submits the recipe form
    Then 'bob' remains on the create recipe page

  Scenario: 'bob' tries to add a recipe with no directions
    And 'bob' fills in the recipe name as 'TestRecipe'
    And 'bob' fills in the recipe calories as '20'
    And 'bob' fills in the recipe protein as '30'
    And 'bob' fills in the recipe fat as '40'
    And 'bob' fills in the recipe carbs as '50'
    And 'bob' submits the recipe form
    Then 'bob' remains on the create recipe page

  Scenario: 'bob' tries to add a recipe with no calories
    And 'bob' fills in the recipe name as 'TestRecipe'
    And 'bob' fills in the recipe directions as 'Test directions'
    And 'bob' fills in the recipe protein as '30'
    And 'bob' fills in the recipe fat as '40'
    And 'bob' fills in the recipe carbs as '50'
    And 'bob' submits the recipe form
    Then 'bob' remains on the create recipe page

  Scenario: 'bob' tries to add a recipe with no protein
    And 'bob' fills in the recipe name as 'TestRecipe'
    And 'bob' fills in the recipe directions as 'Test directions'
    And 'bob' fills in the recipe calories as '20'
    And 'bob' fills in the recipe fat as '40'
    And 'bob' fills in the recipe carbs as '50'
    And 'bob' submits the recipe form
    Then 'bob' remains on the create recipe page

  Scenario: 'bob' tries to add a recipe with no fat
    And 'bob' fills in the recipe name as 'TestRecipe'
    And 'bob' fills in the recipe directions as 'Test directions'
    And 'bob' fills in the recipe calories as '20'
    And 'bob' fills in the recipe protein as '30'
    And 'bob' fills in the recipe carbs as '50'
    And 'bob' submits the recipe form
    Then 'bob' remains on the create recipe page

  Scenario: 'bob' tries to add a recipe with no carbs
    And 'bob' fills in the recipe name as 'TestRecipe'
    And 'bob' fills in the recipe directions as 'Test directions'
    And 'bob' fills in the recipe calories as '20'
    And 'bob' fills in the recipe protein as '30'
    And 'bob' fills in the recipe fat as '40'
    And 'bob' submits the recipe form
    Then 'bob' remains on the create recipe page

  Scenario: 'bob' tries to add a recipe with a blank ingredient
    And 'bob' fills in the recipe name as 'TestRecipe'
    And 'bob' fills in the recipe directions as 'Test directions'
    And 'bob' fills in the recipe calories as '20'
    And 'bob' fills in the recipe protein as '30'
    And 'bob' fills in the recipe fat as '40'
    And 'bob' fills in the recipe carbs as '50'
    And 'bob' adds a blank ingredient
    And 'bob' submits the recipe form
    Then 'bob' remains on the create recipe page