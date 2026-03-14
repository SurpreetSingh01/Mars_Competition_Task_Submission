namespace Mars.Part2.UiTests.Config;

public sealed class TestSettings
{
    public ApplicationSettings Application { get; set; } = new();
    public BrowserSettings Browser { get; set; } = new();
    public TimeoutSettings Timeouts { get; set; } = new();
    public ArtifactSettings Artifacts { get; set; } = new();
    public AuthenticationSettings Authentication { get; set; } = new();
    public RouteSettings Routes { get; set; } = new();
}

public sealed class ApplicationSettings
{
    public string BaseUrl { get; set; } = "http://localhost:5003";
}

public sealed class BrowserSettings
{
    public string Name { get; set; } = "chrome";
    public bool Headless { get; set; }
    public bool Maximize { get; set; } = true;
}

public sealed class TimeoutSettings
{
    public int DefaultWaitSeconds { get; set; } = 15;
    public int PageLoadSeconds { get; set; } = 60;
    public int ScriptSeconds { get; set; } = 30;
}

public sealed class ArtifactSettings
{
    public string ScreenshotDirectory { get; set; } = "Artifacts/Screenshots";
    public string LogDirectory { get; set; } = "Artifacts/Logs";
}

public sealed class AuthenticationSettings
{
    public string CredentialsFile { get; set; } = "TestData/credentials.json";
}

public sealed class RouteSettings
{
    public string Profile { get; set; } = "/Account/Profile";
    public string ShareSkill { get; set; } = "/Home/ServiceListing";
    public string SearchSkill { get; set; } = "/Home/Search";
    public string ListingManagement { get; set; } = "/Home/ListingManagement";
}
