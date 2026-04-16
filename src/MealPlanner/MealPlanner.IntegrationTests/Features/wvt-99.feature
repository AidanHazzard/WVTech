Feature: Add tags to recipes

  Background:
    Given there is a user named 'Gary'
    And 'Gary' is logged into Onebite

  Scenario: Creator adds a predefined tag when creating a recipe
    Given 'Gary' is on the create recipe page
    And 'Gary' fills in the recipe name as 'TaggedRecipe'
    And 'Gary' fills in the recipe directions as 'Test directions'
    And 'Gary' fills in the recipe calories as '200'
    And 'Gary' fills in the recipe protein as '10'
    And 'Gary' fills in the recipe fat as '5'
    And 'Gary' fills in the recipe carbs as '30'
    And 'Gary' selects the predefined tag 'Breakfast'
    And 'Gary' submits the recipe form
    Then the recipe 'TaggedRecipe' has the tag 'Breakfast' in the database

  Scenario: Creator adds a custom tag when creating a recipe
    Given 'Gary' is on the create recipe page
    And 'Gary' fills in the recipe name as 'CustomTagRecipe'
    And 'Gary' fills in the recipe directions as 'Test directions'
    And 'Gary' fills in the recipe calories as '200'
    And 'Gary' fills in the recipe protein as '10'
    And 'Gary' fills in the recipe fat as '5'
    And 'Gary' fills in the recipe carbs as '30'
    And 'Gary' adds the custom tag 'Spicy'
    And 'Gary' submits the recipe form
    Then the recipe 'CustomTagRecipe' has the tag 'Spicy' in the database

  Scenario: Tags are displayed on the recipe detail page
    Given 'Gary' has a recipe named 'DisplayTagRecipe' with the tag 'Dinner'
    When 'Gary' views the recipe detail page for 'DisplayTagRecipe'
    Then the tag 'Dinner' is visible on the page

  Scenario: Creator can update tags when editing a recipe
    Given 'Gary' has a recipe named 'EditTagRecipe' with the tag 'Breakfast'
    And 'Gary' is on the edit recipe page for 'EditTagRecipe'
    When 'Gary' selects the predefined tag 'Dinner'
    And 'Gary' submits the edit recipe form
    Then the recipe 'EditTagRecipe' has the tag 'Dinner' in the database

  Scenario: Tags with different casing are treated as the same tag
    Given 'Gary' is on the create recipe page
    And 'Gary' fills in the recipe name as 'CaseRecipe1'
    And 'Gary' fills in the recipe directions as 'Test directions'
    And 'Gary' fills in the recipe calories as '200'
    And 'Gary' fills in the recipe protein as '10'
    And 'Gary' fills in the recipe fat as '5'
    And 'Gary' fills in the recipe carbs as '30'
    And 'Gary' adds the custom tag 'Spicy'
    And 'Gary' submits the recipe form
    And 'Gary' is on the create recipe page
    And 'Gary' fills in the recipe name as 'CaseRecipe2'
    And 'Gary' fills in the recipe directions as 'Test directions'
    And 'Gary' fills in the recipe calories as '200'
    And 'Gary' fills in the recipe protein as '10'
    And 'Gary' fills in the recipe fat as '5'
    And 'Gary' fills in the recipe carbs as '30'
    And 'Gary' adds the custom tag 'spicy'
    And 'Gary' submits the recipe form
    Then there is only 1 tag with the name 'Spicy' in the database

  Scenario: Tag dropdown shows only the top 10 most used tags
    Given there are 11 tags with varying usage counts
    And 'Gary' is on the create recipe page
    Then the tag dropdown shows exactly 10 options
    And the least used tag is not in the dropdown
