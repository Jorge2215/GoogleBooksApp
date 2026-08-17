# Project Context

- **Owner:** Jorgito
- **Project:** GoogleBooksApp — ASP.NET Core Razor Pages app to search books via the Google Books API
- **Stack:** .NET 10, Razor Pages, IHttpClientFactory, Google Books API v1
- **Created:** 2026-08-16

## Learnings

<!-- Append new learnings below. Each entry is something lasting about the project. -->
- 2026-08-16: Architecture scaffolding lives under `Configuration\`, `Models\GoogleBooks\`, and `Services\GoogleBooks\` so API options, contracts, and DTOs stay separated from Razor Pages.
- 2026-08-16: `Program.cs` binds `GoogleBooks` through `IOptions<GoogleBooksOptions>` and registers a typed `HttpClient` for `IGoogleBooksService`; Cinnamon owns the real API implementation behind that seam.
- 2026-08-16: `Pages\Books.cshtml` and `Pages\Books.cshtml.cs` are placeholder search page stubs for Creta/Cinnamon to complete without reworking routing or DI.
- 2026-08-16: `appsettings.json` keeps only the `GoogleBooks:ApiKey` shape with an empty value; local development should use `dotnet user-secrets` via `UserSecretsId=GoogleBooksApp-local-secrets`.
