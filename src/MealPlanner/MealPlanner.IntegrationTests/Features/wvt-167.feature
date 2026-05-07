Feature: WVT-167 — Recipe Images

  Background:
    Given there is a user named 'Gary'
    And 'Gary' is logged into Onebite

  Scenario: User uploads an image when creating a recipe
    Given 'Gary' navigates to the create recipe page
    When 'Gary' fills in the recipe details and uploads an image file
    And 'Gary' submits the new recipe
    Then 'Gary' sees the recipe page with the uploaded image displayed

  Scenario: User changes the image on an existing recipe
    Given 'Gary' has a WVT167 recipe with an image
    And 'Gary' navigates to the edit recipe page for that recipe
    When 'Gary' uploads a different image file
    And 'Gary' saves the recipe
    Then 'Gary' sees the recipe page with the new image displayed

  Scenario: User removes the image from an existing recipe
    Given 'Gary' has a WVT167 recipe with an image
    And 'Gary' navigates to the edit recipe page for that recipe
    When 'Gary' removes the recipe image
    And 'Gary' saves the recipe
    Then 'Gary' sees the recipe page with the placeholder image displayed

  Scenario: Recipe without an image shows a placeholder thumbnail in search results
    Given 'Gary' has a recipe named 'Vanilla Pudding' with no image
    When 'Gary' searches for 'Vanilla Pudding'
    Then 'Gary' sees a placeholder image next to 'Vanilla Pudding' in the search results

  Scenario: Recipe with an image shows its thumbnail in search results
    Given 'Gary' has a recipe named 'Chocolate Cake' with an image
    When 'Gary' searches for 'Chocolate Cake'
    Then 'Gary' sees the recipe image thumbnail next to 'Chocolate Cake' in the search results

  Scenario: Meal card on the home page shows a collage of recipe images ordered by calories
    Given 'Gary' has a meal planned for today containing multiple recipes with images
    When 'Gary' visits the planner home page
    Then 'Gary' sees multiple recipe images displayed on the meal card
    And the recipe images on the meal card appear in order from highest to lowest calorie count
