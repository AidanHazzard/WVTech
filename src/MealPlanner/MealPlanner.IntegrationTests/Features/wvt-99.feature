Feature: Add tags to recipes

  Background:
    Given there is a user named 'alice'
    And 'alice' is logged into Onebite

  Scenario: Creator adds a predefined tag when creating a recipe
    Given 'alice' is on the create recipe page
    And 'alice' fills in the recipe name as 'TaggedRecipe'
    And 'alice' fills in the recipe directions as 'Test directions'
    And 'alice' fills in the recipe calories as '200'
    And 'alice' fills in the recipe protein as '10'
    And 'alice' fills in the recipe fat as '5'
    And 'alice' fills in the recipe carbs as '30'
    And 'alice' selects the predefined tag 'Breakfast'
    And 'alice' submits the recipe form
    Then the recipe 'TaggedRecipe' has the tag 'Breakfast' in the database

  Scenario: Creator adds a custom tag when creating a recipe
    Given 'alice' is on the create recipe page
    And 'alice' fills in the recipe name as 'CustomTagRecipe'
    And 'alice' fills in the recipe directions as 'Test directions'
    And 'alice' fills in the recipe calories as '200'
    And 'alice' fills in the recipe protein as '10'
    And 'alice' fills in the recipe fat as '5'
    And 'alice' fills in the recipe carbs as '30'
    And 'alice' adds the custom tag 'Spicy'
    And 'alice' submits the recipe form
    Then the recipe 'CustomTagRecipe' has the tag 'Spicy' in the database

  Scenario: Tags are displayed on the recipe detail page
    Given 'alice' has a recipe named 'DisplayTagRecipe' with the tag 'Dinner'
    When 'alice' views the recipe detail page for 'DisplayTagRecipe'
    Then the tag 'Dinner' is visible on the page

  Scenario: Creator can update tags when editing a recipe
    Given 'alice' has a recipe named 'EditTagRecipe' with the tag 'Breakfast'
    And 'alice' is on the edit recipe page for 'EditTagRecipe'
    When 'alice' selects the predefined tag 'Dinner'
    And 'alice' submits the edit recipe form
    Then the recipe 'EditTagRecipe' has the tag 'Dinner' in the database
