Feature: User Food Preference Selection
    As a user, 
    I want to be able to select 
    certain categories of food 
    that I would like for a given meal.

    Background:
        Given there is a user named "Gary"
        And Onebite has at least 2 tags
        And "Gary" is logged into Onebite

    Scenario: Gary sees food preferences on the user settings page
        When he navigates to the "UserSettings" page
        Then he sees the option to set food preference

    Scenario: Gary sets a food preference from the suggested category list
        Given he is on the "UserSettings" page
        When he selects the most popular tag from the food preference dropdown
        And he clicks save preference
        Then he sees the tag in a list of his food preferences
    
    Scenario: Gary reloads page after setting preference
        Given he is on the "UserSettings" page
        When he selects the most popular tag from the food preference dropdown
        And he clicks save preference
        And he reloads the page
        Then he sees the tag in a list of his food preferences

    Scenario: Gary types in a preference that is not in the most popular list
        Given the tag "New Tag" is not in the database
        And he is on the "UserSettings" page
        When he types "New Tag" into the custom food preference input
        And he clicks add tag
        And he clicks save preference
        Then he sees the tag in a list of his food preferences

    Scenario: Gary sets multiple food preferences at once
        Given he is on the "UserSettings" page
        When he selects the most popular tag from the food preference dropdown
        And he selects the next most popular tag from the food preference dropdown
        And he clicks save preference
        Then he sees both tags in his list of preferences

    Scenario: Gary removes a food preference
        Given "Gary" has the food preference "Italian"
        And he is on the "UserSettings" page
        When he clicks on the remove food preference button for "Italian"
        Then he has no food preferences

    Scenario: Gary sets a food preference when he already has one
        Given "Gary" has the food preference "Italian"
        And he is on the "UserSettings" page
        When he selects a tag that is not "Italian"
        And he clicks save preference
        Then he sees both tags in his list of preferences

    Scenario: Gary removes a food preference when he has multiple
        Given "Gary" has the food preference "Italian"
        And "Gary" has the food preference "Cheap"
        And he is on the "UserSettings" page
        When he clicks on the remove food preference button for "Italian"
        Then he sees only the tag "Cheap" in his list of preferences

    Scenario: Gary selects a food preference from the popular list and types one in
        Given the tag "New Tag" is not in the database
        And he is on the "UserSettings" page
        When he selects the most popular tag from the food preference dropdown
        And he types "New Tag" into the custom food preference input
        And he clicks add tag
        And he clicks save preference
        Then he sees both tags in his list of preferences
