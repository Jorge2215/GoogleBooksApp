# Squad Decisions

## Active Decisions

### Toru — Project Structure Decision (2026-08-16)

**Folder structure**
- `Configuration\GoogleBooksOptions.cs` holds configuration binding types.
- `Models\GoogleBooks\` holds DTOs for `BookResult`, `VolumeInfo`, and `ImageLinks`.
- `Services\GoogleBooks\` holds the `IGoogleBooksService` contract and the placeholder typed-client implementation seam for Cinnamon.
- `Pages\Books.cshtml` and `Pages\Books.cshtml.cs` reserve the search page route and page model for Creta and Cinnamon.

**Configuration strategy**
- Bind the `GoogleBooks` section through `IOptions<GoogleBooksOptions>`.
- Register the Google Books service as a typed `HttpClient` so outbound API concerns stay centralized and testable.
- Keep `appsettings.json` as structure only; local secrets belong in the user-secrets store and deployed secrets belong in environment-specific configuration.

**API key handling**
- Replaced the plaintext `GoogleBooks:ApiKey` value in `appsettings.json` with an empty string so the repository copy no longer carries a live-looking secret.
- Added `UserSecretsId` to the project so local development can use `dotnet user-secrets`.
- This workspace is not currently a valid Git repository, so tracked state / `.gitignore` / Git history could not be verified. Treat history as potentially exposed and rotate the key if it was ever pushed.

**Coding standards**
- PascalCase for public types/members, camelCase for locals/parameters; clear suffixes like `Options`, `Service`, `Model`.
- Nullable reference types enabled; model optional API fields explicitly as nullable instead of suppressing warnings.
- Prefer async methods returning `Task`/`Task<T>` with a `CancellationToken` parameter for I/O boundaries.
- Default DI lifetimes: singleton for stateless configuration, scoped for request state, typed `HttpClient` for outbound HTTP services.
- Read configuration through options binding, not ad-hoc `IConfiguration` lookups inside feature code.

## Upcoming Work (Planned — Next Session)

- **2026-08-16 (Jorgito):** Next working session will focus on: (1) publishing the source code to a GitHub repository, (2) publishing/deploying the web app to Azure. Not started yet — flagging for Toru (repo/deploy structure) and team routing when session resumes.

## Governance

- All meaningful changes require team consensus
- Document architectural decisions here
- Keep history focused on work, decisions focused on direction

---
### Merged inbox decisions — 2026-08-17T01:44:01Z

#### Cinnamon (Backend Developer)
## Cinnamon API Integration Contract — 2026-08-16

### Service contract
- `IGoogleBooksService` now exposes:
  - `Task<GoogleBooksSearchResult> SearchAsync(string? title, string? author, int startIndex, int maxResults, CancellationToken cancellationToken = default)`
- `title` and `author` are optional so the page model can support title-only, author-only, or combined searches.
- `startIndex` and `maxResults` map directly to the Google Books API pagination parameters.

### Result model
- `Models\GoogleBooks\GoogleBooksSearchResult.cs` is the service return type and API response envelope.
- Shape:
  - `IReadOnlyList<BookResult> Items`
  - `int TotalItems`
  - `string? ErrorMessage`
- Expected behaviors:
  - No query: empty `Items`, `TotalItems = 0`, `ErrorMessage = "Enter a title or author to search."`
  - Zero matches: empty `Items`, `TotalItems = 0`, `ErrorMessage = "No books matched your search."`
  - Upstream/API failure or timeout: empty `Items`, `TotalItems = 0`, user-safe `ErrorMessage`
  - Valid page with results: populated `Items`, `TotalItems > 0`, `ErrorMessage = null`

### BooksModel contract for Creta
- `string? TitleQuery { get; set; }`
- `string? AuthorQuery { get; set; }`
- `int PageIndex { get; set; }` (`0`-based, query-string bindable)
- `const int PageSize = 10`
- `IReadOnlyList<BookResult> Results { get; }`
- `int TotalItems { get; }`
- `bool HasNextPage { get; }`
- `bool HasPreviousPage { get; }`
- `string? ErrorMessage { get; }`

### Page behavior
- `OnPostAsync` normalizes input, resets to page `0`, and redirects to the GET route for shareable/searchable URLs.
- `OnGetAsync` performs the actual search for query-string driven requests and pagination links.
- The Razor view should bind directly to the public property names listed above; those names are now final for this iteration.

### HTTP client setup
- `Program.cs` keeps the typed `HttpClient` registration and now sets:
  - Base address: `https://www.googleapis.com/books/v1/`
  - Timeout: `10` seconds


