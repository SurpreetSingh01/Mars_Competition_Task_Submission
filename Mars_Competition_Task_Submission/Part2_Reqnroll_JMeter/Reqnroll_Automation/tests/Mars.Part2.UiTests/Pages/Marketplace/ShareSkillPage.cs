using Mars.Part2.UiTests.Components.Marketplace;
using Mars.Part2.UiTests.Models;
using Mars.Part2.UiTests.Pages.Base;
using OpenQA.Selenium;

namespace Mars.Part2.UiTests.Pages.Marketplace;

public sealed class ShareSkillPage : BasePage
{
    private static readonly By[] FormReadyLocators =
    {
        By.CssSelector("input[name='title']"),
        By.Name("title"),
        By.CssSelector("input[placeholder='Write a title to describe the service you provide.']"),
        By.CssSelector("textarea[name='description']"),
        By.Name("description")
    };

    private static readonly By[] ListingRowLocators =
    {
        By.CssSelector("table tbody tr"),
        By.XPath("//table/tbody/tr")
    };

    public ShareSkillPage(UiTestContext context) : base(context)
    {
        Form = new ShareSkillFormComponent(context);
    }

    public ShareSkillFormComponent Form { get; }

    public void Open()
    {
        NavigateTo(Context.Routes.ShareSkill);
    }

    public override void WaitUntilReady()
    {
        base.WaitUntilReady();
        Element.FindVisible("share skill form", FormReadyLocators);
    }

    public void CreateListing(ShareSkillData data)
    {
        Form.Fill(data);
        Form.Submit();
    }

    public bool IsListingVisible(string title)
    {
        NavigateTo(Context.Routes.ListingManagement);
        var rows = Element.FindAllVisible(ListingRowLocators);
        return rows.Any(row => row.Text.Contains(title, StringComparison.OrdinalIgnoreCase));
    }
}
