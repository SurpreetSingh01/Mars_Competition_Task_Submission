using System.Text.RegularExpressions;
using Mars.Part2.UiTests.Models;
using Mars.Part2.UiTests.Pages.Base;
using OpenQA.Selenium;

namespace Mars.Part2.UiTests.Pages.Marketplace;

public sealed class ListingManagementPage : BasePage
{
    private static readonly By[] HeadingLocators =
    {
        By.XPath("//h1[contains(normalize-space(.),'Manage Listings')]"),
        By.XPath("//*[self::h1 or self::h2 or self::div][contains(normalize-space(.),'Manage Listings')]")
    };

    private static readonly By[] RowLocators =
    {
        By.CssSelector("table tbody tr"),
        By.XPath("//table/tbody/tr")
    };

    private static readonly Regex ListingTitlePattern = new(@"Listing\s+\d+", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public ListingManagementPage(UiTestContext context) : base(context)
    {
    }

    public void Open()
    {
        NavigateTo(Context.Routes.ListingManagement);
    }

    public override void WaitUntilReady()
    {
        base.WaitUntilReady();
        Wait.WaitForCondition(_ => Element.ExistsAny(HeadingLocators) || Element.FindAllVisible(RowLocators).Count > 0,
            "listing management content",
            10);
    }

    public string GetTopListingTitle()
    {
        WaitUntilReady();

        var title = Element.FindAllVisible(RowLocators)
            .Select(ExtractTitle)
            .FirstOrDefault(item => !string.IsNullOrWhiteSpace(item));

        if (string.IsNullOrWhiteSpace(title))
        {
            throw new InvalidOperationException(
                $"No non-empty listing title was visible on Listing Management. {Context.DebugState.BuildPageStateSummary()}");
        }

        return title;
    }

    public bool HasListing(string title)
    {
        WaitUntilReady();
        return Element.FindAllVisible(RowLocators)
            .Any(row => ExtractTitle(row).Contains(title, StringComparison.OrdinalIgnoreCase) ||
                        row.Text.Contains(title, StringComparison.OrdinalIgnoreCase));
    }

    private static string ExtractTitle(IWebElement row)
    {
        var rowText = row.Text.Trim();
        var rowMatch = ListingTitlePattern.Match(rowText);
        if (rowMatch.Success)
        {
            return rowMatch.Value;
        }

        foreach (var cellText in row.FindElements(By.TagName("td"))
                     .Where(cell => cell.Displayed)
                     .Select(cell => cell.Text.Trim())
                     .Where(text => !string.IsNullOrWhiteSpace(text)))
        {
            var cellMatch = ListingTitlePattern.Match(cellText);
            if (cellMatch.Success)
            {
                return cellMatch.Value;
            }
        }

        return rowText;
    }
}
