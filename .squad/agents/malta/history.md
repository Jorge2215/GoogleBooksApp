# Project Context

- **Owner:** Jorgito
- **Project:** GoogleBooksApp — ASP.NET Core Razor Pages app to search books via the Google Books API
- **Stack:** .NET 10, Razor Pages, IHttpClientFactory, Google Books API v1
- **Created:** 2026-08-16

## Learnings

<!-- Append new learnings below. Each entry is something lasting about the project. -->

- [Scribe] (UTC 2026-08-17T01:44:01Z) Search feature implemented (API integration + UI + pagination). Ready: test cases & docs. Key files: Services/GoogleBooks, Models/GoogleBooks, Pages/Books.cshtml(.cs)
- [Malta] (UTC 2026-08-16T22:46:25-03:00) Automated coverage for GoogleBooksService lives in GoogleBooksApp.Tests with a stub HttpMessageHandler, and the manual browser checklist lives at .squad/agents/malta/test-checklist.md.
- [Malta] (UTC 2026-08-18T21:30:00-03:00) Advanced filter tests (issue #5) written anticipatorily in GoogleBooksServiceFilterTests.cs. 18 test cases covering language filter, sort order, client-side year filtering, edge cases, and combined filters. Tests compile and run successfully - all 30 tests pass (10 existing + 20 new). Cinnamon's parallel implementation landed perfectly aligned with the expected contract. Test expectations documented in .squad/decisions/inbox/malta-advanced-filters-tests.md.
