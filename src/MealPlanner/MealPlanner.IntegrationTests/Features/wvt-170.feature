Feature: Regenerate a single recipe within a meal

# WVT-170

  Background:
    Given there is a user named 'Gary'
    And 'Gary' is logged into Onebite
    And 'Gary' has a calorie target set

  Scenario: User sees a regenerate button next to each recipe on the meal detail page
    Given 'Gary' has a meal named 'Lunch' containing recipes 'Tofu Bowl' and 'Garden Salad'
    When 'Gary' opens the meal detail page for 'Lunch'
    Then a regenerate button is visible next to 'Tofu Bowl'
    And a regenerate button is visible next to 'Garden Salad'

  Scenario: Regenerate button is hidden when a meal contains only one recipe
    Given 'Gary' has a meal named 'Lunch' containing only a recipe 'Tofu Bowl'
    When 'Gary' opens the meal detail page for 'Lunch'
    Then no regenerate button is visible next to 'Tofu Bowl'

  Scenario: User sees a regenerate button next to each recipe on the day plan summary
    Given 'Gary' is viewing his generated day plan summary
    When 'Gary' looks at the recipes within a meal in the summary
    Then a regenerate button is visible next to each recipe in that meal

  Scenario: Regenerating one recipe replaces only that recipe and leaves the others alone
    Given 'Gary' has a meal named 'Lunch' containing recipes 'Tofu Bowl' and 'Garden Salad'
    And 'Gary' has an upvoted recipe named 'Quinoa Plate' that is not in 'Lunch'
    When 'Gary' opens the meal detail page for 'Lunch'
    And 'Gary' clicks regenerate next to 'Tofu Bowl'
    Then the meal still contains a recipe named 'Garden Salad'
    And the meal no longer contains a recipe named 'Tofu Bowl'
    And the meal contains a different recipe in place of 'Tofu Bowl'

  Scenario: Regenerating respects dietary restrictions
    Given 'Gary' has a 'Vegan' dietary restriction
    And 'Gary' has a meal named 'Lunch' containing a recipe 'Tofu Bowl' tagged 'Vegan' and a recipe 'Garden Salad' tagged 'Vegan'
    And 'Gary' has a recipe named 'Beef Stew' without any tags
    And 'Gary' has an upvoted recipe named 'Lentil Soup' tagged 'Vegan' that is not in 'Lunch'
    When 'Gary' opens the meal detail page for 'Lunch'
    And 'Gary' clicks regenerate next to 'Tofu Bowl'
    Then the meal contains a recipe named 'Lentil Soup'
    And the meal does not contain a recipe named 'Beef Stew'

  Scenario: User is told when no alternative recipe is available and the original stays
    Given 'Gary' has a meal named 'Lunch' containing recipes 'Tofu Bowl' and 'Garden Salad'
    And 'Gary' has no other eligible recipes to replace 'Tofu Bowl'
    When 'Gary' opens the meal detail page for 'Lunch'
    And 'Gary' clicks regenerate next to 'Tofu Bowl'
    Then 'Gary' sees a message that no alternative recipe is available
    And the meal still contains a recipe named 'Tofu Bowl'
    And the meal still contains a recipe named 'Garden Salad'

  Scenario: User undoes a regeneration and the original recipe is restored
    Given 'Gary' has a meal named 'Lunch' containing recipes 'Tofu Bowl' and 'Garden Salad'
    And 'Gary' has an upvoted recipe named 'Quinoa Plate' that is not in 'Lunch'
    When 'Gary' opens the meal detail page for 'Lunch'
    And 'Gary' clicks regenerate next to 'Tofu Bowl'
    And 'Gary' clicks the undo option for the regeneration
    Then the meal contains a recipe named 'Tofu Bowl'
    And the meal contains a recipe named 'Garden Salad'

  Scenario: Undo option disappears after the user navigates away
    Given 'Gary' has a meal named 'Lunch' containing recipes 'Tofu Bowl' and 'Garden Salad'
    And 'Gary' has an upvoted recipe named 'Quinoa Plate' that is not in 'Lunch'
    When 'Gary' opens the meal detail page for 'Lunch'
    And 'Gary' clicks regenerate next to 'Tofu Bowl'
    And 'Gary' navigates to the home page
    And 'Gary' returns to the meal detail page for 'Lunch'
    Then no undo option is visible on the meal detail page

  Scenario: User regenerates a recipe from the day plan summary
    Given 'Gary' is viewing his generated day plan summary
    And the first meal in the summary contains a recipe named 'Tofu Bowl'
    And 'Gary' has an upvoted recipe named 'Quinoa Plate' that is not in the day plan
    When 'Gary' clicks regenerate next to 'Tofu Bowl' in the summary
    Then the first meal in the summary no longer contains a recipe named 'Tofu Bowl'
    And the first meal in the summary contains a different recipe in place of 'Tofu Bowl'
    And all other meals in the summary remain unchanged
