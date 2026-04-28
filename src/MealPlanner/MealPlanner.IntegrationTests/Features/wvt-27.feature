Feature: Manually add item to pantry
# WVT-27

  Background:
    Given there is a user named 'Gary'
    And 'Gary' is logged into Onebite
    And 'Gary' is on the pantry page

  Scenario: User adds a pantry item and it appears in the pantry list
    When 'Gary' submits the add pantry item form with name 'Milk', amount '2', and measurement 'cups'
    Then 'Milk' appears in the pantry list

  Scenario: Added item shows the correct details in the pantry list
    When 'Gary' submits the add pantry item form with name 'Butter', amount '1', and measurement 'lbs'
    Then the pantry list shows 'Butter' with an amount of '1' and measurement 'lbs'

  Scenario: A confirmation message is shown after successfully adding an item
    When 'Gary' submits the add pantry item form with name 'Eggs', amount '12', and measurement 'pieces'
    Then a success message is displayed on the pantry page

  Scenario: Submitting the form with no name shows a validation error
    When 'Gary' submits the add pantry item form with no name, amount '3', and measurement 'cups'
    Then an error message is displayed on the pantry page
    And no new item is added to the pantry list
