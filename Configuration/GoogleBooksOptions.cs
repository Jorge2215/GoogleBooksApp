namespace GoogleBooksApp.Configuration;

public sealed class GoogleBooksOptions
{
    public const string SectionName = "GoogleBooks";

    public string ApiKey { get; init; } = string.Empty;
}
