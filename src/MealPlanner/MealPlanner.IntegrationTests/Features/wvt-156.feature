Feature: WVT-156

  Background:
    Given there is a user named 'Jack'
    And 'Jack' is logged into Onebite

  Scenario: Clicking Add A Meal from a future date pre-fills that date on the create meal form
    Given 'Jack' is on the home page for a future date
    When 'Jack' clicks Add A Meal
    Then the create meal form is pre-filled with that date

  Scenario: A meal created from a future date is saved on that date
    Given 'Jack' is on the home page for a future date
    When 'Jack' clicks Add A Meal
    And 'Jack' creates the meal with title 'Future Meal'
    Then the meal 'Future Meal' is saved on that future date
