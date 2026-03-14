using Newtonsoft.Json.Linq;

namespace MarsAutomation.Helpers;

public static class JsonReader
{
    private static readonly Lazy<JObject> LazyRoot = new(() =>
    {
        var baseDir = AppContext.BaseDirectory;
        var path = Path.Combine(baseDir, "TestData", "TestData.json");
        if (!File.Exists(path))
        {
            throw new FileNotFoundException($"Test data file not found at '{path}'.");
        }

        var json = File.ReadAllText(path);
        return JObject.Parse(json);
    });

    private static JObject Root => LazyRoot.Value;

    public static string GetString(params string[] pathSegments)
    {
        if (pathSegments.Length == 0)
        {
            throw new ArgumentException("At least one path segment is required.", nameof(pathSegments));
        }

        JToken? token = Root;
        foreach (var segment in pathSegments)
        {
            token = token?[segment];
            if (token is null)
            {
                throw new InvalidOperationException($"Path '{string.Join(".", pathSegments)}' not found in TestData.json.");
            }
        }

        return token.Type == JTokenType.String
            ? token.Value<string>()!
            : token.ToString();
    }
}

