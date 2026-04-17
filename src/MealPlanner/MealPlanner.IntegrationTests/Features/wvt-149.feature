Feature: WVT-142

  Background:
    Given there is a user named 'Jack'
    And 'Jack' is logged into Onebite
    And 'Jack' is on the user settings page

  Scenario: 'Jack' can see the theme toggle on the settings page
    Then a theme toggle is shown on the settings page

  Scenario: 'Jack' can toggle the theme
    When 'Jack' clicks the theme toggle
    Then the theme changes