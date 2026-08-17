# Project Context

- **Owner:** Jorgito
- **Project:** GoogleBooksApp — ASP.NET Core Razor Pages app to search books via the Google Books API
- **Stack:** .NET 10, Razor Pages, IHttpClientFactory, Google Books API v1
- **Created:** 2026-08-16

## Learnings

<!-- Append new learnings below. Each entry is something lasting about the project. -->

- 2026-08-16: Toru scaffolded project structure: Configuration/GoogleBooksOptions, Models/GoogleBooks DTOs, Services/GoogleBooks typed-client seam, Pages/Books stub (TODOs remain). API key removed from appsettings.json and is now expected in dotnet user-secrets (UserSecretsId added to project).
- 2026-08-16: Creta built the `Pages\Books.cshtml` search experience with a POST search form, responsive results cards, thumbnail and broken-image fallbacks, empty/error messaging, and pagination links that preserve `TitleQuery` and `AuthorQuery`. The view reads Cinnamon's planned pagination/error properties (`PageIndex`, `TotalItems`, `HasNextPage`, `HasPreviousPage`, `ErrorMessage`) defensively via reflection so Razor still compiles until the code-behind lands, and assumes Google Books detail links can be built from `BookResult.Id` as `https://books.google.com/books?id={Id}`.
- 2026-08-16: Once Cinnamon confirmed the final `BooksModel` contract, the `Pages\Books.cshtml` top code block was simplified to direct property access for `ErrorMessage`, `PageIndex`, `TotalItems`, `HasNextPage`, and `HasPreviousPage` without changing the rendered UI.
