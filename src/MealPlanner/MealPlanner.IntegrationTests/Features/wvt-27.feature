Feature: Manually add item to pantry
# WVT-27

  Background:
    Given there is a user named 'Gary'
    And 'Gary' is logged into Onebite
    And 'Gary' is on the pantry page

  Scenario: User adds a pantry item and it appears in the pantry list
    When 'Gary' submits the add pantry item form with name 'Milk', amount '2', and measurement 'Cup(s)'
    Then 'Milk' appears in the pantry list

  Scenario: Added item shows the correct details in the pantry list
    When 'Gary' submits the add pantry item form with name 'Butter', amount '1', and measurement 'Pounds'
    Then the pantry list shows 'Butter' with an amount of '1' and measurement 'Pound'

  Scenario: A confirmation message is shown after successfully adding an item
    When 'Gary' submits the add pantry item form with name 'Eggs', amount '12', and measurement 'Count'
    Then a success message is displayed on the pantry page

  Scenario: Submitting the form with no name shows a validation error
    When 'Gary' submits the add pantry item form with no name, amount '3', and measurement 'Cup(s)'
    Then an error message is displayed on the pantry page
    And no new item is added to the pantry list

  Scenario: Removing the only pantry item leaves the pantry empty
    Given 'Gary' has a pantry item named 'Flour' with amount '3' and measurement 'Cup(s)'
    When 'Gary' removes the pantry item named 'Flour'
    Then the pantry list is empty

  Scenario: Removing one item from a pantry with multiple items leaves the others intact
    Given 'Gary' has a pantry item named 'Sugar' with amount '2' and measurement 'Cup(s)'
    And 'Gary' has a pantry item named 'Salt' with amount '1' and measurement 'Ounce(s)'
    When 'Gary' removes the pantry item named 'Sugar'
    Then 'Salt' appears in the pantry list
    And 'Sugar' does not appear in the pantry list
