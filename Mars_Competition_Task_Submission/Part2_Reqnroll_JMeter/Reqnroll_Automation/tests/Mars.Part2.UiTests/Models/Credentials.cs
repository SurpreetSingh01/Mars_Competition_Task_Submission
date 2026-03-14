namespace Mars.Part2.UiTests.Models;

public sealed class Credentials
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;

    public Credentials MergeEnvironmentOverrides()
    {
        var envEmail = Environment.GetEnvironmentVariable("MARS_EMAIL");
        var envPassword = Environment.GetEnvironmentVariable("MARS_PASSWORD");

        return new Credentials
        {
            Email = string.IsNullOrWhiteSpace(envEmail) ? Email : envEmail,
            Password = string.IsNullOrWhiteSpace(envPassword) ? Password : envPassword
        };
    }
}
