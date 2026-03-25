@shareskill
Feature: Share Skill
  As an authenticated Mars user
  I want to publish a skill listing
  So that other users can find my service

  Background:
    Given I am signed in to Mars using the "DefaultUser" credentials

  Scenario: Create a new service listing
    When I open the share skill page
    And I create a service listing using "DefaultShareSkill"
    Then the service listing "DefaultShareSkill" should be visible in listing management
