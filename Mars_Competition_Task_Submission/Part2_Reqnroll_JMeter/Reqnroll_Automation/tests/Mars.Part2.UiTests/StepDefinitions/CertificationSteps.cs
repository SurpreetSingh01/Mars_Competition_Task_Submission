using Mars.Part2.UiTests.Models;
using Mars.Part2.UiTests.Pages.Profile;
using NUnit.Framework;
using Reqnroll;

namespace Mars.Part2.UiTests.StepDefinitions;

[Binding]
public sealed class CertificationSteps
{
    private const string RuntimeCertificationDataKey = "RuntimeCertificationData";

    private readonly UiTestContext _uiContext;
    private readonly ScenarioContext _scenarioContext;

    public CertificationSteps(ScenarioContext scenarioContext)
    {
        _scenarioContext = scenarioContext;
        _uiContext = scenarioContext.Get<UiTestContext>();
    }

    [When("I add a new certification record using {string}")]
    public void WhenIAddANewCertificationRecordUsing(string dataKey)
    {
        var source = _uiContext.TestData.ReadRequired<CertificationData>("TestData/certification.json", dataKey);
        var data = new CertificationData
        {
            Certificate = $"{source.Certificate} {DateTime.Now:yyyyMMddHHmmss}",
            From = source.From,
            Year = source.Year
        };

        new ProfilePage(_uiContext).Certifications.AddCertification(data);
        _scenarioContext.Set(data, RuntimeCertificationDataKey);
    }

    [Then("the certification record {string} should appear on my profile")]
    public void ThenTheCertificationRecordShouldAppearOnMyProfile(string dataKey)
    {
        var data = _scenarioContext.TryGetValue<CertificationData>(RuntimeCertificationDataKey, out var runtimeData) &&
                   runtimeData is not null
            ? runtimeData
            : _uiContext.TestData.ReadRequired<CertificationData>("TestData/certification.json", dataKey);

        var exists = new ProfilePage(_uiContext).Certifications.HasCertification(data);
        Assert.That(exists, Is.True, "Expected the certification record to be visible on the profile page.");
    }
}
