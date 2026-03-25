using Mars.Part2.UiTests.Helpers;
using Mars.Part2.UiTests.Models;
using Mars.Part2.UiTests.Pages.Marketplace;
using NUnit.Framework;
using Reqnroll;

namespace Mars.Part2.UiTests.StepDefinitions;

[Binding]
public sealed class SearchSkillSteps
{
    private const string RuntimeShareSkillDataKey = "RuntimeShareSkillData";
    private const string RuntimeSearchSkillDataKey = "RuntimeSearchSkillData";
    private const string RuntimeCreatedListingTitleKey = "RuntimeCreatedListingTitle";

    private readonly ScenarioContext _scenarioContext;
    private readonly UiTestContext _uiContext;
    private readonly SearchSkillPage _searchSkillPage;
    private readonly ShareSkillPage _shareSkillPage;
    private readonly ListingManagementPage _listingManagementPage;

    public SearchSkillSteps(ScenarioContext scenarioContext)
    {
        _scenarioContext = scenarioContext;
        _uiContext = scenarioContext.Get<UiTestContext>();
        _searchSkillPage = new SearchSkillPage(_uiContext);
        _shareSkillPage = new ShareSkillPage(_uiContext);
        _listingManagementPage = new ListingManagementPage(_uiContext);
    }

    [Given("I have a searchable service listing using {string}")]
    public void GivenIHaveASearchableServiceListingUsing(string dataKey)
    {
        var source = _uiContext.TestData.ReadRequired<ShareSkillData>("TestData/shareskill.json", dataKey);
        var data = RuntimeDataFactory.CreateUniqueShareSkill(source);

        _shareSkillPage.Open();
        _shareSkillPage.CreateListing(data);

        _listingManagementPage.Open();
        var listingAfterSave = _listingManagementPage.GetTopListingTitle();

        _scenarioContext.Set(data, RuntimeShareSkillDataKey);
        _scenarioContext.Set(listingAfterSave, RuntimeCreatedListingTitleKey);
        _scenarioContext.Set(new SearchSkillData
        {
            SearchTerm = data.Title,
            ExpectedResultText = data.Title
        }, RuntimeSearchSkillDataKey);
    }

    [When("I open the search skill page")]
    public void WhenIOpenTheSearchSkillPage()
    {
        _searchSkillPage.Open();
    }

    [When("I search for a skill using {string}")]
    public void WhenISearchForASkillUsing(string dataKey)
    {
        var data = ResolveRuntimeOrStaticSearchData(dataKey);
        _searchSkillPage.SearchFor(data);
        _scenarioContext.Set(data, RuntimeSearchSkillDataKey);
    }

    [Then("search results for {string} should be displayed")]
    public void ThenSearchResultsForShouldBeDisplayed(string dataKey)
    {
        var data = ResolveRuntimeOrStaticSearchData(dataKey);
        var expectedText = string.IsNullOrWhiteSpace(data.ExpectedResultText) ? data.SearchTerm : data.ExpectedResultText;
        var exists = _searchSkillPage.HasResultsFor(expectedText);
        Assert.That(exists, Is.True, "Expected search results to include the requested skill.");
    }

    private SearchSkillData ResolveRuntimeOrStaticSearchData(string dataKey)
    {
        if (_scenarioContext.TryGetValue<SearchSkillData>(RuntimeSearchSkillDataKey, out var runtimeSearch) && runtimeSearch is not null)
        {
            return runtimeSearch;
        }

        if (_scenarioContext.TryGetValue<string>(RuntimeCreatedListingTitleKey, out var runtimeListingTitle) &&
            !string.IsNullOrWhiteSpace(runtimeListingTitle))
        {
            return new SearchSkillData
            {
                SearchTerm = runtimeListingTitle,
                ExpectedResultText = runtimeListingTitle
            };
        }

        if (_scenarioContext.TryGetValue<ShareSkillData>(RuntimeShareSkillDataKey, out var runtimeShare) && runtimeShare is not null)
        {
            return new SearchSkillData
            {
                SearchTerm = runtimeShare.Title,
                ExpectedResultText = runtimeShare.Title
            };
        }

        return _uiContext.TestData.ReadRequired<SearchSkillData>("TestData/searchskill.json", dataKey);
    }
}
