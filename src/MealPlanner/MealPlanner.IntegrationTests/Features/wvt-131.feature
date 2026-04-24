Feature: WVT-131

# Meals Can Be Scheduled For More Than Once A Week

  Background:
    Given there is a user named 'Gary'
    And 'Gary' is logged into Onebite

  Scenario: Enabling weekly repeat reveals day-of-week checkboxes
    Given 'Gary' is on the create meal page
    When 'Gary' enables weekly repeat
    Then day-of-week checkboxes are visible on the page

  Scenario: Meal appears on every selected repeat day
    Given 'Gary' is on the create meal page
    And 'Gary' enters the meal title 'Multi-Day Meal'
    And 'Gary' sets the meal date to next Monday
    And 'Gary' enables weekly repeat
    And 'Gary' selects Monday, Tuesday, and Thursday as repeat days
    When 'Gary' creates the meal
    Then the meal 'Multi-Day Meal' appears on the planner on Monday
    And the meal 'Multi-Day Meal' appears on the planner on Tuesday
    And the meal 'Multi-Day Meal' appears on the planner on Thursday

  Scenario: Meal does not appear on unselected repeat days
    Given 'Gary' is on the create meal page
    And 'Gary' enters the meal title 'Selective Meal'
    And 'Gary' sets the meal date to next Monday
    And 'Gary' enables weekly repeat
    And 'Gary' selects Monday and Wednesday as repeat days
    When 'Gary' creates the meal
    Then the meal 'Selective Meal' does not appear on the planner on Tuesday
    And the meal 'Selective Meal' does not appear on the planner on Thursday

  Scenario: Previously selected repeat days are pre-checked when editing
    Given 'Gary' has a weekly repeating meal titled 'Protein Shake' scheduled on Monday and Wednesday
    When 'Gary' opens the edit meal page for 'Protein Shake'
    Then the Monday repeat day checkbox is checked
    And the Wednesday repeat day checkbox is checked
    And the Tuesday repeat day checkbox is not checked

  Scenario: Edit a meal to change its repeat days
    Given 'Gary' has a weekly repeating meal titled 'Morning Run Fuel' scheduled on Monday and Wednesday
    And 'Gary' opens the edit meal page for 'Morning Run Fuel'
    When 'Gary' deselects Wednesday from the repeat days
    And 'Gary' selects Friday as a repeat day
    And 'Gary' saves the meal changes
    Then the meal 'Morning Run Fuel' appears on the planner on Monday
    And the meal 'Morning Run Fuel' appears on the planner on Friday
    And the meal 'Morning Run Fuel' does not appear on the planner on Wednesday
