Feature: WVT-165

  Background:
    Given there is a user named 'Frank'
    And 'Frank' is logged into Onebite

  # --- View Meal: remove recipe from meal ---

  Scenario: Removing a recipe from the view meal page shows an in-app confirmation
    Given 'Frank' has a meal with a recipe
    And 'Frank' is on the view meal page
    When 'Frank' clicks the remove recipe button
    Then an in-app confirmation is shown instead of a browser dialog

  Scenario: Cancelling recipe removal on the view meal page keeps the recipe
    Given 'Frank' has a meal with a recipe
    And 'Frank' is on the view meal page
    When 'Frank' clicks the remove recipe button
    And 'Frank' clicks cancel on the in-app confirmation
    Then the recipe is still visible on the page

  Scenario: Confirming recipe removal on the view meal page removes the recipe
    Given 'Frank' has a meal with a recipe
    And 'Frank' is on the view meal page
    When 'Frank' clicks the remove recipe button
    And 'Frank' clicks confirm on the in-app confirmation
    Then the recipe is no longer visible on the page

  # --- Edit Meal: remove recipe from meal ---

  Scenario: Removing a recipe from the edit meal page shows an in-app confirmation
    Given 'Frank' has a meal with a recipe
    And 'Frank' is on the edit meal page
    When 'Frank' clicks the remove recipe button
    Then an in-app confirmation is shown instead of a browser dialog

  Scenario: Cancelling recipe removal on the edit meal page keeps the recipe
    Given 'Frank' has a meal with a recipe
    And 'Frank' is on the edit meal page
    When 'Frank' clicks the remove recipe button
    And 'Frank' clicks cancel on the in-app confirmation
    Then the recipe is still visible on the page

  Scenario: Confirming recipe removal on the edit meal page removes the recipe
    Given 'Frank' has a meal with a recipe
    And 'Frank' is on the edit meal page
    When 'Frank' clicks the remove recipe button
    And 'Frank' clicks confirm on the in-app confirmation
    Then the recipe is no longer visible on the page

  # --- Create Meal: remove recipe from form ---

  Scenario: Removing a recipe while creating a meal shows an in-app confirmation
    Given 'Frank' is on the create meal page with a recipe added to the list
    When 'Frank' clicks the remove recipe button
    Then an in-app confirmation is shown instead of a browser dialog

  Scenario: Cancelling recipe removal on the create meal page keeps the recipe
    Given 'Frank' is on the create meal page with a recipe added to the list
    When 'Frank' clicks the remove recipe button
    And 'Frank' clicks cancel on the in-app confirmation
    Then the recipe is still visible on the page

  Scenario: Confirming recipe removal on the create meal page removes the recipe
    Given 'Frank' is on the create meal page with a recipe added to the list
    When 'Frank' clicks the remove recipe button
    And 'Frank' clicks confirm on the in-app confirmation
    Then the recipe is no longer visible on the page

  # --- Recipe Library: delete owned recipe ---

  Scenario: Deleting a recipe from the recipe library shows an in-app confirmation
    Given 'Frank' has an owned recipe in the recipe library
    And 'Frank' is on the recipe library page
    When 'Frank' clicks the delete recipe button
    Then an in-app confirmation is shown instead of a browser dialog

  Scenario: Cancelling recipe deletion keeps the recipe in the library
    Given 'Frank' has an owned recipe in the recipe library
    And 'Frank' is on the recipe library page
    When 'Frank' clicks the delete recipe button
    And 'Frank' clicks cancel on the in-app confirmation
    Then the recipe is still visible on the page

  Scenario: Confirming recipe deletion removes the recipe from the library
    Given 'Frank' has an owned recipe in the recipe library
    And 'Frank' is on the recipe library page
    When 'Frank' clicks the delete recipe button
    And 'Frank' clicks confirm on the in-app confirmation
    Then the recipe is no longer visible on the page

  # --- Edit Meal: delete entire meal ---

  Scenario: Deleting a meal from the edit meal page shows an in-app confirmation
    Given 'Frank' has a meal with a recipe
    And 'Frank' is on the edit meal page
    When 'Frank' clicks the delete meal button
    Then an in-app confirmation is shown instead of a browser dialog

  Scenario: Cancelling meal deletion on the edit meal page keeps the meal
    Given 'Frank' has a meal with a recipe
    And 'Frank' is on the edit meal page
    When 'Frank' clicks the delete meal button
    And 'Frank' clicks cancel on the in-app confirmation
    Then 'Frank' is still on the edit meal page

  Scenario: Confirming meal deletion on the edit meal page removes the meal
    Given 'Frank' has a meal with a recipe
    And 'Frank' is on the edit meal page
    When 'Frank' clicks the delete meal button
    And 'Frank' clicks confirm on the in-app confirmation
    Then 'Frank' is redirected away from the edit meal page

  # --- Home Page: delete entire meal ---

  Scenario: Deleting a meal from the home page shows an in-app confirmation
    Given 'Frank' has a meal scheduled for today
    And 'Frank' is on the planner home page
    When 'Frank' clicks the delete meal button
    Then an in-app confirmation is shown instead of a browser dialog

  Scenario: Cancelling meal deletion on the home page keeps the meal card
    Given 'Frank' has a meal scheduled for today
    And 'Frank' is on the planner home page
    When 'Frank' clicks the delete meal button
    And 'Frank' clicks cancel on the in-app confirmation
    Then the meal card is still visible on the home page

  Scenario: Confirming meal deletion on the home page removes the meal card
    Given 'Frank' has a meal scheduled for today
    And 'Frank' is on the planner home page
    When 'Frank' clicks the delete meal button
    And 'Frank' clicks confirm on the in-app confirmation
    Then the meal card is no longer visible on the home page
