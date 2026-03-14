using OpenQA.Selenium;

namespace Mars.Part2.UiTests.Helpers;

public sealed class ScreenshotHelper
{
    private readonly IWebDriver _driver;
    private readonly string _screenshotDirectory;

    public ScreenshotHelper(IWebDriver driver, string screenshotDirectory)
    {
        _driver = driver;
        _screenshotDirectory = PathHelper.ResolveProjectPath(screenshotDirectory);
        Directory.CreateDirectory(_screenshotDirectory);
    }

    public string Save(string scenarioName)
    {
        if (_driver is not ITakesScreenshot screenshotDriver)
        {
            return string.Empty;
        }

        var fileName = $"{Sanitize(scenarioName)}_{DateTime.Now:yyyyMMdd_HHmmss}.png";
        var fullPath = Path.Combine(_screenshotDirectory, fileName);
        screenshotDriver.GetScreenshot().SaveAsFile(fullPath);
        return fullPath;
    }

    private static string Sanitize(string name)
    {
        foreach (var invalidChar in Path.GetInvalidFileNameChars())
        {
            name = name.Replace(invalidChar, '_');
        }

        return name.Replace(' ', '_');
    }
}
