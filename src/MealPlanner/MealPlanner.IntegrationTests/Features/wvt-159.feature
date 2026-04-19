Feature: WVT-159

  Background:
    Given there is a user named 'Jack'
    And 'Jack' is logged into Onebite
    And 'Jack' has a meal with recipes created

  Scenario: Deleting a meal on the home page keeps the user on the home page
    Given 'Jack' is on the home page for today
    When 'Jack' deletes a meal from the home page
    Then 'Jack' remains on the home page

  Scenario: Checking off a meal on the home page keeps the user on the home page
    Given 'Jack' is on the home page for today
    When 'Jack' checks off a meal on the home page
    Then 'Jack' remains on the home page
