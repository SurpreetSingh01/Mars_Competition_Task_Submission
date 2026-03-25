using AventStack.ExtentReports;
using AventStack.ExtentReports.Reporter;

namespace MarsAutomation.Reports;

public static class ExtentReportManager
{
    private static readonly Lazy<ExtentReports> LazyExtent = new(CreateInstance);

    public static ExtentReports Instance => LazyExtent.Value;

    private static ExtentReports CreateInstance()
    {
        var reportsRoot = Path.Combine(AppContext.BaseDirectory, "Reports");
        Directory.CreateDirectory(reportsRoot);

        var reportPath = Path.Combine(reportsRoot, $"MarsAutomation_{DateTime.Now:yyyyMMdd_HHmmss}.html");
        var reporter = new ExtentSparkReporter(reportPath);

        var extent = new ExtentReports();
        extent.AttachReporter(reporter);
        extent.AddSystemInfo("Framework", ".NET / NUnit / Selenium");

        return extent;
    }
}

