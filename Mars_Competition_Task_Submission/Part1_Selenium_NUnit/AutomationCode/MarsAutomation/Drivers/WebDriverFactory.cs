using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Edge;

namespace MarsAutomation.Drivers;

public static class WebDriverFactory
{
    public static IWebDriver Create(string browser = "Chrome", bool headless = false)
    {
        browser = browser?.Trim() ?? "Chrome";

        try
        {
            return browser.Equals("Edge", StringComparison.OrdinalIgnoreCase)
                ? CreateEdge(headless)
                : CreateChrome(headless);
        }
        catch (WebDriverException)
        {
            // Fallback between Chrome and Edge if the requested one fails to start.
            return browser.Equals("Edge", StringComparison.OrdinalIgnoreCase)
                ? CreateChrome(headless)
                : CreateEdge(headless);
        }
    }

    private static IWebDriver CreateChrome(bool headless)
    {
        var options = new ChromeOptions();
        AddCommonChromiumArgs(options);
        if (headless)
        {
            options.AddArgument("--headless=new");
        }

        return new ChromeDriver(options);
    }

    private static IWebDriver CreateEdge(bool headless)
    {
        var options = new EdgeOptions();
        AddCommonChromiumArgs(options);
        if (headless)
        {
            options.AddArgument("--headless=new");
        }

        return new EdgeDriver(options);
    }

    private static void AddCommonChromiumArgs(dynamic options)
    {
        options.AddArgument("--no-sandbox");
        options.AddArgument("--disable-dev-shm-usage");
        options.AddArgument("--disable-gpu");
        options.AddArgument("--window-size=1920,1080");
        options.AddArgument("--remote-allow-origins=*");
        options.AddArgument("--disable-notifications");
        options.AddArgument("--disable-extensions");
    }
}

