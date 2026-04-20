Feature: Add time and date to meals

Scenario: User can edit the day of a meal
  Given there is a user named 'Jack'
  And 'Jack' is logged into Onebite
  And 'Jack' has a meal created
  And the edit meal page is open
  When 'Jack' selects meal date "2026-04-20"
  And User saves the meal
  Then the meal date field is saved as "2026-04-20"

Scenario: User can edit the month of a meal
  

Scenario: User can make a meal repeat weekly
  Given there is a user named 'Jack'
  And 'Jack' is logged into Onebite
  And 'Jack' has a meal created
  And the edit meal page is open
  When 'Jack' enables weekly repeat
  And User saves the meal
  Then the meal repeat rule is saved as weekly