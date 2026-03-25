using OpenQA.Selenium;

namespace MarsAutomation.Helpers;

public static class ScreenshotHelper
{
    public static string CaptureScreenshot(IWebDriver driver, string testName)
    {
        var screenshotsRoot = Path.Combine(AppContext.BaseDirectory, "Screenshots");
        Directory.CreateDirectory(screenshotsRoot);

        var fileNameSafe = string.Concat(testName.Split(Path.GetInvalidFileNameChars()));
        var fileName = $"{fileNameSafe}_{DateTime.Now:yyyyMMdd_HHmmssfff}.png";
        var fullPath = Path.Combine(screenshotsRoot, fileName);

        var screenshot = ((ITakesScreenshot)driver).GetScreenshot();
        screenshot.SaveAsFile(fullPath);

        return fullPath;
    }
}

