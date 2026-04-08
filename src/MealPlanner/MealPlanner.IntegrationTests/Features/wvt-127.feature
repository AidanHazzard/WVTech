Feature: WVT-127
    Background:
        Given a user is on the edit meal page

    Scenario: User can see all recipes associated with the meal
        Then all associated recipes are displayed

    Scenario: Meal title updates immediately when edited
        When User updates the meal title
        Then the updated meal title is shown immediately

    Scenario: Updated meal title is saved with the meal
        When User updates the meal title
        And User saves the meal
        Then the meal is saved with the updated title