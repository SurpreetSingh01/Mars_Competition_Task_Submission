@education
Feature: Education
  As an authenticated Mars user
  I want to maintain my education details
  So that my profile reflects my qualifications

  Background:
    Given I am signed in to Mars using the "DefaultUser" credentials

  Scenario: Add a new education record
    When I open the profile page
    And I add a new education record using "DefaultEducation"
    Then the education record "DefaultEducation" should appear on my profile
