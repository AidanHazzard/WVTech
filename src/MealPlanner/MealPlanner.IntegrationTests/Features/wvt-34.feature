Feature: Recipe voting
    Background:
        Given there is a user named 'Jack'
        And there is a recipe named 'Oatmeal Cookies' with no votes
    
    Scenario: Jack is on a recipe page with no votes
        Given 'Jack' is logged into Onebite
        And he is on the recipe page for the recipe
        Then he sees the recipe has a rating of '0%'
    
    Scenario: Jack upvotes a recipe that has no other votes
        Given 'Jack' is logged into Onebite
        And he is on the recipe page for the recipe
        When he upvotes the recipe
        Then he sees the recipe has a rating of '100%'

    Scenario: Gary downvotes a recipe with 1 upvotes
        Given 'Jack' had upvoted the recipe
        And there is a user named 'Gary'
        And 'Gary' is logged into Onebite
        And he is on the recipe page for the recipe
        When he downvotes the recipe
        Then he sees the recipe has a rating of "50%"
