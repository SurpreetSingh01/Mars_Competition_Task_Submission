using OpenQA.Selenium;

namespace Mars.Part2.UiTests.Components.Common;

public sealed class LoadingOverlayComponent : BaseComponent
{
    private static readonly By[] OverlayLocators =
    {
        By.Id("overlay"),
        By.CssSelector(".ui.active.dimmer"),
        By.CssSelector(".ui.loader.active"),
        By.CssSelector(".loading.icon")
    };

    public LoadingOverlayComponent(Models.UiTestContext context) : base(context)
    {
    }

    public void WaitUntilHidden()
    {
        foreach (var locator in OverlayLocators.Where(Element.Exists))
        {
            try
            {
                Wait.WaitForElementInvisible(locator, "loading overlay", 5);
            }
            catch (WebDriverTimeoutException)
            {
            }
        }
    }
}
