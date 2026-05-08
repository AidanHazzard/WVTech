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

  Scenario: Previous Exports button is visible when shopping list has items
    Given 'Dave' has items on their shopping list
    And 'Dave' is on the shopping list page
    Then the 'Previous Exports' button is visible

  Scenario: Clicking Previous Exports opens the history modal
    Given 'Dave' has items on their shopping list
    And 'Dave' is on the shopping list page
    When 'Dave' clicks the 'Previous Exports' button
    Then the export history modal is displayed

  Scenario: Export history shows empty state when no exports exist
    Given 'Dave' has no previous Kroger exports
    And 'Dave' has items on their shopping list
    And 'Dave' is on the shopping list page
    When 'Dave' clicks the 'Previous Exports' button
    Then the export history modal shows 'No previous exports'

  Scenario: Export history shows past exports with item count and time
    Given 'Dave' has a previous Kroger export with 2 items
    And 'Dave' has items on their shopping list
    And 'Dave' is on the shopping list page
    When 'Dave' clicks the 'Previous Exports' button
    Then the export history modal shows an entry with '2 items'

  Scenario: Clicking an export entry shows the exported items
    Given 'Dave' has a previous Kroger export with 2 items
    And 'Dave' has items on their shopping list
    And 'Dave' is on the shopping list page
    When 'Dave' clicks the 'Previous Exports' button
    And 'Dave' clicks on the first export entry
    Then the items from that export are shown

  Scenario: Adding items from export history adds them to the shopping list
    Given 'Dave' has no previous Kroger exports
    And 'Dave' has a previous Kroger export with 2 items
    And 'Dave' has an empty shopping list
    And 'Dave' is on the shopping list page
    When 'Dave' clicks the 'Previous Exports' button
    And 'Dave' clicks on the first export entry
    And 'Dave' clicks the 'Add to Shopping List' button
    Then the shopping list shows the items from the export
