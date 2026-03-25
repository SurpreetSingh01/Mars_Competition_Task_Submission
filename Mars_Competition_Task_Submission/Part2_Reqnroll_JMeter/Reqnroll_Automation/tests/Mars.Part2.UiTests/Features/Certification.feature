@certification
Feature: Certification
  As an authenticated Mars user
  I want to maintain my certifications
  So that my profile reflects my credentials

  Background:
    Given I am signed in to Mars using the "DefaultUser" credentials

  Scenario: Add a new certification record
    When I open the profile page
    And I add a new certification record using "DefaultCertification"
    Then the certification record "DefaultCertification" should appear on my profile
