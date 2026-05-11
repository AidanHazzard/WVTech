Feature: Filter recipes by dietary restrictions
# WVT-101

  Background:
    Given there is a user named 'Gary'
    And 'Gary' has a 'Nut Allergy' dietary restriction active
    And 'Gary' is logged into Onebite

  Scenario: Recipes with conflicting ingredients are hidden from search results
    Given there is a recipe named 'Peanut Butter Cookies' that contains a nut-based ingredient
    And there is a recipe named 'Oatmeal Cookies' that contains no nut-based ingredients
    When 'Gary' searches for 'Cookies'
    Then 'Oatmeal Cookies' appears in the search results
    And 'Peanut Butter Cookies' does not appear in the search results

  Scenario: Safe recipes display the matching dietary restriction tag
    Given there is a recipe named 'Fruit Salad' that contains no nut-based ingredients
    When 'Gary' searches for 'Fruit Salad'
    Then the 'Fruit Salad' recipe card displays a 'Nut Allergy' tag

  Scenario: A recipe must satisfy all active restrictions to appear
    Given 'Gary' also has a 'Gluten-Free' dietary restriction active
    And there is a recipe named 'Nut-Free Bread' that contains no nuts but contains gluten
    And there is a recipe named 'Rice Bowl' that contains no nuts and no gluten
    When 'Gary' searches for 'Bowl'
    Then 'Rice Bowl' appears in the search results
    And 'Nut-Free Bread' does not appear in the search results

  Scenario: All applicable tags are shown when a recipe satisfies multiple restrictions
    Given 'Gary' also has a 'Gluten-Free' dietary restriction active
    And there is a recipe named 'Rice Bowl' that contains no nuts and no gluten
    When 'Gary' searches for 'Rice Bowl'
    Then the 'Rice Bowl' recipe card displays a 'Nut Allergy' tag
    And the 'Rice Bowl' recipe card displays a 'Gluten-Free' tag

  Scenario: Empty state message appears when no recipes match active restrictions
    Given every available recipe contains a nut-based ingredient
    When 'Gary' searches for recipes
    Then a message is displayed explaining no results match the active filters
    And a link to the dietary restrictions settings page is visible
