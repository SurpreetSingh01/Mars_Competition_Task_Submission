using OpenQA.Selenium;

namespace Mars.Part2.UiTests.Helpers;

public sealed class DebugStateHelper
{
    private readonly IWebDriver _driver;
    private readonly string _logDirectory;

    public DebugStateHelper(IWebDriver driver, string logDirectory)
    {
        _driver = driver;
        _logDirectory = PathHelper.ResolveProjectPath(logDirectory);
        Directory.CreateDirectory(_logDirectory);
    }

    public string Save(string scenarioName)
    {
        var fileName = $"{Sanitize(scenarioName)}_{DateTime.Now:yyyyMMdd_HHmmss}.log";
        var fullPath = Path.Combine(_logDirectory, fileName);
        File.WriteAllText(fullPath, BuildPageStateSummary());
        return fullPath;
    }

    public string BuildPageStateSummary()
    {
        string readyState = "unknown";
        string bodySnippet = string.Empty;

        if (_driver is IJavaScriptExecutor javascript)
        {
            readyState = javascript.ExecuteScript("return document.readyState")?.ToString() ?? "unknown";
            bodySnippet = javascript.ExecuteScript("return document.body ? document.body.innerText.slice(0, 500) : '';")?.ToString() ?? string.Empty;
        }

        return $"""
            Timestamp: {DateTime.Now:O}
            Current URL: {_driver.Url}
            Page Title: {_driver.Title}
            Document Ready State: {readyState}
            Body Snippet:
            {bodySnippet}
            """;
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
