namespace Mars.Part2.UiTests.Helpers;

public static class PathHelper
{
    private static readonly Lazy<string> ProjectRoot = new(FindProjectRoot);

    public static string GetProjectRoot() => ProjectRoot.Value;

    public static string ResolveProjectPath(string relativePath)
    {
        var normalized = relativePath.Replace('/', Path.DirectorySeparatorChar);
        return Path.GetFullPath(Path.Combine(GetProjectRoot(), normalized));
    }

    private static string FindProjectRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);

        while (current is not null)
        {
            var projectFile = Path.Combine(current.FullName, "Mars.Part2.UiTests.csproj");
            if (File.Exists(projectFile))
            {
                return current.FullName;
            }

            current = current.Parent;
        }

        throw new DirectoryNotFoundException("Could not locate the Mars.Part2.UiTests project root.");
    }
}
