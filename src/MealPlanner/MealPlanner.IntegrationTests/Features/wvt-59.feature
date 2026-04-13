Feature: WVT-59
    As a user, 
    I want to be able to have recommended meals 
    based on my caloric needs

    Background: 
        Given there is a user named 'Jack'
        And 'Jack' is logged into Onebite

    Scenario: Jack can press a "recommend meal" button
        When he navigates to the 'Meal/NewMeal' page
        Then he sees a recommend meal button
    
    Scenario: Jack has a meal recommended for him for the current day
        Given he is on the "Meal/NewMeal" page
        When he clicks the recommend meal button
        And he enters the meal title
        And he clicks the generate meal button
        Then he is redirected to that meals meal page
        And he sees a newly generated meal with that title
    
    Scenario: Jack is recommended food he has upvoted
        Given 'Jack' has no other meals
        And 'Jack' had upvoted the recipe with id -1
        And no other recipes have been upvoted
        When he has a meal recommended for him
        Then his new meal contains the recipe with id -1
    
    Scenario: Jack is recommended a Meal within his caloric requirements
        Given 'Jack' has no other meals
        And 'Jack' has a daily calorie limit of 1800 calories
        When he has a meal recommended for him
        Then he sees a newly generated meal with at least one recipe
        And the meal has less total calories than 'Jack's calorie limit
    
    Scenario: Jack only wants oatmeal cookies
        Given 'Jack' has no other meals
        And he has downvoted all recipes other than 'Oatmeal Cookies'
        When he has a meal recommended for him
        Then he sees a newly generated meal with only 'Oatmeal Cookies'
    
    Scenario: Meal recommendation doesn't repeat meals on the same day
        Given 'Jack' has no other meals
        And 'Jack' has no daily calorie limit
        And 'Jack' has a meal with all recipes other than 'Oatmeal Cookies'
        And 'Jack' had upvoted the recipe with id -1
        When he has a meal recommended for him
        Then he sees a newly generated meal with only 'Oatmeal Cookies'
    
    Scenario: Meal recommendation includes other meals for caloric requirements
        Given 'Jack' has no other meals
        And 'Jack' has another meal that has a recipe with 900 calories
        And 'Jack' has a daily calorie limit of 1800 calories
        When he has a meal recommended for him
        Then he sees a newly generated meal with at least one recipe
        And all meals in the day have less combined calories than 'Jack's calorie limit