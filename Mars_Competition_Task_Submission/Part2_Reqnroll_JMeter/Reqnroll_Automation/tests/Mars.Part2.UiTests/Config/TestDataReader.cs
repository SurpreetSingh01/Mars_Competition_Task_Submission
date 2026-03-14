using System.Text.Json;
using Mars.Part2.UiTests.Helpers;

namespace Mars.Part2.UiTests.Config;

public sealed class TestDataReader
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public T ReadRequired<T>(string relativePath, string key) where T : class, new()
    {
        var fullPath = PathHelper.ResolveProjectPath(relativePath);

        if (!File.Exists(fullPath))
        {
            throw new FileNotFoundException($"Test data file was not found: {fullPath}");
        }

        var json = File.ReadAllText(fullPath);
        var records = JsonSerializer.Deserialize<Dictionary<string, T>>(json, SerializerOptions);

        if (records is null || !records.TryGetValue(key, out var value) || value is null)
        {
            throw new KeyNotFoundException($"Test data key '{key}' was not found in {fullPath}.");
        }

        return value;
    }
}