#### Creta (Frontend Developer)
## Creta Search UI Notes — 2026-08-16

### What I built
- Replaced the placeholder `Pages\Books.cshtml` content with a complete Razor Pages search UI.
- Added a semantic POST search form bound to `TitleQuery` and `AuthorQuery`.
- Added responsive result cards for each `BookResult` showing title, comma-joined authors, description copy, a Google Books link, and a cover area that stays stable when images are missing or fail to load.
- Added empty/error messaging and previous/next pagination controls that carry the active query terms in the route values.

### Assumptions to confirm with Cinnamon
- I built pagination/error UI against the documented `BooksModel` contract: `PageIndex`, `TotalItems`, `HasNextPage`, `HasPreviousPage`, and `ErrorMessage`.
- Because those properties are not present in the current local `Books.cshtml.cs` yet, the view reads them via reflection for now so the app still builds. If Cinnamon ships different property names, update the view bindings to match the final contract.
- I constructed the external book link as `https://books.google.com/books?id={Id}` because `BookResult` does not currently expose a dedicated preview link field. If Cinnamon later adds a proper preview URL from the API response, the markup should switch to that field.

### Files touched
- `Pages\Books.cshtml`
- `.squad\agents\creta\history.md`
- `.squad\decisions\inbox\creta-search-ui.md`

### Validation
- Ran `dotnet build` on 2026-08-16.
- The Razor view no longer reports compilation errors after the `Request.Query` fix.
- The full project build is currently blocked by a syntax error in `Pages\Books.cshtml.cs` around `NormalizeQuery` (duplicate line in Cinnamon-owned code-behind), so final end-to-end build confirmation needs Cinnamon to correct that file first.


## Creta View Cleanup Notes — 2026-08-16

- Removed the temporary reflection-based property reads from `Pages\Books.cshtml` now that `BooksModel` publicly exposes `ErrorMessage`, `PageIndex`, `TotalItems`, `HasNextPage`, and `HasPreviousPage`.
- Kept the page markup, styling, and behavior unchanged; this was a view-only cleanup of the top Razor code block.
- Revalidated the project with `dotnet build` after switching the view to direct model property access.


---

---
### Merged inbox: malta-test-suite.md - 2026-08-16T22:54:16.6120154-03:00


# Malta Test Suite Report — 2026-08-16

## What I tested

- `GoogleBooksService` title-only query composition
- `GoogleBooksService` author-only query composition
- `GoogleBooksService` combined title + author query composition
- Pagination parameters (`startIndex`, `maxResults`)
- Zero-result API responses
- Empty search criteria handling with no outbound HTTP call
- Non-success HTTP responses
- Malformed and empty JSON responses
- Missing optional response fields (`authors`, `description`, `imageLinks`)

## Test project

- Location: `GoogleBooksApp.Tests\GoogleBooksApp.Tests.csproj`
- Framework: `net10.0`
- Referenced from: `GoogleBooksApp.slnx`

## Manual checklist

- Location: `.squad/agents/malta/test-checklist.md`

## How to run

- Build: `dotnet build GoogleBooksApp.slnx --nologo`
- Tests: `dotnet test GoogleBooksApp.slnx --nologo`

## Validation result

- `dotnet build GoogleBooksApp.slnx --nologo` passed.
- `dotnet test GoogleBooksApp.slnx --nologo` passed.
- Automated coverage currently includes 10 passing xUnit tests for the service layer using a stub `HttpMessageHandler` only; no live Google Books API calls are made.

## Bugs found for coordinator routing

- No new product bugs were confirmed in Cinnamon or Creta code during this test pass.

## Notes

- I added root project exclusions for `GoogleBooksApp.Tests\**` so the web app project does not compile test sources or copy nested test artifacts during solution builds.

---
### Merged inbox: nutmeg-readme.md - 2026-08-16T22:54:16.6611336-03:00


Nutmeg README update — 2026-08-16

I created/updated README.md in the repo root to cover:

