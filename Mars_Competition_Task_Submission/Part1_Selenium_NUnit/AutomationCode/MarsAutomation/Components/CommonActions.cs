using MarsAutomation.Helpers;
using OpenQA.Selenium;

namespace MarsAutomation.Components;

public class CommonActions
{
    private readonly IWebDriver _driver;

    public CommonActions(IWebDriver driver)
    {
        _driver = driver;
    }

    public void Click(By locator)
    {
        // Wait for target to be clickable.
        var element = WaitHelper.WaitForElementClickable(_driver, locator, 10);

        try
        {
            element.Click();
        }
        catch (ElementClickInterceptedException)
        {
            // If something is overlaid (e.g. login modal), force the click via JavaScript.
            ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].click();", element);
        }
    }

    public void Type(By locator, string text)
    {
        var el = WaitHelper.WaitForElementVisible(_driver, locator);
        el.Clear();
        el.SendKeys(text);
    }

    public string GetText(By locator) =>
        WaitHelper.WaitForElementVisible(_driver, locator).Text;
}

