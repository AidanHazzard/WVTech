Feature: WVT-173 — Custom Food Preference Enter Key

  Background:
    Given there is a user named 'Gary'
    And 'Gary' is logged into Onebite
    And 'Gary' navigates to the user settings page

  Scenario: Pressing Enter in the custom preference input adds the preference to the pending list
    When 'Gary' types 'Sushi' into the custom food preference input and presses Enter
    Then 'Gary' sees 'Sushi' in the pending food preferences list

  Scenario: Pressing Enter with an empty custom preference input does nothing
    When 'Gary' presses Enter in an empty custom food preference input
    Then no new item appears in the pending food preferences list

  Scenario: Preferences added by pressing Enter can be saved
    When 'Gary' types 'Tacos' into the custom food preference input and presses Enter
    And 'Gary' clicks the Save Preferences button
    And 'Gary' navigates to the user settings page
    Then 'Gary' sees 'Tacos' in the saved food preferences list
