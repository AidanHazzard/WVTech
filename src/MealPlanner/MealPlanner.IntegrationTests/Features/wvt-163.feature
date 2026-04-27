Feature: WVT-163

  Background:
    Given there is a user named 'Dave'
    And 'Dave' is logged into Onebite

  Scenario: Single meal is shown as a card on the home page
    Given 'Dave' has 1 meal scheduled for today
    When 'Dave' navigates to the home page
    Then there is 1 meal card on the home page

  Scenario: All meal cards are displayed when many meals are scheduled
    Given 'Dave' has 6 meals scheduled for today
    When 'Dave' navigates to the home page
    Then there are 6 meal cards on the home page

  Scenario: Each meal card title is readable when many meals exist
    Given 'Dave' has 4 meals scheduled for today
    When 'Dave' navigates to the home page
    Then all meal card titles are readable on the home page
