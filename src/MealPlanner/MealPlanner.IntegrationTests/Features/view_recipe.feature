Feature: View Recipe
    Scenario: User navigates to existing recipe
        Given the user is on the Search Recipes page
        And the user searchs for 'Oatmeal Cookies'
        When the user clicks on the first search result
        Then the user navigates to the 'Oatmeal Cookies' recipe page
    
    Scenario: User attempts to navigate to non-existant recipe
        Given the recipe with id 0 doesnt exist
        When the user navigates to 'Recipes/0'
        Then the user is redirected to SelectType