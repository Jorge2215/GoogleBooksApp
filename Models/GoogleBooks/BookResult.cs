using System.Text.Json.Serialization;

namespace GoogleBooksApp.Models.GoogleBooks;

public sealed class BookResult
{
    [JsonPropertyName("id")]
    public string Id { get; init; } = string.Empty;

    [JsonPropertyName("volumeInfo")]
    public VolumeInfo? VolumeInfo { get; init; }
}
