Feature: Nutrition page display

Scenario: User sees nutrition bars on the nutrition page
  Given there is a user named 'Jack'
  And 'Jack' is logged into Onebite
  When 'Jack' navigates to the nutrition page
  Then the calories nutrition bar is shown
  And the protein nutrition bar is shown
  And the fat nutrition bar is shown
  And the carbs nutrition bar is shown

Scenario: User sees nutrition target fractions on the nutrition page
  Given there is a user named 'Jack'
  And 'Jack' is logged into Onebite
  When 'Jack' navigates to the nutrition page
  Then the calories nutrition fraction is shown
  And the protein nutrition fraction is shown
  And the fat nutrition fraction is shown
  And the carbs nutrition fraction is shown