Feature: nutritionBar

  Background:
    Given there is a user named 'bob'
    And 'bob' is logged into Onebite

  Scenario: 'bob' checks to see their progress
    Given 'bob' is on the create recipe page
    And 'bob' fills in the recipe name as 'TestRecipe'
    And 'bob' fills in the recipe directions as 'Test directions'
    And 'bob' fills in the recipe calories as '20'
    And 'bob' fills in the recipe protein as '30'
    And 'bob' fills in the recipe fat as '40'
    And 'bob' fills in the recipe carbs as '50'
    And 'bob' submits the recipe form
    And 'bob' is on the create meal page for nutrition
    And 'bob' fills in the meal title as 'testMeal'
    And 'bob' sets the meal date to today
    And 'bob' searches for recipe 'TestRecipe'
    And 'bob' clicks the first recipe result
    And 'bob' submits the meal form
    And 'bob' is on the page 'UserSettings/Nutrition'
    And 'bob' fills in the nutrition targets
    And 'bob' is on the page 'FoodEntries/Nutrition'
    Then Meal Bars callories are at 20/40
    Then Meal Bars protien are at 30/50
    Then Meal Bars fats are at 40/60
    Then Meal Bars carbs are at 50/70