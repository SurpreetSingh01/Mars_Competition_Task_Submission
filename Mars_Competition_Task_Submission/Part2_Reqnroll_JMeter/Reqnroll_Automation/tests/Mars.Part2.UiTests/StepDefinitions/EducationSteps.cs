using Mars.Part2.UiTests.Models;
using Mars.Part2.UiTests.Pages.Profile;
using NUnit.Framework;
using Reqnroll;

namespace Mars.Part2.UiTests.StepDefinitions;

[Binding]
public sealed class EducationSteps
{
    private readonly UiTestContext _uiContext;

    public EducationSteps(ScenarioContext scenarioContext)
    {
        _uiContext = scenarioContext.Get<UiTestContext>();
    }

    [When("I add a new education record using {string}")]
    public void WhenIAddANewEducationRecordUsing(string dataKey)
    {
        var data = _uiContext.TestData.ReadRequired<EducationData>("TestData/education.json", dataKey);
        new ProfilePage(_uiContext).Education.AddEducation(data);
    }

    [Then("the education record {string} should appear on my profile")]
    public void ThenTheEducationRecordShouldAppearOnMyProfile(string dataKey)
    {
        var data = _uiContext.TestData.ReadRequired<EducationData>("TestData/education.json", dataKey);
        var exists = new ProfilePage(_uiContext).Education.HasEducationRecord(data);
        Assert.That(exists, Is.True, "Expected the education record to be visible on the profile page.");
    }
}
