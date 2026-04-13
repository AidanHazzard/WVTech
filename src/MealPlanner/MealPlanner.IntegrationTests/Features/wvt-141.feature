Feature: WVT-141

  Background:
    Given there is a user named 'Jack'
    And 'Jack' is logged into Onebite
    And 'Jack' has a meal with recipes created

  Scenario: 'Jack' can see the meal title on the view meal page
    Given 'Jack' is on the view meal page
    Then the meal title is shown on the page