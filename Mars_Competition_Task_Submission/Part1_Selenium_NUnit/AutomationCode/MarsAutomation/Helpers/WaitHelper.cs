using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace MarsAutomation.Helpers;

public static class WaitHelper
{
    public static IWebElement WaitForElementVisible(
        IWebDriver driver,
        By locator,
        int timeoutSeconds = 10)
    {
        if (driver == null) throw new ArgumentNullException(nameof(driver));

        var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(timeoutSeconds));
        return wait.Until(d =>
        {
            var el = d.FindElement(locator);
            return el.Displayed ? el : null!;
        });
    }

    public static IWebElement WaitForElementClickable(
        IWebDriver driver,
        By locator,
        int timeoutSeconds = 10)
    {
        if (driver == null) throw new ArgumentNullException(nameof(driver));

        var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(timeoutSeconds));
        return wait.Until(d =>
        {
            var el = d.FindElement(locator);
            return (el.Displayed && el.Enabled) ? el : null!;
        });
    }

    public static void WaitUntil(
        IWebDriver driver,
        Func<IWebDriver, bool> condition,
        int timeoutSeconds = 10)
    {
        if (driver == null) throw new ArgumentNullException(nameof(driver));
        if (condition == null) throw new ArgumentNullException(nameof(condition));

        var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(timeoutSeconds));
        wait.Until(condition);
    }

    public static bool WaitForElementInvisible(
        IWebDriver driver,
        By locator,
        int timeoutSeconds = 10)
    {
        if (driver == null) throw new ArgumentNullException(nameof(driver));

        var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(timeoutSeconds));
        return wait.Until(d =>
        {
            var elements = d.FindElements(locator);
            return elements.Count == 0 || elements.All(e => !e.Displayed);
        });
    }

    public static void WaitForUrlContains(
        IWebDriver driver,
        string partialUrl,
        int timeoutSeconds = 10)
    {
        if (driver == null) throw new ArgumentNullException(nameof(driver));
        if (string.IsNullOrWhiteSpace(partialUrl)) throw new ArgumentException("Value cannot be null or empty.", nameof(partialUrl));

        var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(timeoutSeconds));
        wait.Until(d => (d.Url ?? string.Empty).Contains(partialUrl, StringComparison.OrdinalIgnoreCase));
    }
}

