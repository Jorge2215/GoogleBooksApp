using System.Text.Json.Serialization;

namespace GoogleBooksApp.Models.GoogleBooks;

public sealed class ImageLinks
{
    [JsonPropertyName("smallThumbnail")]
    public string? SmallThumbnail { get; init; }

    [JsonPropertyName("thumbnail")]
    public string? Thumbnail { get; init; }
}
