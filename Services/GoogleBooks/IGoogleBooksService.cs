using GoogleBooksApp.Models.GoogleBooks;

namespace GoogleBooksApp.Services.GoogleBooks;

public interface IGoogleBooksService
{
    Task<GoogleBooksSearchResult> SearchAsync(
        string? title,
        string? author,
        int startIndex,
        int maxResults,
        string? language = null,
        string? sortOrder = null,
        int? yearFrom = null,
        int? yearTo = null,
        CancellationToken cancellationToken = default);

    Task<BookResult?> GetByIdAsync(string volumeId, CancellationToken cancellationToken = default);
}
