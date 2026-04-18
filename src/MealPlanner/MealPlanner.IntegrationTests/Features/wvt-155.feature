Feature: WVT-155

  Background:
    Given there is a user named 'Jack'
    And 'Jack' is logged into Onebite
    And 'Jack' has a meal with recipes created

  Scenario: User can navigate to recipe detail from view meal page
    Given 'Jack' is on the view meal page
    When 'Jack' clicks on a recipe name in the meal
    Then 'Jack' is navigated to that recipe's detail page

  Scenario: User can navigate to recipe detail from edit meal page
    Given 'Jack' is on the edit meal page
    When 'Jack' clicks on a recipe name in the meal
    Then 'Jack' is navigated to that recipe's detail page
