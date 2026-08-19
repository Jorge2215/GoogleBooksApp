using System.Text.Json.Serialization;

namespace GoogleBooksApp.Models.GoogleBooks;

public sealed class IndustryIdentifier
{
    [JsonPropertyName("type")]
    public string? Type { get; init; }

    [JsonPropertyName("identifier")]
    public string? Identifier { get; init; }
}
