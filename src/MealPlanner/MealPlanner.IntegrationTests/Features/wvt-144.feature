Feature: Recommend meals for entire day

# WVT-144

  Background:
    Given there is a user named 'Gary'
    And 'Gary' is logged into Onebite
    And 'Gary' has a calorie target set

  Scenario: User sees the option to generate a full day plan on the Create Meal page
    When 'Gary' navigates to the Create Meal page
    Then a 'Generate Day Plan' button is visible alongside the existing generate meal button

  Scenario: User is walked through day plan configuration
    Given 'Gary' is on the Create Meal page
    When 'Gary' clicks 'Generate Day Plan'
    Then 'Gary' is asked how many meals he would like for the day
    When 'Gary' enters 3 for the number of meals
    Then 'Gary' is asked whether he would like snacks included
    When 'Gary' chooses to include snacks
    Then 'Gary' is asked what size he would like his snacks to be
    And the snack size options include 'Small Snack' and 'Large Snack'

  Scenario: User configures size and food type for each meal in the day plan
    Given 'Gary' has specified 2 meals with no snacks for his day plan
    When 'Gary' is presented with the configuration for each meal
    Then 'Gary' is asked what size he would like each meal to be
    And the meal size options include 'Small', 'Average', and 'Large'
    And 'Gary' is asked what type of food he would like using tags

  Scenario: User is shown a summary after the day plan is generated
    Given 'Gary' has completed the day plan configuration
    When the day plan is generated
    Then 'Gary' sees a summary of his meal plan for the day
    And the summary shows each recommended meal by name

  Scenario: User regenerates a meal from the summary
    Given 'Gary' is viewing his generated day plan summary
    When 'Gary' chooses to regenerate one of the meals
    Then 'Gary' is shown the meal configuration form with his previous size and tag selections prefilled
    When 'Gary' confirms the configuration and regenerates
    Then the updated meal appears in the summary in place of the previous recommendation
    And all other meals in the summary remain unchanged
