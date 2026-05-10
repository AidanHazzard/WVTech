Feature: Auto remove pantry items when a meal is completed
# WVT-28

  Background:
    Given there is a user named 'Gary'
    And 'Gary' is logged into Onebite

  Scenario: Completing a meal with matching pantry items shows a removal prompt
    Given 'Gary' has no pantry items
    And 'Gary' has a pantry item named 'Chicken' with amount '2' and measurement 'Pound(s)'
    And 'Gary' has a meal named 'Dinner' today with a recipe containing ingredient 'Chicken'
    And 'Gary' is on the home page
    When 'Gary' marks the meal 'Dinner' as completed
    Then a pantry removal prompt is displayed

  Scenario: Accepting the removal prompt removes the matching ingredients from the pantry
    Given 'Gary' has no pantry items
    And 'Gary' has a pantry item named 'Chicken' with amount '2' and measurement 'Pound(s)'
    And 'Gary' has a meal named 'Dinner' today with a recipe containing ingredient 'Chicken'
    And 'Gary' is on the home page
    When 'Gary' marks the meal 'Dinner' as completed
    And 'Gary' accepts the pantry removal prompt
    And 'Gary' is on the pantry page
    Then 'Chicken' does not appear in the pantry list

  Scenario: Declining the removal prompt keeps the pantry unchanged
    Given 'Gary' has no pantry items
    And 'Gary' has a pantry item named 'Chicken' with amount '2' and measurement 'Pound(s)'
    And 'Gary' has a meal named 'Dinner' today with a recipe containing ingredient 'Chicken'
    And 'Gary' is on the home page
    When 'Gary' marks the meal 'Dinner' as completed
    And 'Gary' declines the pantry removal prompt
    And 'Gary' is on the pantry page
    Then 'Chicken' appears in the pantry list

  Scenario: Unchecking a completed meal that had auto-removed ingredients shows a restore prompt
    Given 'Gary' has no pantry items
    And 'Gary' has a meal named 'Dinner' today with a recipe containing ingredient 'Chicken'
    And 'Gary' has completed meal 'Dinner' with auto-removed pantry ingredient 'Chicken'
    And 'Gary' is on the home page
    When 'Gary' marks the meal 'Dinner' as incomplete
    Then a pantry restore prompt is displayed

  Scenario: Accepting the restore prompt adds the auto-removed ingredient back to the pantry
    Given 'Gary' has no pantry items
    And 'Gary' has a meal named 'Dinner' today with a recipe containing ingredient 'Chicken'
    And 'Gary' has completed meal 'Dinner' with auto-removed pantry ingredient 'Chicken'
    And 'Gary' is on the home page
    When 'Gary' marks the meal 'Dinner' as incomplete
    And 'Gary' accepts the pantry restore prompt
    And 'Gary' is on the pantry page
    Then 'Chicken' appears in the pantry list

  Scenario: Declining the restore prompt leaves the pantry unchanged
    Given 'Gary' has no pantry items
    And 'Gary' has a meal named 'Dinner' today with a recipe containing ingredient 'Chicken'
    And 'Gary' has completed meal 'Dinner' with auto-removed pantry ingredient 'Chicken'
    And 'Gary' is on the home page
    When 'Gary' marks the meal 'Dinner' as incomplete
    And 'Gary' declines the pantry restore prompt
    And 'Gary' is on the pantry page
    Then 'Chicken' does not appear in the pantry list
