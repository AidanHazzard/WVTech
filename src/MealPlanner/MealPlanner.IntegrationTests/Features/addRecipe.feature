Feature: addRecipe

  Background:
    Given there is a user named 'bob'
    And 'bob' is logged into Onebite
    And 'bob' is on the create recipe page

  Scenario: 'bob' Adds A Succesful Recipe
    Given 'bob' fills in the recipe name as 'TestRecipe'
    And 'bob' fills in the recipe directions as 'Test directions'
    And 'bob' fills in the recipe calories as '20'
    And 'bob' fills in the recipe protein as '30'
    And 'bob' fills in the recipe fat as '40'
    And 'bob' fills in the recipe carbs as '50'
    