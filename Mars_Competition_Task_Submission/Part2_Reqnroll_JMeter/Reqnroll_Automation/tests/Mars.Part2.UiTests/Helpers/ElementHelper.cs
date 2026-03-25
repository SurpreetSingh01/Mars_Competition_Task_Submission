using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;

namespace Mars.Part2.UiTests.Helpers;

public sealed class ElementHelper
{
    private readonly IWebDriver _driver;
    private readonly WaitHelper _wait;

    public ElementHelper(IWebDriver driver, WaitHelper wait)
    {
        _driver = driver;
        _wait = wait;
    }

    public bool Exists(By locator)
    {
        try
        {
            return _driver.FindElements(locator).Any();
        }
        catch (WebDriverException)
        {
            return false;
        }
    }

    public bool ExistsAny(params By[] locators) => locators.Any(Exists);

    public IWebElement FindVisible(string description, params By[] locators)
    {
        foreach (var locator in locators)
        {
            if (_wait.TryWaitForElementVisible(locator, out var element, 3) && element is not null)
            {
                return element;
            }
        }

        throw new InvalidOperationException(
            $"Unable to find a visible element for '{description}'. Tried locators: {string.Join(" | ", locators.Select(locator => locator.ToString()))}. Current URL: '{_driver.Url}'.");
    }

    public IWebElement FindClickable(string description, params By[] locators)
    {
        foreach (var locator in locators)
        {
            try
            {
                return _wait.WaitForElementClickable(locator, description, 3);
            }
            catch (WebDriverTimeoutException)
            {
            }
        }

        throw new InvalidOperationException(
            $"Unable to find a clickable element for '{description}'. Tried locators: {string.Join(" | ", locators.Select(locator => locator.ToString()))}. Current URL: '{_driver.Url}'.");
    }

    public IReadOnlyList<IWebElement> FindAllVisible(params By[] locators)
    {
        foreach (var locator in locators)
        {
            var elements = _driver.FindElements(locator)
                .Where(element => element.Displayed)
                .ToList();

            if (elements.Count > 0)
            {
                return elements;
            }
        }

        return Array.Empty<IWebElement>();
    }

    public void Click(string description, params By[] locators)
    {
        var element = FindClickable(description, locators);
        ScrollIntoView(element);

        try
        {
            element.Click();
        }
        catch (ElementClickInterceptedException)
        {
            ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].click();", element);
        }
    }

    public void EnterText(string description, string value, params By[] locators)
    {
        PerformInput(description, locators, element =>
        {
            element.Clear();
            element.SendKeys(value);
        });
    }

    public void EnterAndSubmit(string description, string value, params By[] locators)
    {
        PerformInput(description, locators, element =>
        {
            element.Clear();
            element.SendKeys(value);
            element.SendKeys(Keys.Enter);
        });
    }

    public void SelectDropdownByText(string description, string value, params By[] locators)
    {
        var element = FindVisible(description, locators);
        ScrollIntoView(element);

        var select = new SelectElement(element);
        try
        {
            select.SelectByText(value);
        }
        catch (NoSuchElementException)
        {
            var option = select.Options.FirstOrDefault(item =>
                string.Equals(item.Text.Trim(), value.Trim(), StringComparison.OrdinalIgnoreCase) ||
                item.Text.Contains(value, StringComparison.OrdinalIgnoreCase));

            if (option is null)
            {
                var availableOptions = string.Join(", ",
                    select.Options
                        .Select(item => item.Text.Trim())
                        .Where(text => !string.IsNullOrWhiteSpace(text)));

                throw new NoSuchElementException(
                    $"Dropdown option '{value}' was not found for '{description}'. Available options: {availableOptions}.", null);
            }

            option.Click();
        }
    }

    public void ClickLabelByText(string labelText)
    {
        var locator = By.XPath($"//label[contains(normalize-space(.),'{labelText}')]");
        Click(labelText, locator);
    }

    public void AddTag(string description, string tagValue, params By[] locators)
    {
        var element = FindVisible(description, locators);
        ScrollIntoView(element);
        element.SendKeys(tagValue);
        element.SendKeys(Keys.Enter);
    }

    public void ScrollIntoView(IWebElement element)
    {
        ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].scrollIntoView({ block: 'center' });", element);
    }

    private void PerformInput(string description, By[] locators, Action<IWebElement> action)
    {
        const int maxAttempts = 3;

        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                var element = FindVisible(description, locators);
                ScrollIntoView(element);
                action(element);
                return;
            }
            catch (StaleElementReferenceException) when (attempt < maxAttempts)
            {
            }
        }

        throw new InvalidOperationException(
            $"Input interaction for '{description}' became stale after {maxAttempts} attempts. Current URL: '{_driver.Url}'.");
    }
}
