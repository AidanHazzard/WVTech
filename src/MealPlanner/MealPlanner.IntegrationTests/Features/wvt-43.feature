Feature: Recipe search
    Background:
        Given User is on recipe page
        
    Scenario: User sees search bar
        Then The user sees a search bar

    Scenario: User searches for recipe
        When User types 'Oatmeal' in the search
        Then The search list populates with recipes that have 'Oatmeal' in their name

    Scenario: No search results 
        When User types 'I dont exist' in the search
        Then User is told there are no recipes found

    Scenario: Search is case-insensitive
        Given User had searched for 'Oatmeal'
        When User types 'oatmeal' in the search
        Then The search results are the same
        
