using AventStack.ExtentReports;
using MarsAutomation.Reports;
using NUnit.Framework;

namespace MarsAutomation.Hooks;

[SetUpFixture]
public class TestSetup
{
    public static ExtentReports? Extent { get; private set; }

    [OneTimeSetUp]
    public void GlobalSetup()
    {
        Extent = ExtentReportManager.Instance;
    }

    [OneTimeTearDown]
    public void GlobalTeardown()
    {
        Extent?.Flush();
    }
}
