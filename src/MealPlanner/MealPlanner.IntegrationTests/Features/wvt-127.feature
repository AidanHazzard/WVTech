Feature: Edit meal details

As a user, I want to see all recipes associated with a meal and edit the meal title
so that I can update my meal details accurately.

Scenario: Edit meal page displays all associated recipes
Given I am on the edit meal page for a meal with one or more recipes
Then all associated recipes are displayed

Scenario: Meal title updates immediately when edited
Given I am on the edit meal page for a meal
When I update the meal title
Then the updated meal title is shown immediately

Scenario: Updated meal title is saved with the meal
Given I am on the edit meal page for a meal
When I update the meal title
And I save the meal
Then the meal is saved with the updated title