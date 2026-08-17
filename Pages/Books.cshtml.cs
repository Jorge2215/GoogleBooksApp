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
    }

    private static string? NormalizeQuery(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
