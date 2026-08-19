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
        string? language = null,
        string? sortOrder = null,
        int? yearFrom = null,
        int? yearTo = null,
        CancellationToken cancellationToken = default)
    {
        var normalizedTitle = NormalizeQueryPart(title);
        var normalizedAuthor = NormalizeQueryPart(author);
        var normalizedLanguage = NormalizeLanguage(language);
        var normalizedSortOrder = NormalizeSortOrder(sortOrder);
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

        var requestUri = BuildRequestUri(
            normalizedTitle,
            normalizedAuthor,
            sanitizedStartIndex,
            sanitizedMaxResults,
            normalizedLanguage,
            normalizedSortOrder);

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

            // CLIENT-SIDE FILTERING: Apply language and year-range filters after deserialization.
            // - Language filter: While langRestrict is sent to the API (server-side), we also filter
            //   client-side for consistency and to handle edge cases where the API returns non-matching items.
            // - Year filter: Google Books API does not support server-side year-range filtering, so this
            //   is purely client-side. When a year filter is active, we update TotalItems to reflect the
            //   filtered count (not the API's original total). This means pagination may show inaccurate
            //   "total" counts across pages, as each page is filtered independently.
            var filteredItems = FilterByLanguageAndYearRange(apiResult.Items, normalizedLanguage, yearFrom, yearTo);
            var hasFilter = normalizedLanguage is not null || yearFrom.HasValue || yearTo.HasValue;

            if (filteredItems.Count == 0)
            {
                // Filters eliminated all results on this page
                return new GoogleBooksSearchResult
                {
                    TotalItems = hasFilter ? 0 : apiResult.TotalItems,
                    ErrorMessage = "No books matched your search criteria on this page."
                };
            }

            return new GoogleBooksSearchResult
            {
                Items = filteredItems,
                TotalItems = hasFilter ? filteredItems.Count : apiResult.TotalItems
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

    private static string? NormalizeLanguage(string? language)
    {
        if (string.IsNullOrWhiteSpace(language))
        {
            return null;
        }

        var trimmed = language.Trim().ToLowerInvariant();
        // Accept 2-letter ISO 639-1 codes only
        return trimmed.Length == 2 ? trimmed : null;
    }

    private static string? NormalizeSortOrder(string? sortOrder)
    {
        if (string.IsNullOrWhiteSpace(sortOrder))
        {
            return null;
        }

        var trimmed = sortOrder.Trim().ToLowerInvariant();
        return trimmed == "newest" ? "newest" : null; // "relevance" is API default, so null = relevance
    }

    private static IReadOnlyList<BookResult> FilterByLanguageAndYearRange(
        IReadOnlyList<BookResult> items,
        string? language,
        int? yearFrom,
        int? yearTo)
    {
        if (language is null && yearFrom is null && yearTo is null)
        {
            return items;
        }

        var filtered = new List<BookResult>();

        foreach (var item in items)
        {
            // Language filter (client-side, in addition to server-side langRestrict)
            if (language is not null)
            {
                var itemLanguage = item.VolumeInfo?.Language?.ToLowerInvariant();
                if (itemLanguage != language)
                {
                    continue;
                }
            }

            // Year filter (client-side only, API doesn't support year filtering)
            if (yearFrom.HasValue || yearTo.HasValue)
            {
                var publishedYear = ExtractYear(item.VolumeInfo?.PublishedDate);

                // If year filter is active but we can't determine the book's year, exclude it
                if (publishedYear is null)
                {
                    continue;
                }

                if (yearFrom.HasValue && publishedYear < yearFrom.Value)
                {
                    continue;
                }

                if (yearTo.HasValue && publishedYear > yearTo.Value)
                {
                    continue;
                }
            }

            filtered.Add(item);
        }

        return filtered;
    }

    private static int? ExtractYear(string? publishedDate)
    {
        if (string.IsNullOrWhiteSpace(publishedDate))
        {
            return null;
        }

        // Google Books returns dates in formats like "2020", "2020-05", "2020-05-12"
        // Extract the leading 4-digit year
        if (publishedDate.Length >= 4 && int.TryParse(publishedDate[..4], out var year))
        {
            return year;
        }

        return null;
    }

    private string BuildRequestUri(
        string? title,
        string? author,
        int startIndex,
        int maxResults,
        string? language,
        string? sortOrder)
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

        var uriBuilder = new System.Text.StringBuilder();
        uriBuilder.Append($"volumes?q={string.Join("+", queryParts)}");
        uriBuilder.Append($"&startIndex={startIndex}");
        uriBuilder.Append($"&maxResults={maxResults}");

        // langRestrict: ISO 639-1 language code (e.g., "en", "es", "fr")
        if (language is not null)
        {
            uriBuilder.Append($"&langRestrict={Uri.EscapeDataString(language)}");
        }

        // orderBy: "newest" or omit for "relevance" (API default)
        if (sortOrder == "newest")
        {
            uriBuilder.Append("&orderBy=newest");
        }

        uriBuilder.Append($"&key={Uri.EscapeDataString(_options.ApiKey)}");

        return uriBuilder.ToString();
    }

    private static string BuildEncodedQueryPart(string filter, string value) =>
        Uri.EscapeDataString($"{filter}:\"{value}\"");
}
