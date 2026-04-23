Feature: Add time and date to meals

Scenario: User can edit the date of a meal
  Given there is a user named 'Jack'
  And 'Jack' is logged into Onebite
  And 'Jack' has a meal created
  And the edit meal page is open
  When 'Jack' selects meal day "20"
  And 'Jack' selects the meal day "April"
  And User saves the meal
  Then the meal day field is saved as "20"
  And the meal month field is saved as "April"

Scenario: User can make a meal repeat weekly
  Given there is a user named 'Jack'
  And 'Jack' is logged into Onebite
  And 'Jack' has a meal created
  And the edit meal page is open
  When 'Jack' enables weekly repeat
  And User saves the meal
  Then the meal repeat rule is saved as weekly