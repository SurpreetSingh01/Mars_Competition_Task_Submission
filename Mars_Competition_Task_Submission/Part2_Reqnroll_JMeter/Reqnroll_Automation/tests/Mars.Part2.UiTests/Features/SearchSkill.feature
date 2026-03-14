@searchskill
Feature: Search Skill
  As an authenticated Mars user
  I want to search for skills
  So that I can find relevant service listings

  Background:
    Given I am signed in to Mars using the "DefaultUser" credentials

  Scenario: Search for a skill listing
    Given I have a searchable service listing using "DefaultShareSkill"
    When I open the search skill page
    And I search for a skill using "DefaultSearchSkill"
    Then search results for "DefaultSearchSkill" should be displayed
