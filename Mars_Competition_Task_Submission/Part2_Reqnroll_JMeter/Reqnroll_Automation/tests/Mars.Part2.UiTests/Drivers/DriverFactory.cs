using Mars.Part2.UiTests.Config;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Edge;

namespace Mars.Part2.UiTests.Drivers;

public static class DriverFactory
{
    public static IWebDriver Create(TestSettings settings)
    {
        var browserName = settings.Browser.Name.Trim().ToLowerInvariant();

        IWebDriver driver = browserName switch
        {
            "edge" => new EdgeDriver(BuildEdgeOptions(settings)),
            _ => new ChromeDriver(BuildChromeOptions(settings))
        };

        driver.Manage().Timeouts().ImplicitWait = TimeSpan.Zero;
        driver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(settings.Timeouts.PageLoadSeconds);
        driver.Manage().Timeouts().AsynchronousJavaScript = TimeSpan.FromSeconds(settings.Timeouts.ScriptSeconds);

        if (settings.Browser.Maximize)
        {
            driver.Manage().Window.Maximize();
        }

        return driver;
    }

    private static ChromeOptions BuildChromeOptions(TestSettings settings)
    {
        var options = new ChromeOptions();
        options.AddArgument("--disable-notifications");
        options.AddArgument("--disable-infobars");
        options.AddArgument("--disable-popup-blocking");

        if (settings.Browser.Headless)
        {
            options.AddArgument("--headless=new");
            options.AddArgument("--window-size=1920,1080");
        }

        return options;
    }

    private static EdgeOptions BuildEdgeOptions(TestSettings settings)
    {
        var options = new EdgeOptions();
        options.AddArgument("--disable-notifications");
        options.AddArgument("--disable-popup-blocking");

        if (settings.Browser.Headless)
        {
            options.AddArgument("--headless=new");
            options.AddArgument("--window-size=1920,1080");
        }

        return options;
    }
}
