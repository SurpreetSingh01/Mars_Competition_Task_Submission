using Mars.Part2.UiTests.Models;
using Mars.Part2.UiTests.Pages.Home;
using Mars.Part2.UiTests.Pages.Profile;
using Reqnroll;

namespace Mars.Part2.UiTests.StepDefinitions;

[Binding]
public sealed class CommonNavigationSteps
{
    private readonly ScenarioContext _scenarioContext;
    private readonly UiTestContext _uiContext;

    public CommonNavigationSteps(ScenarioContext scenarioContext)
    {
        _scenarioContext = scenarioContext;
        _uiContext = scenarioContext.Get<UiTestContext>();
    }

    [Given("I am signed in to Mars using the {string} credentials")]
    public void GivenIAmSignedInToMarsUsingTheCredentials(string credentialKey)
    {
        var credentials = _uiContext.TestData
            .ReadRequired<Credentials>(_uiContext.Settings.Authentication.CredentialsFile, credentialKey)
            .MergeEnvironmentOverrides();

        var homePage = new HomePage(_uiContext);
        homePage.SignIn(credentials);

        var profilePage = new ProfilePage(_uiContext);
        profilePage.Open();
        profilePage.EnsureAuthenticated(credentials);

        _scenarioContext.Set(credentials, nameof(Credentials));
    }

    [Given("I open the profile page")]
    [When("I open the profile page")]
    public void GivenIOpenTheProfilePage()
    {
        new ProfilePage(_uiContext).Open();
    }
}
