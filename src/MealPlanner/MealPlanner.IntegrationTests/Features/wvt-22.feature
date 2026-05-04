Feature: WVT-22 Zip Code for Kroger Export

  Background:
    Given there is a user named 'Dave'
    And 'Dave' is logged into Onebite

  Scenario: User sees the zip code field on the shopping list page
    Given 'Dave' is on the shopping list page
    Then a zip code input field is visible

  Scenario: User can enter a zip code and export to Kroger
    Given 'Dave' is on the shopping list page
    When 'Dave' enters zip code '97401' and clicks export to Kroger
    Then the zip code '97401' is shown in the export section

  Scenario: Zip code persists after leaving and returning to the shopping list
    Given 'Dave' is on the shopping list page
    When 'Dave' enters zip code '97401' and clicks export to Kroger
    And 'Dave' navigates away and returns to the shopping list page
    Then the zip code '97401' is shown in the export section
