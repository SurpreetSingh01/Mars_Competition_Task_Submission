using Mars.Part2.UiTests.Helpers;
using Mars.Part2.UiTests.Models;
using Mars.Part2.UiTests.Pages.Marketplace;
using NUnit.Framework;
using Reqnroll;

namespace Mars.Part2.UiTests.StepDefinitions;

[Binding]
public sealed class ShareSkillSteps
{
    private const string RuntimeShareSkillDataKey = "RuntimeShareSkillData";
    private const string RuntimeCreatedListingTitleKey = "RuntimeCreatedListingTitle";

    private readonly ScenarioContext _scenarioContext;
    private readonly UiTestContext _uiContext;
    private readonly ShareSkillPage _shareSkillPage;
    private readonly ListingManagementPage _listingManagementPage;

    public ShareSkillSteps(ScenarioContext scenarioContext)
    {
        _scenarioContext = scenarioContext;
        _uiContext = scenarioContext.Get<UiTestContext>();
        _shareSkillPage = new ShareSkillPage(_uiContext);
        _listingManagementPage = new ListingManagementPage(_uiContext);
    }

    [When("I open the share skill page")]
    public void WhenIOpenTheShareSkillPage()
    {
        _shareSkillPage.Open();
    }

    [When("I create a service listing using {string}")]
    public void WhenICreateAServiceListingUsing(string dataKey)
    {
        var source = _uiContext.TestData.ReadRequired<ShareSkillData>("TestData/shareskill.json", dataKey);
        var data = RuntimeDataFactory.CreateUniqueShareSkill(source);

        _shareSkillPage.Open();
        _shareSkillPage.CreateListing(data);

        _listingManagementPage.Open();
        var listingAfterSave = _listingManagementPage.GetTopListingTitle();

        _scenarioContext.Set(data, RuntimeShareSkillDataKey);
        _scenarioContext.Set(listingAfterSave, RuntimeCreatedListingTitleKey);
    }

    [Then("the service listing {string} should be visible in listing management")]
    public void ThenTheServiceListingShouldBeVisibleInListingManagement(string dataKey)
    {
        _listingManagementPage.Open();

        var expectedTitle = ResolveCreatedListingTitle(dataKey);
        var exists = _listingManagementPage.HasListing(expectedTitle);
        Assert.That(exists, Is.True, "Expected the new listing to be visible in listing management.");
    }

    private string ResolveCreatedListingTitle(string dataKey)
    {
        if (_scenarioContext.TryGetValue<string>(RuntimeCreatedListingTitleKey, out var actualTitle) &&
            !string.IsNullOrWhiteSpace(actualTitle))
        {
            return actualTitle;
        }

        var data = ResolveRuntimeOrStaticData(dataKey);
        return data.Title;
    }

    private ShareSkillData ResolveRuntimeOrStaticData(string dataKey)
    {
        if (_scenarioContext.TryGetValue<ShareSkillData>(RuntimeShareSkillDataKey, out var runtimeData) && runtimeData is not null)
        {
            return runtimeData;
        }

        return _uiContext.TestData.ReadRequired<ShareSkillData>("TestData/shareskill.json", dataKey);
    }
}
