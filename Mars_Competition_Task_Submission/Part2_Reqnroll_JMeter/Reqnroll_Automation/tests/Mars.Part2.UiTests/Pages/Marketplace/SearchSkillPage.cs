using Mars.Part2.UiTests.Components.Marketplace;
using Mars.Part2.UiTests.Models;
using Mars.Part2.UiTests.Pages.Base;
using OpenQA.Selenium;

namespace Mars.Part2.UiTests.Pages.Marketplace;

public sealed class SearchSkillPage : BasePage
{
    private static readonly By[] SearchInputLocators =
    {
        By.CssSelector(".ui.small.icon.input.search-box input[placeholder='Search skills']"),
        By.CssSelector(".search-box input"),
        By.CssSelector("input[placeholder='Search skills']"),
        By.Name("searchString"),
        By.Name("search")
    };

    private static readonly By[] SearchTriggerLocators =
    {
        By.CssSelector(".search-box i.search.link.icon"),
        By.CssSelector("i.search.link.icon"),
        By.CssSelector("button[type='submit']")
    };

    public SearchSkillPage(UiTestContext context) : base(context)
    {
        Results = new SearchResultsComponent(context);
    }

    public SearchResultsComponent Results { get; }

    public void Open()
    {
        NavigateTo(Context.Routes.SearchSkill);
    }

    public override void WaitUntilReady()
    {
        base.WaitUntilReady();
        Element.FindVisible("search input", SearchInputLocators);
    }

    public void SearchFor(SearchSkillData data)
    {
        var term = data.SearchTerm ?? string.Empty;

        WaitUntilReady();
        Element.EnterAndSubmit("search skill input", term, SearchInputLocators);

        if (!Wait.TryWaitForCondition(driver => driver.Url.Contains("searchString=", StringComparison.OrdinalIgnoreCase), 5) &&
            Element.ExistsAny(SearchTriggerLocators))
        {
            Element.Click("search trigger", SearchTriggerLocators);
        }

        Wait.TryWaitForCondition(driver =>
            driver.Url.Contains("searchString=", StringComparison.OrdinalIgnoreCase) || Results.HasAnyResults(), 8);
    }

    public bool HasResultsFor(string expectedText)
    {
        Wait.WaitForCondition(_ => Results.HasAnyResults(), "search result cards to appear", 10);
        return Results.AnyResultContains(expectedText);
    }
}
