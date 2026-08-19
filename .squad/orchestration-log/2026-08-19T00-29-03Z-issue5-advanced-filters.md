Timestamp: 2026-08-19T00:29:03Z
Issue: #5 — Advanced search filters (language, year range, sort)

Summary:
- Agents: Cinnamon (backend) and Malta (tests) worked in parallel; Creta (UI) followed and integrated frontend changes.
- Cinnamon added PublishedDate/Language, extended IGoogleBooksService.SearchAsync with language/sortOrder/yearFrom/yearTo; language+sortOrder sent to API; year range filtered client-side (API lacks server-side year range), TotalItems becomes approximate when year filter active.
- Malta added GoogleBooksServiceFilterTests.cs (20 tests); total 30 tests pass.
- Creta added advanced filters UI (details section), preserved filters in pagination, and added disclaimer.

Artifacts created:
- .squad/orchestration-log/2026-08-19T00-29-03Z-issue5-advanced-filters.md
- .squad/log/2026-08-19T00-29-03Z-issue5-advanced-filters.md
- Merged inbox decision files into .squad/decisions.md

Notes:
- Decision inbox files were consolidated and removed from .squad/decisions/inbox.
- Build: 0 errors/warnings; Tests: 30/30 passed.

Author: Scribe
