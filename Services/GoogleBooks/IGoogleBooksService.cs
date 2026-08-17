using GoogleBooksApp.Models.GoogleBooks;

namespace GoogleBooksApp.Services.GoogleBooks;

public interface IGoogleBooksService
{
    Task<GoogleBooksSearchResult> SearchAsync(
        string? title,
        string? author,
        int startIndex,
        int maxResults,
        CancellationToken cancellationToken = default);
}
