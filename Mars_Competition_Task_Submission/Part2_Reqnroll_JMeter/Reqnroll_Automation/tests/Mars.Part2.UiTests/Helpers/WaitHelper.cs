using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace Mars.Part2.UiTests.Helpers;

public sealed class WaitHelper
{
    private readonly IWebDriver _driver;
    private readonly int _defaultTimeoutSeconds;

    public WaitHelper(IWebDriver driver, int defaultTimeoutSeconds)
    {
        _driver = driver;
        _defaultTimeoutSeconds = defaultTimeoutSeconds;
    }

    public IWebElement WaitForElementVisible(By locator, string description, int? timeoutSeconds = null)
    {
        try
        {
            return CreateWait(timeoutSeconds).Until(driver =>
            {
                var element = driver.FindElement(locator);
                return element.Displayed ? element : null;
            }) ?? throw BuildTimeout(description, locator);
        }
        catch (WebDriverTimeoutException ex)
        {
            throw new WebDriverTimeoutException(BuildMessage("visible", description, locator), ex);
        }
    }

    public bool TryWaitForElementVisible(By locator, out IWebElement? element, int? timeoutSeconds = null)
    {
        try
        {
            element = CreateWait(timeoutSeconds).Until(driver =>
            {
                var item = driver.FindElement(locator);
                return item.Displayed ? item : null;
            });

            return element is not null;
        }
        catch (WebDriverTimeoutException)
        {
            element = null;
            return false;
        }
    }

    public IWebElement WaitForElementClickable(By locator, string description, int? timeoutSeconds = null)
    {
        try
        {
            return CreateWait(timeoutSeconds).Until(driver =>
            {
                var element = driver.FindElement(locator);
                return element.Displayed && element.Enabled ? element : null;
            }) ?? throw BuildTimeout(description, locator);
        }
        catch (WebDriverTimeoutException ex)
        {
            throw new WebDriverTimeoutException(BuildMessage("clickable", description, locator), ex);
        }
    }

    public void WaitForElementInvisible(By locator, string description, int? timeoutSeconds = null)
    {
        try
        {
            CreateWait(timeoutSeconds).Until(driver =>
            {
                var elements = driver.FindElements(locator);
                return elements.Count == 0 || elements.All(element => !element.Displayed);
            });
        }
        catch (WebDriverTimeoutException ex)
        {
            throw new WebDriverTimeoutException(BuildMessage("invisible", description, locator), ex);
        }
    }

    public void WaitForUrlContains(string urlFragment, int? timeoutSeconds = null)
    {
        try
        {
            CreateWait(timeoutSeconds).Until(driver =>
                driver.Url.Contains(urlFragment, StringComparison.OrdinalIgnoreCase));
        }
        catch (WebDriverTimeoutException ex)
        {
            throw new WebDriverTimeoutException(
                $"Timed out waiting for URL to contain '{urlFragment}'. Current URL: '{_driver.Url}'.",
                ex);
        }
    }

    public void WaitForCondition(Func<IWebDriver, bool> condition, string description, int? timeoutSeconds = null)
    {
        try
        {
            CreateWait(timeoutSeconds).Until(driver => condition(driver));
        }
        catch (WebDriverTimeoutException ex)
        {
            throw new WebDriverTimeoutException(
                $"Timed out waiting for condition: {description}. Current URL: '{_driver.Url}'.",
                ex);
        }
    }

    public bool TryWaitForCondition(Func<IWebDriver, bool> condition, int? timeoutSeconds = null)
    {
        try
        {
            CreateWait(timeoutSeconds).Until(driver => condition(driver));
            return true;
        }
        catch (WebDriverTimeoutException)
        {
            return false;
        }
    }

    public void WaitForDocumentReady(int? timeoutSeconds = null)
    {
        WaitForCondition(driver =>
        {
            if (driver is not IJavaScriptExecutor javascript)
            {
                return true;
            }

            var readyState = javascript.ExecuteScript("return document.readyState")?.ToString();
            return string.Equals(readyState, "complete", StringComparison.OrdinalIgnoreCase);
        }, "document.readyState == complete", timeoutSeconds);
    }

    private WebDriverWait CreateWait(int? timeoutSeconds)
    {
        var wait = new WebDriverWait(new SystemClock(), _driver, TimeSpan.FromSeconds(timeoutSeconds ?? _defaultTimeoutSeconds), TimeSpan.FromMilliseconds(250));
        wait.IgnoreExceptionTypes(typeof(NoSuchElementException), typeof(StaleElementReferenceException));
        return wait;
    }

    private WebDriverTimeoutException BuildTimeout(string description, By locator)
    {
        return new(BuildMessage("available", description, locator));
    }

    private string BuildMessage(string expectedState, string description, By locator)
    {
        return $"Timed out waiting for {description} to become {expectedState}. Locator: '{locator}'. Current URL: '{_driver.Url}'.";
    }
}
