Feature: ThemeChange

  Background:
    Given there is a user named 'bob'
    And 'bob' is logged into Onebite
    And 'bob' has a user profile

  Scenario: 'bob' toggles from light to dark theme
    Given 'bob' is on the page 'UserSettings'
    When 'bob' clicks the change theme button
    Then 'bob' has dark theme enabled in the database

  Scenario: 'bob' toggles back to light theme
    Given 'bob' is on the page 'UserSettings'
    When 'bob' clicks the change theme button
    Given 'bob' is on the page 'UserSettings'
    When 'bob' clicks the change theme button
    Then 'bob' has light theme enabled in the database