using GoogleBooksApp.Configuration;
using GoogleBooksApp.Models.GoogleBooks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace GoogleBooksApp.Services.GoogleBooks;

public sealed class GoogleBooksService(
    HttpClient httpClient,
    IOptions<GoogleBooksOptions> options,
    ILogger<GoogleBooksService> logger) : IGoogleBooksService
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);
    private readonly HttpClient _httpClient = httpClient;
    private readonly GoogleBooksOptions _options = options.Value;
    private readonly ILogger<GoogleBooksService> _logger = logger;

    public async Task<GoogleBooksSearchResult> SearchAsync(
        string? title,
        string? author,
        int startIndex,
        int maxResults,
        CancellationToken cancellationToken = default)
    {
        var normalizedTitle = NormalizeQueryPart(title);
        var normalizedAuthor = NormalizeQueryPart(author);
        var sanitizedStartIndex = Math.Max(startIndex, 0);
        var sanitizedMaxResults = Math.Clamp(maxResults, 1, 40);

        if (string.IsNullOrWhiteSpace(normalizedTitle) && string.IsNullOrWhiteSpace(normalizedAuthor))
        {
            return GoogleBooksSearchResult.Empty("Enter a title or author to search.");
        }

        if (string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            _logger.LogWarning("Google Books API key is not configured.");
            return GoogleBooksSearchResult.Empty("Book search is unavailable right now. Please try again later.");
        }

        var requestUri = BuildRequestUri(normalizedTitle, normalizedAuthor, sanitizedStartIndex, sanitizedMaxResults);

        try
        {
            using var response = await _httpClient.GetAsync(requestUri, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "Google Books API returned status code {StatusCode} for request {RequestUri}.",
                    (int)response.StatusCode,
                    requestUri);

                return GoogleBooksSearchResult.Empty("Unable to load books right now. Please try again later.");
            }

            await using var contentStream = await response.Content.ReadAsStreamAsync(cancellationToken);
            var apiResult = await JsonSerializer.DeserializeAsync<GoogleBooksSearchResult>(
                contentStream,
                SerializerOptions,
                cancellationToken);

            if (apiResult is null)
            {
                _logger.LogWarning("Google Books API returned an empty response body for request {RequestUri}.", requestUri);
                return GoogleBooksSearchResult.Empty("Received an invalid response from the book service.");
            }

            if (apiResult.TotalItems <= 0)
            {
                return GoogleBooksSearchResult.Empty("No books matched your search.");
            }

            if (apiResult.Items.Count == 0)
            {
                return new GoogleBooksSearchResult
                {
                    TotalItems = apiResult.TotalItems,
                    ErrorMessage = "No books were returned for the selected page."
                };
            }

            return new GoogleBooksSearchResult
            {
                Items = apiResult.Items,
                TotalItems = apiResult.TotalItems
            };
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(ex, "Google Books request failed for {RequestUri}.", requestUri);
            return GoogleBooksSearchResult.Empty("Unable to reach the book service right now. Please try again later.");
        }
        catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            _logger.LogWarning(ex, "Google Books request timed out for {RequestUri}.", requestUri);
            return GoogleBooksSearchResult.Empty("The book service took too long to respond. Please try again later.");
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "Google Books response could not be parsed for {RequestUri}.", requestUri);
            return GoogleBooksSearchResult.Empty("Received an invalid response from the book service.");
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Unexpected Google Books failure for {RequestUri}.", requestUri);
            throw;
        }
    }

    private static string? NormalizeQueryPart(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private string BuildRequestUri(string? title, string? author, int startIndex, int maxResults)
    {
        var queryParts = new List<string>();

        if (title is not null)
        {
            queryParts.Add(BuildEncodedQueryPart("intitle", title));
        }

        if (author is not null)
        {
            queryParts.Add(BuildEncodedQueryPart("inauthor", author));
        }

        return $"volumes?q={string.Join("+", queryParts)}&startIndex={startIndex}&maxResults={maxResults}&key={Uri.EscapeDataString(_options.ApiKey)}";
    }

    private static string BuildEncodedQueryPart(string filter, string value) =>
        Uri.EscapeDataString($"{filter}:\"{value}\"");
}
