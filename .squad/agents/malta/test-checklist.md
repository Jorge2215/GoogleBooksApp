# Malta Manual Test Checklist — 2026-08-16

## Browser/UI checks

- Search by title only and confirm expected results appear with title, author text, description text, and Google Books link.
- Search by author only and confirm the query stays in the URL and results update correctly.
- Search by title + author together and confirm both values persist across pagination links.
- Use a result with no cover image or break image loading in the browser and confirm the "No cover available" fallback stays visible without layout collapse.
- Use a result with no description and confirm the UI shows the fallback copy instead of blank space.
- Use a search with no matches and confirm the empty/error state message renders once and is easy to spot.

## Pagination checks

- Confirm the first results page disables Previous and only enables Next when more than one page exists.
- Move forward until the last page and confirm Next becomes disabled at the boundary.
- Navigate back from page 2+ and confirm Previous returns to the earlier page without losing active filters.
- Try a result count that is not a multiple of 10 and confirm the last page does not show duplicate or skipped items.

## Responsive checks

- At narrow/mobile width, confirm cards stack vertically, text remains readable, and controls do not overflow.
- Confirm the search form fields remain usable on small screens and the Search button stays visible.
