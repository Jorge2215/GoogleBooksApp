# Squad Decisions

## Active Decisions

### 2026-09-02T14:01:26-03:00: User directive
**By:** Jorgito (via Copilot)
**What:** Keep only dev, qas, and main branches active in the repo — feature/work branches should be merged and deleted after merge.
**Why:** User request — captured for team memory

### 2026-09-14T20:23:09-03:00: Gate Azure deployments on passing non-integration tests
**By:** Malta, Toru
**What:** Add a required `dotnet test GoogleBooksApp.slnx --configuration Release --filter "Category!=Integration"` step to `.github/workflows/deploy-dev.yml`, `.github/workflows/deploy-qas.yml`, and `.github/workflows/deploy-prd.yml`, after build and before publish/deploy, with no soft-fail or bypass behavior.
**Why:** Malta confirmed the deploy workflows were not test-gated at all. Toru's fix makes each Azure deployment fail fast on regressions while excluding integration tests that require a live Google Books API key unavailable in CI.

## Governance

- All meaningful changes require team consensus
- Document architectural decisions here
- Keep history focused on work, decisions focused on direction