- Project overview: what the app does (search Google Books by title/author)
- Prerequisites: .NET 10 SDK
- Setup: how to obtain a Google Books API key (Google Cloud Console -> enable Books API -> create API key) and how to store it safely using dotnet user-secrets (dotnet user-secrets set "GoogleBooks:ApiKey" "YOUR_KEY") from the project directory (the project contains a UserSecretsId).
- Run instructions: dotnet run
- Functional usage: title/author/both searches, pagination (PageSize = 10, zero-based PageIndex), and UI messages for empty/no-result/error states.
- Project structure overview: Configuration/, Models/GoogleBooks/, Services/GoogleBooks/, Pages/Books.*
- Testing: noted that no test project exists yet; include instructions to run `dotnet test` once tests are added.

If you'd like any wording changes, or want additional screenshots or examples, I can update the README.

--- Inbox file: toru-home-page-route.md (orig LastWrite: 08/16/2026 23:06:46) ---

# Decision: Books.cshtml as Root Route

**Date:** 2026-08-16  
**Author:** Toru (Architect)  
**Status:** Implemented

## Context

The app's purpose is Google Books search. The default Razor Pages scaffold set `Pages/Index.cshtml` as the root route `/`, but the real entry point should be `Pages/Books.cshtml`.

## Decision

- Changed `@page` to `@page "/"` in `Pages/Books.cshtml` — it now owns the root route.
- Changed `@page` to `@page "/Index"` in `Pages/Index.cshtml` — still reachable at `/Index` but no longer conflicts with `/`.
- Updated `Pages/Shared/_Layout.cshtml` nav: the brand link and "Home" nav item both now point to `asp-page="/Books"` (which resolves to `/`).

## Rationale

Setting the route directly via `@page "/"` on `Books.cshtml` is the idiomatic Razor Pages approach — no redirect overhead, no Program.cs plumbing, and it's immediately clear from the page file itself which page owns the root.


---
### Merged inbox: creta-visual-theme.md - 2026-08-16T23:10:37.0596850-03:00


# Creta Visual Theme Decision — 2026-08-16

## Palette

- Background: `#eef7ff`
- Strong background accent: `#d8ebff`
- Surface: `#ffffff`
- Muted surface: `#f5faff`
- Border: `#cfe0f2`
- Primary blue: `#4a90e2`
- Primary blue (strong): `#2f6fb2`
- Text: `#111827`
- Muted text: `#475569`
- Error background: `#fff1f2`
- Error text: `#9f1239`

## Typography

- Font family: `Inter`, falling back to `system-ui`, `-apple-system`, `BlinkMacSystemFont`, `"Segoe UI"`, `sans-serif`
- Heading style: heavier weight with tighter tracking for clearer hierarchy
- Body copy: 16px base size with 1.6 line-height for readability

## Files touched

- `Pages\Shared\_Layout.cshtml`
- `Pages\Books.cshtml`
- `wwwroot\css\site.css`
- `.squad\agents\creta\history.md`
- `.squad\decisions\inbox\creta-visual-theme.md`

## UI state confirmation

- Preserved the functional rendering for search form, results list, pagination, empty state, error state, and missing-cover fallback.
- Moved the shared Books page theme rules into the global stylesheet so the home route, Privacy page, Index page, and Error page inherit the same palette and typography.



--- Inbox merge: toru-gitignore-update.md (LastWrite: 2026-08-16 23:45:03Z) ---

# Decision: .gitignore updated for .NET / Visual Studio

**Date:** 2026-08-16  
**Author:** Toru (Architect)  
**Status:** Accepted

## Context

The repo is being published to GitHub. The existing `.gitignore` only covered Squad runtime noise. Standard .NET/Visual Studio build artifacts (`bin/`, `obj/`, `.vs/`, etc.) were unguarded and would have been committed.

## Decision

Prepended a `# .NET / Visual Studio` section to `.gitignore` covering:
- `[Bb]in/`, `[Oo]bj/` — build output directories
- `.vs/` — Visual Studio local IDE state
- `.idea/` — JetBrains Rider IDE state
- `*.user`, `*.suo` — user-specific project files
- `packages/` — NuGet packages restore folder
- NuGet package artefacts (`*.nupkg`, `*.snupkg`, lock files)

## Explicitly NOT ignored

- `appsettings.json` — no longer contains secrets; real key lives in dotnet user-secrets (outside the repo). Safe and intentional to commit.
- All `.squad/` content that should be committed (decisions, team, routing, agents, casting) — left untouched.
