Feature: Month and day dropdown meal scheduling

Scenario: User sees month and day dropdowns on new meal page
  Given there is a user named 'Jack'
  And 'Jack' is logged into Onebite
  When 'Jack' navigates to the new meal page
  Then the month dropdown is shown
  And the day dropdown is shown

Scenario: User creates a meal using month and day dropdowns
  Given there is a user named 'Jack'
  And 'Jack' is logged into Onebite
  And 'Jack' is on the new meal page
  When 'Jack' enters a meal title "Test Dropdown Meal"
  And 'Jack' selects month "April"
  And 'Jack' selects day "15"
  And 'Jack' saves the meal
  Then the meal form submits successfully

Scenario: User edits a meal with month and day dropdowns
  Given there is a user named 'Jack'
  And 'Jack' is logged into Onebite
  And 'Jack' has a meal created
  And the edit meal page is open
  When 'Jack' selects month "April"
  And 'Jack' selects day "20"
  And User saves the meal
  Then the edit meal form submits successfully