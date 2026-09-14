# 2026-09-14: Gate Azure deployments on passing non-integration tests

## Decision
- Add a required `dotnet test GoogleBooksApp.slnx --configuration Release --filter "Category!=Integration"` step to the dev, qas, and production deploy workflows.
- Keep the test step after build and before publish/deploy so any failing test stops the deployment immediately.
- Exclude `Category=Integration` tests from this gate because they depend on a live Google Books API key that is not part of the deployment workflows' CI environment.

## Why
- Deploying without any automated test gate makes CI/CD optimistic when it should be defensive.
- Running the solution test command keeps the workflow aligned with the repo structure and ensures the deploy gate covers the test project consistently.
- Filtering out integration tests keeps the gate deterministic and fast while still enforcing the unit-level safety net on every deployment path.
- No bypass (`continue-on-error`, conditional soft-fail, etc.) keeps failures loud and early, which is the correct default for deployment workflows.
