Feature: WVT-178 Recipe rating shows correctly on filtered searches

  Background:
    Given there is a user named 'Gary'
    And 'Gary' is logged into Onebite

  Scenario: Recipe rating is visible when no filter is applied
    Given 'Gary' has a recipe named 'UpvotedOatmeal' with the tag 'Breakfast'
    And 'Gary' has upvoted the recipe 'UpvotedOatmeal'
    And 'Gary' is on the recipe search page
    When 'Gary' searches for 'UpvotedOatmeal'
    Then the recipe 'UpvotedOatmeal' appears in the search results with a rating above 0%

  Scenario: Recipe rating is still visible after a tag filter is applied
    Given 'Gary' has a recipe named 'UpvotedOatmeal' with the tag 'Breakfast'
    And 'Gary' has upvoted the recipe 'UpvotedOatmeal'
    And 'Gary' is on the recipe search page
    When 'Gary' searches for 'UpvotedOatmeal'
    And 'Gary' selects 'Breakfast' from the tag filter
    Then the recipe 'UpvotedOatmeal' appears in the search results with a rating above 0%
