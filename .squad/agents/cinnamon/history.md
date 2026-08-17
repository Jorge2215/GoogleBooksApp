# Project Context

- **Owner:** Jorgito
- **Project:** GoogleBooksApp — ASP.NET Core Razor Pages app to search books via the Google Books API
- **Stack:** .NET 10, Razor Pages, IHttpClientFactory, Google Books API v1
- **Created:** 2026-08-16

## Learnings

<!-- Append new learnings below. Each entry is something lasting about the project. -->

- 2026-08-16: Toru scaffolded project structure: Configuration/GoogleBooksOptions, Models/GoogleBooks DTOs, Services/GoogleBooks typed-client seam, Pages/Books stub (TODOs remain). API key removed from appsettings.json and is now expected in dotnet user-secrets (UserSecretsId added to project).
- 2026-08-16: Implemented Google Books API pagination in `Services\GoogleBooks\IGoogleBooksService` and `Services\GoogleBooks\GoogleBooksService` with the contract `Task<GoogleBooksSearchResult> SearchAsync(string? title, string? author, int startIndex, int maxResults, CancellationToken cancellationToken = default)`. `Models\GoogleBooks\GoogleBooksSearchResult.cs` now carries `Items`, `TotalItems`, and `ErrorMessage`, and `Pages\Books.cshtml.cs` exposes `TitleQuery`, `AuthorQuery`, `PageIndex`, `PageSize`, `Results`, `TotalItems`, `HasNextPage`, `HasPreviousPage`, and `ErrorMessage` for the Razor view.
