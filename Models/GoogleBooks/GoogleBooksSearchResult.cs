using System.Text.Json.Serialization;

namespace GoogleBooksApp.Models.GoogleBooks;

public sealed class GoogleBooksSearchResult
{
    [JsonPropertyName("items")]
    public IReadOnlyList<BookResult> Items { get; init; } = [];

    [JsonPropertyName("totalItems")]
    public int TotalItems { get; init; }

    [JsonIgnore]
    public string? ErrorMessage { get; init; }

    public static GoogleBooksSearchResult Empty(string? errorMessage = null) =>
        new()
        {
            ErrorMessage = errorMessage
        };
}
