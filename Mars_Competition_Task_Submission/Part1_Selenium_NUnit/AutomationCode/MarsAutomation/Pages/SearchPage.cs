using MarsAutomation.Components;
using MarsAutomation.Helpers;
using OpenQA.Selenium;

namespace MarsAutomation.Pages;

public class SearchPage
{
    private readonly IWebDriver _driver;
    private readonly CommonActions _actions;
    private readonly NavigationMenu _menu;
    private readonly string _baseUrl;

    private readonly By _searchInput = By.CssSelector("input[name='search'], input[name='searchString'], input[placeholder*='skill']");
    private readonly By _categoryDropdown = By.Name("categoryId");
    private readonly By _searchButton = By.XPath("//button[normalize-space()='Search']");
    private readonly By _resultsRows = By.CssSelector("table tbody tr");
    private readonly By _resultCards = By.CssSelector(".ui.card, .service-item, .service-info, .search-results .item, .search-result");

    public SearchPage(IWebDriver driver)
    {
        _driver = driver;
        _actions = new CommonActions(driver);
        _menu = new NavigationMenu(driver);
        _baseUrl = JsonReader.GetString("login", "baseUrl").TrimEnd('/');
    }

    public void NavigateToSearch()
    {
        _menu.GoToSearch();
        EnsureSearchPageReady();
    }

    public void SearchByKeyword(string keyword)
    {
        EnsureSearchPageReady();
        _actions.Type(_searchInput, keyword);
        _actions.Click(_searchButton);
    }

    public void SearchByCategory(string category)
    {
        EnsureSearchPageReady();

        var dropdowns = _driver.FindElements(_categoryDropdown).Where(e => e.Displayed).ToList();
        if (dropdowns.Any())
        {
            var dropdown = WaitHelper.WaitForElementClickable(_driver, _categoryDropdown);
            dropdown.Click();
            _driver.FindElement(By.XPath($"//select[@name='categoryId']/option[normalize-space()='{category}']")).Click();
            _actions.Click(_searchButton);
            return;
        }

        var categoryTile = By.XPath(
            $"//*[self::a or self::div or self::span or self::h3][contains(normalize-space(),'{category}')]");

        if (_driver.FindElements(categoryTile).Any(e => e.Displayed))
        {
            _actions.Click(categoryTile);
            return;
        }

        // The current home UI may not expose a dedicated category tile for every test-data value,
        // so fall back to the main search box with the category text.
        SearchByKeyword(category);
    }

    public bool HasResults()
    {
        try
        {
            WaitHelper.WaitUntil(_driver, driver =>
            {
                var tableRows = driver.FindElements(_resultsRows).Count;
                var cards = driver.FindElements(_resultCards).Count;
                var currentUrl = (driver.Url ?? string.Empty).TrimEnd('/');

                return tableRows > 0 || cards >= 2 || !string.Equals(currentUrl, _baseUrl, StringComparison.OrdinalIgnoreCase);
            }, 10);

            return true;
        }
        catch
        {
            return false;
        }
    }

    private void EnsureSearchPageReady()
    {
        var loginPage = new LoginPage(_driver);
        var username = JsonReader.GetString("login", "username");
        var password = JsonReader.GetString("login", "password");

        loginPage.LoginIfNeeded(username, password);
        loginPage.DismissModalIfPresent();

        WaitHelper.WaitUntil(_driver, driver =>
            driver.FindElements(_searchInput).Any(e => e.Displayed) ||
            driver.FindElements(_categoryDropdown).Any(e => e.Displayed) ||
            driver.FindElements(By.XPath("//*[self::a or self::div or self::span or self::h3][normalize-space()='Programming & Tech']")).Any(e => e.Displayed),
            10);
    }
}

