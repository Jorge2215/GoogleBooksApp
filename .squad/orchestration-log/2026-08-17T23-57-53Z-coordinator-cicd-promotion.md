Coordinator orchestration log — 2026-08-17T23:57:53Z (UTC)

Summary:
- Committed and pushed UI changes (About modal, label fix, light theme) earlier.
- Re-triggered deploy workflows and confirmed Dev/QAS/Prd deployments succeeded.
- Configured Google Books API key as App Setting `GoogleBooks__ApiKey` on all three App Services.
- Opened and merged PRs: dev->qas (PR #3) and qas->main (PR #4); verified resulting deployments.
- Smoke-tested live QAS and PRD hostnames (HTTP 200).

Artifacts:
- Commit: a28a578 "Add about modal, label fix, and light theme"
- Deployed runs: deploy-dev run 32081816323, deploy-qas run 32082269298, deploy-prd run 32082343781
- App Service hostnames listed in decisions entry.

Coordinator: Scribe
Timestamp: 2026-08-17T23:57:53Z
