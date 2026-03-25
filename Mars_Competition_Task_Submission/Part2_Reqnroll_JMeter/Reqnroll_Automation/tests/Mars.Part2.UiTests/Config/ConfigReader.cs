using Microsoft.Extensions.Configuration;
using Mars.Part2.UiTests.Helpers;

namespace Mars.Part2.UiTests.Config;

public static class ConfigReader
{
    private static readonly Lazy<TestSettings> CachedSettings = new(LoadInternal);

    public static TestSettings Load() => CachedSettings.Value;

    private static TestSettings LoadInternal()
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(PathHelper.GetProjectRoot())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
            .Build();

        var settings = configuration.Get<TestSettings>() ?? new TestSettings();

        if (string.IsNullOrWhiteSpace(settings.Application.BaseUrl))
        {
            throw new InvalidOperationException("Application.BaseUrl is missing in appsettings.json.");
        }

        return settings;
    }
}
