Feature: Remove meal from planner

Scenario: User can remove a meal from the planner index page
  Given there is a user named 'Jack'
  And 'Jack' is logged into Onebite
  And 'Jack' has a meal created
  And 'Jack' is on the planner page
  When 'Jack' clicks the delete meal button
  And 'Jack' confirms meal deletion
  Then the meal is removed from the planner page

Scenario: A confirmation alert appears before deletion
  Given there is a user named 'Jack'
  And 'Jack' is logged into Onebite
  And 'Jack' has a meal created
  And 'Jack' is on the planner page
  When 'Jack' clicks the delete meal button
  Then a delete confirmation alert is shown

Scenario: Removing one repeated meal does not remove future repeated meals
  Given there is a user named 'Jack'
  And 'Jack' is logged into Onebite
  And 'Jack' has a weekly repeating meal
  And 'Jack' is on the planner page
  When 'Jack' clicks the delete meal button
  And 'Jack' confirms meal deletion
  Then future repeated meals still exist