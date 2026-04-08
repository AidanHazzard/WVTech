Feature: wvt-34
    Background:
        Given There is a user named 'Jack'
        And There is a user named 'Katy'
        And There is a user named 'Bob'
        And There is a user named 'Gary'
        And There is a recipe page 'recipe/1'
    
    Scenario: Jack is on a recipe page with no votes
        Given 'Jack' is logged into Onebite
        And They are on the 'recipe/1' page
        Then They see the recipe has a rating of '0%'
    
    Scenario: Jack upvotes a recipe that has no other votes
        Given 'Jack' is logged into Onebite
        And They are on the 'recipe/1' page
        When 'Jack' upvotes the recipe
        Then They see the recipe has a rating of '100%'
    
    Scenario: Gary downvotes a recipe that has 1 upvote
        Given 'Gary' is logged into Onebite
        And 'Jack' is logged into Onebite
        And They are on the 'recipe/1' page
        When 'Jack' upvotes the recipe
        And 'Gary' downvotes the recipe
        Then They see the recipe has a rating of '0%'
    
    Scenario: Katy downvotes a recipe that has other votes
        Given 'Katy' is logged into Onebite
        * 'Jack' is logged into Onebite
        * 'Gary' is logged into Onebite
        * 'Bob' is logged into Onebite
        And They are on the 'recipe/1' page
        When 'Bob' upvotes the recipe
        * 'Jack' upvotes the recipe
        * 'Gary' downvotes the recipe
        And 'Katy' downvotes the recipe
        Then They see the recipe has a rating of '50%'

    
        
