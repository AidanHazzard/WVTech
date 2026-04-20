Feature: Add meal title

Background:  
  Given there is a user named 'Jack'
  And 'Jack' is logged into Onebite
  And 'Jack' is on the new meal page

Scenario: User must provide a title when creating a meal
  When 'Jack' enters a meal title "Breakfast"
  And 'Jack' saves the meal
  Then the meal creation form submits successfully

Scenario: Meal title appears on the planner index page
  When 'Jack' enters a meal title "Lunch Block"
  And 'Jack' saves the meal
  Then the planner shows the meal title "Lunch Block"

Scenario: User provides only title and date, not time
  When 'Jack' enters a meal title "Dinner"
  And 'Jack' saves the meal
  Then the new meal page does not require a time field