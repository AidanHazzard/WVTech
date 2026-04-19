Feature: WVT-158

  Background:
    Given there is a user named 'Alice'
    And 'Alice' is logged into Onebite

  Scenario: Manually added item with same name as recipe ingredient is not accumulated when date range changes
    Given 'Alice' has a meal today with a WVT158 auto ingredient
    And 'Alice' views the shopping list so the auto ingredient is synced
    And 'Alice' manually adds the WVT158 ingredient to her shopping list
    When 'Alice' changes the shopping list to a date range that excludes today
    And 'Alice' changes the shopping list date range back to today
    Then the WVT158 ingredient amount has not been doubled on the shopping list

  Scenario: Manually added item is preserved when the date range excludes its matching recipe
    Given 'Alice' has a meal today with a WVT158 auto ingredient
    And 'Alice' views the shopping list so the auto ingredient is synced
    And 'Alice' manually adds the WVT158 ingredient to her shopping list
    When 'Alice' changes the shopping list to a date range that excludes today
    Then the manually added WVT158 ingredient is still on the shopping list
