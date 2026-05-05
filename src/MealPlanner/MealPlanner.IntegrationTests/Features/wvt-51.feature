Feature: Long Term Nutrition report

# WVT-51

  Background:
    Given there is a user named 'Gary'
    And 'Gary' has completed meals with nutrition data in the past 30 days
    And 'Gary' is logged into Onebite

  Scenario: Nutrition report page shows weekly tab active by default
    When he navigates to the "FoodEntries/NutritionReport" page
    Then the "Weekly" tab is active
    And the "Monthly" tab is visible

  Scenario: User switches to monthly view
    Given he is on the "FoodEntries/NutritionReport" page
    When 'Gary' clicks the "Monthly" tab
    Then the "Monthly" tab is active
    And the "Weekly" tab is visible

  Scenario: User switches back to weekly view
    Given he is on the "FoodEntries/NutritionReport" page
    And 'Gary' clicks the "Monthly" tab
    When 'Gary' clicks the "Weekly" tab
    Then the "Weekly" tab is active
    And the "Monthly" tab is visible
