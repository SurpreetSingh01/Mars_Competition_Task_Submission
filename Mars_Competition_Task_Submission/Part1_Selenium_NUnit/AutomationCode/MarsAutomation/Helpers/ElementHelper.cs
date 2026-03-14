using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace MarsAutomation.Helpers;

public static class ElementHelper
{
    public static bool IsPresent(IWebDriver driver, By locator, int timeoutSeconds = 0)
    {
        if (driver == null) throw new ArgumentNullException(nameof(driver));

        if (timeoutSeconds <= 0)
        {
            return driver.FindElements(locator).Any();
        }

        try
        {
            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(timeoutSeconds));
            return wait.Until(d => d.FindElements(locator).Any());
        }
        catch (WebDriverTimeoutException)
        {
            return false;
        }
    }
}

