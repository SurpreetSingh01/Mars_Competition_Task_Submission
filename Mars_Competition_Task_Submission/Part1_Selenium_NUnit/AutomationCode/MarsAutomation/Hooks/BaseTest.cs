using AventStack.ExtentReports;
using MarsAutomation.Drivers;
using MarsAutomation.Helpers;
using MarsAutomation.Pages;
using MarsAutomation.Reports;
using NUnit.Framework;
using OpenQA.Selenium;

namespace MarsAutomation.Hooks;

public class BaseTest
{
    protected IWebDriver Driver = null!;
    protected ExtentTest CurrentTest = null!;
    private ExtentReports? _extent;
    protected virtual bool RequiresAuthenticatedSession => true;

    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        _extent = ExtentReportManager.Instance;

        Driver = WebDriverFactory.Create("Chrome", headless: false);

        try
        {
            Driver.Manage().Window.Maximize();
        }
        catch (WebDriverException ex)
        {
            Console.WriteLine($"[WARN] Could not maximize browser: {ex.Message}");
        }

        Driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(2);

        var baseUrl = JsonReader.GetString("login", "baseUrl");
        Driver.Navigate().GoToUrl(baseUrl);
    }

    [SetUp]
    public void Setup()
    {
        _extent ??= ExtentReportManager.Instance;
        CurrentTest = _extent.CreateTest(TestContext.CurrentContext.Test.Name);

        var baseUrl = JsonReader.GetString("login", "baseUrl");
        var profileUrl = baseUrl.TrimEnd('/') + "/Account/Profile";

        var loginPage = new LoginPage(Driver);
        var username = JsonReader.GetString("login", "username");
        var password = JsonReader.GetString("login", "password");

        if (RequiresAuthenticatedSession)
        {
            loginPage.LoginIfNeeded(username, password);
            Driver.Navigate().GoToUrl(profileUrl);
            return;
        }

        loginPage.LogoutIfNeeded();
        Driver.Navigate().GoToUrl(baseUrl);
    }

    [TearDown]
    public void TearDown()
    {
        var result = TestContext.CurrentContext.Result.Outcome.Status;
        var message = TestContext.CurrentContext.Result.Message;

        if (CurrentTest is null)
        {
            return;
        }

        if (result == NUnit.Framework.Interfaces.TestStatus.Failed)
        {
            if (Driver is not null)
            {
                var screenshotPath = ScreenshotHelper.CaptureScreenshot(Driver, TestContext.CurrentContext.Test.Name);
                CurrentTest.Fail(message ?? "Test failed")
                           .AddScreenCaptureFromPath(screenshotPath);
            }
            else
            {
                CurrentTest.Fail(message ?? "Test failed");
            }
        }
        else
        {
            CurrentTest.Pass("Test passed");
        }
    }

    [OneTimeTearDown]
    public void OneTimeTeardown()
    {
        _extent?.Flush();
        Driver?.Quit();
        Driver?.Dispose();
    }
}
