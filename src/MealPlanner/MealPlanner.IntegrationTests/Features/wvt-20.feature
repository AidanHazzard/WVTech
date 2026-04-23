Feature: WVT-20
  Background:
    Given there is a user named 'Alice'
    And 'Alice' is logged into Onebite

  Scenario: Shopping list includes ingredients from upcoming meals
    Given 'Alice' has an upcoming meal with ingredients
    When 'Alice' views her shopping list
    Then the shopping list contains the ingredients from her upcoming meal

  Scenario: Adding a meal adds its ingredients to the shopping list
    Given 'Alice' is on the create meal page
    When 'Alice' creates a meal with a recipe that has ingredients
    Then the ingredients from that recipe appear on her shopping list

  Scenario: Removing a meal removes its ingredients from the shopping list
    Given 'Alice' has an upcoming meal with ingredients
    When 'Alice' deletes that meal
    Then the ingredients from that meal are no longer on her shopping list

  Scenario: Duplicate ingredients across multiple meals are not repeated
    Given 'Alice' has two upcoming meals that share an ingredient
    When 'Alice' views her shopping list
    Then that shared ingredient appears only once on the shopping list

  Scenario: User can manually add items alongside auto-populated ingredients
    Given 'Alice' has an upcoming meal with ingredients
    And 'Alice' has manually added an item to her shopping list
    When 'Alice' views her shopping list
    Then both the auto-populated ingredients and the manually added item are present

  Scenario: Manually added items are not removed when a meal is deleted
    Given 'Alice' has an upcoming meal with ingredients
    And 'Alice' has manually added an item to her shopping list
    When 'Alice' deletes that meal
    Then the manually added item is still on her shopping list

  Scenario: Shopping list database is updated when a meal is added
    Given 'Alice' is on the create meal page
    When 'Alice' creates a meal with a recipe that has ingredients
    Then the shopping list items are saved to the database

  Scenario: Shopping list database is updated when a meal is removed
    Given 'Alice' has an upcoming meal with ingredients
    When 'Alice' deletes that meal
    Then the associated shopping list items are removed from the database

  Scenario: User can convert shopping list item measurements to a different unit
    Given 'Alice' has an upcoming meal with an ingredient measured in cups
    When 'Alice' views her shopping list
    And 'Alice' changes the display unit to 'Ounce(s)'
    Then the ingredient is displayed converted to ounces
