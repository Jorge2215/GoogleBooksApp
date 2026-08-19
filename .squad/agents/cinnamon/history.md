# Project Context

- **Owner:** Jorgito
- **Project:** GoogleBooksApp — ASP.NET Core Razor Pages app to search books via the Google Books API
- **Stack:** .NET 10, Razor Pages, IHttpClientFactory, Google Books API v1
- **Created:** 2026-08-16

## Learnings

<!-- Append new learnings below. Each entry is something lasting about the project. -->

- 2026-08-16: Toru scaffolded project structure: Configuration/GoogleBooksOptions, Models/GoogleBooks DTOs, Services/GoogleBooks typed-client seam, Pages/Books stub (TODOs remain). API key removed from appsettings.json and is now expected in dotnet user-secrets (UserSecretsId added to project).
- 2026-08-16: Implemented Google Books API pagination in `Services\GoogleBooks\IGoogleBooksService` and `Services\GoogleBooks\GoogleBooksService` with the contract `Task<GoogleBooksSearchResult> SearchAsync(string? title, string? author, int startIndex, int maxResults, CancellationToken cancellationToken = default)`. `Models\GoogleBooks\GoogleBooksSearchResult.cs` now carries `Items`, `TotalItems`, and `ErrorMessage`, and `Pages\Books.cshtml.cs` exposes `TitleQuery`, `AuthorQuery`, `PageIndex`, `PageSize`, `Results`, `TotalItems`, `HasNextPage`, `HasPreviousPage`, and `ErrorMessage` for the Razor view.
- 2026-08-18: Implemented advanced search filters backend (GitHub issue #5): Added `PublishedDate` and `Language` properties to `VolumeInfo.cs`; extended `SearchAsync` with `language`, `sortOrder`, `yearFrom`, and `yearTo` parameters. Language filter maps to Google Books API's `langRestrict` (server-side) and is also applied client-side for consistency. Sort order maps to `orderBy` ("newest" only, "relevance" is default/omitted). Year-range filtering is purely client-side (API limitation) with robust date parsing for formats "YYYY", "YYYY-MM", "YYYY-MM-DD". When filters are active, `TotalItems` reflects the filtered count per page (not API's original total). `Pages\Books.cshtml.cs` now exposes `Language`, `YearFrom`, `YearTo`, and `SortOrder` bound properties with normalization/validation. All 30 tests pass (10 original + 20 new filter tests). Backend contract is ready for UI wiring by Creta.
- 2026-08-18: Implemented book details page backend (GitHub issue #6): Created `Models/GoogleBooks/IndustryIdentifier.cs` for ISBN/identifier representation. Extended `VolumeInfo.cs` with `Categories`, `PageCount`, and `IndustryIdentifiers` properties. Added `Task<BookResult?> GetByIdAsync(string volumeId, CancellationToken cancellationToken = default)` to `IGoogleBooksService` and `GoogleBooksService` — calls Google Books API's single-volume endpoint `volumes/{volumeId}?key={apiKey}`, returns `null` for missing ID, unconfigured API key, non-success status, or deserialization failure (all failures logged as warnings). Created `Pages\BookDetails.cshtml.cs` with route `@page "/Books/Details/{Id}"`, exposing `Id` (route-bound), `Book` (BookResult?), and `IsNotFound` (bool). Minimal placeholder .cshtml created for build compatibility. All 52 tests pass. Backend contract ready for UI wiring by Creta.


