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
- 2026-08-17: CI/CD uses three dedicated GitHub Actions workflows at `.github\workflows\deploy-dev.yml`, `.github\workflows\deploy-qas.yml`, and `.github\workflows\deploy-prd.yml` with .NET 10 build/publish steps and Azure App Service deployment via publish-profile secrets.
- 2026-08-17: Release branch policy target is `dev` open for direct pushes, with `qas` and `main` intended to require pull requests while using `push` triggers on those protected branches to deploy after merges land.
- 2026-08-17: Always verify gh CLI writes with a follow-up GET before reporting success.
