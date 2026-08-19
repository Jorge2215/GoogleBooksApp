using GoogleBooksApp.Models.GoogleBooks;
using GoogleBooksApp.Services.GoogleBooks;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GoogleBooksApp.Pages;

public sealed class BooksModel(IGoogleBooksService googleBooksService) : PageModel
{
    private readonly IGoogleBooksService _googleBooksService = googleBooksService;

    public const int PageSize = 10;

    [Display(Name = "Title")]
    [BindProperty(SupportsGet = true)]
    public string? TitleQuery { get; set; }

    [Display(Name = "Author")]
    [BindProperty(SupportsGet = true)]
    public string? AuthorQuery { get; set; }

    [Display(Name = "Language")]
    [BindProperty(SupportsGet = true)]
    public string? Language { get; set; }

    [Display(Name = "From year")]
    [BindProperty(SupportsGet = true)]
    public int? YearFrom { get; set; }

    [Display(Name = "To year")]
    [BindProperty(SupportsGet = true)]
    public int? YearTo { get; set; }

    [Display(Name = "Sort by")]
    [BindProperty(SupportsGet = true)]
    public string SortOrder { get; set; } = "relevance";

    [BindProperty(SupportsGet = true)]
    public int PageIndex { get; set; }

    public IReadOnlyList<BookResult> Results { get; private set; } = [];

    public int TotalItems { get; private set; }

    public bool HasNextPage => (PageIndex + 1) * PageSize < TotalItems;

    public bool HasPreviousPage => PageIndex > 0;

    public string? ErrorMessage { get; private set; }

    public Task OnGetAsync(CancellationToken cancellationToken) =>
        LoadSearchResultsAsync(showEmptyQueryMessage: false, cancellationToken);

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        PageIndex = 0;
        NormalizeInput();

        if (!HasSearchCriteria())
        {
            await LoadSearchResultsAsync(showEmptyQueryMessage: true, cancellationToken);
            return Page();
        }

        return RedirectToPage(new
        {
            TitleQuery,
            AuthorQuery,
            Language,
            YearFrom,
            YearTo,
            SortOrder,
            PageIndex
        });
    }

    private async Task LoadSearchResultsAsync(bool showEmptyQueryMessage, CancellationToken cancellationToken)
    {
        NormalizeInput();
        PageIndex = Math.Max(PageIndex, 0);
        Results = [];
        TotalItems = 0;
        ErrorMessage = null;

        if (!showEmptyQueryMessage && !HasSearchCriteria())
        {
            return;
        }

        var searchResult = await _googleBooksService.SearchAsync(
            TitleQuery,
            AuthorQuery,
            PageIndex * PageSize,
            PageSize,
            Language,
            SortOrder,
            YearFrom,
            YearTo,
            cancellationToken);

        Results = searchResult.Items;
        TotalItems = searchResult.TotalItems;
        ErrorMessage = searchResult.ErrorMessage;
    }

    private bool HasSearchCriteria() =>
        !string.IsNullOrWhiteSpace(TitleQuery) || !string.IsNullOrWhiteSpace(AuthorQuery);

    private void NormalizeInput()
    {
        TitleQuery = NormalizeQuery(TitleQuery);
        AuthorQuery = NormalizeQuery(AuthorQuery);
        Language = NormalizeLanguageCode(Language);
        SortOrder = NormalizeSortOrder(SortOrder);
        NormalizeYearRange();
    }

    private static string? NormalizeQuery(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static string? NormalizeLanguageCode(string? language)
    {
        if (string.IsNullOrWhiteSpace(language))
        {
            return null;
        }

        var trimmed = language.Trim().ToLowerInvariant();
        // Accept 2-letter ISO 639-1 codes only
        return trimmed.Length == 2 ? trimmed : null;
    }

    private static string NormalizeSortOrder(string? sortOrder)
    {
        if (string.IsNullOrWhiteSpace(sortOrder))
        {
            return "relevance";
        }

        var trimmed = sortOrder.Trim().ToLowerInvariant();
        return trimmed == "newest" ? "newest" : "relevance";
    }

    private void NormalizeYearRange()
    {
        const int MinYear = 1450;
        const int MaxYear = 2100;

        // Clamp to sane bounds
        if (YearFrom.HasValue)
        {
            YearFrom = Math.Clamp(YearFrom.Value, MinYear, MaxYear);
        }

        if (YearTo.HasValue)
        {
            YearTo = Math.Clamp(YearTo.Value, MinYear, MaxYear);
        }

        // Ensure YearFrom <= YearTo; swap if needed
        if (YearFrom.HasValue && YearTo.HasValue && YearFrom > YearTo)
        {
            (YearFrom, YearTo) = (YearTo, YearFrom);
        }
    }
}
