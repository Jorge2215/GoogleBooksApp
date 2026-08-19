using System.Text.Json.Serialization;

namespace GoogleBooksApp.Models.GoogleBooks;

public sealed class VolumeInfo
{
    [JsonPropertyName("title")]
    public string? Title { get; init; }

    [JsonPropertyName("authors")]
    public IReadOnlyList<string>? Authors { get; init; }

    [JsonPropertyName("description")]
    public string? Description { get; init; }

    [JsonPropertyName("imageLinks")]
    public ImageLinks? ImageLinks { get; init; }

    [JsonPropertyName("publishedDate")]
    public string? PublishedDate { get; init; }

    [JsonPropertyName("language")]
    public string? Language { get; init; }
}
