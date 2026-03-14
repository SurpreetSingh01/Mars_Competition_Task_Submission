namespace Mars.Part2.UiTests.Config;

public sealed class RouteMap
{
    private readonly string _baseUrl;

    public RouteMap(TestSettings settings)
    {
        _baseUrl = settings.Application.BaseUrl.TrimEnd('/');
        Profile = Normalize(settings.Routes.Profile);
        ShareSkill = Normalize(settings.Routes.ShareSkill);
        SearchSkill = Normalize(settings.Routes.SearchSkill);
        ListingManagement = Normalize(settings.Routes.ListingManagement);
    }

    public string Profile { get; }
    public string ShareSkill { get; }
    public string SearchSkill { get; }
    public string ListingManagement { get; }

    public string ToAbsolute(string relativeRoute)
    {
        var normalizedRoute = Normalize(relativeRoute);
        return $"{_baseUrl}{normalizedRoute}";
    }

    private static string Normalize(string route)
    {
        if (string.IsNullOrWhiteSpace(route))
        {
            throw new InvalidOperationException("Route value cannot be empty.");
        }

        return route.StartsWith('/') ? route : $"/{route}";
    }
}
