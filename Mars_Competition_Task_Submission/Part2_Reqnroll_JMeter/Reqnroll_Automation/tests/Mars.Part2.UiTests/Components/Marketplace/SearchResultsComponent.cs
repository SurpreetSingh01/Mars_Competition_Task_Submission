using Mars.Part2.UiTests.Components.Common;
using Mars.Part2.UiTests.Models;
using OpenQA.Selenium;

namespace Mars.Part2.UiTests.Components.Marketplace;

public sealed class SearchResultsComponent : BaseComponent
{
    private static readonly By[] ResultLocators =
    {
        By.CssSelector(".ui.cards .card"),
        By.CssSelector(".ui.card"),
        By.XPath("//table/tbody/tr")
    };

    public SearchResultsComponent(UiTestContext context) : base(context)
    {
    }

    public bool HasAnyResults()
    {
        return Element.FindAllVisible(ResultLocators).Count > 0;
    }

    public bool AnyResultContains(string text)
    {
        return Element.FindAllVisible(ResultLocators)
            .Any(result => result.Text.Contains(text, StringComparison.OrdinalIgnoreCase));
    }
}
