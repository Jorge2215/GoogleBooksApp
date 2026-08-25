using GoogleBooksApp.Models.GoogleBooks;
using GoogleBooksApp.Services.GoogleBooks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GoogleBooksApp.Pages;

public sealed class BookDetailsModel(IGoogleBooksService googleBooksService) : PageModel
{
    private readonly IGoogleBooksService _googleBooksService = googleBooksService;

    [BindProperty(SupportsGet = true)]
    public string? Id { get; set; }

    public BookResult? Book { get; private set; }

    public bool IsNotFound { get; private set; }

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(Id))
        {
            IsNotFound = true;
            return;
        }

        Book = await _googleBooksService.GetByIdAsync(Id, cancellationToken);
        IsNotFound = Book is null;
    }
}
