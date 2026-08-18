Objective:
Configure branch protection policies and GitHub Actions workflows for the Book Search App 
that consumes the Google Books API. The app has three environments in Azure App Service:
- GoogleBooksDev
- GoogleBooksQas
- GoogleBooksPrd

Repository Branches:
- dev
- qas
- main

(Remote Repository is: https://github.com/Jorge2215/GoogleBooksApp.git)

Tasks:

1. Branch Policies:
   - Configure GitHub branch protection so that:
     - qas and main branches do NOT allow direct push.
     - Updates to qas and main must be done via Pull Request + Merge.
     - dev branch allows direct push (no restrictions).

2. GitHub Actions Workflows:
   - Create three deployment workflows in `.github/workflows/`.

   a) Dev Deployment:
      - Workflow triggered manually (workflow_dispatch).
      - Deploys to Azure App Service: GoogleBooksDev.
      - Uses publishing profile stored in GitHub Secrets.

   b) QAS Deployment:
      - Workflow triggered automatically on Pull Request + Merge into qas branch.
      - Deploys to Azure App Service: GoogleBooksQas.
      - Uses publishing profile stored in GitHub Secrets.

   c) Production Deployment:
      - Workflow triggered automatically on Pull Request + Merge into main branch.
      - Deploys to Azure App Service: GoogleBooksPrd.
      - Uses publishing profile stored in GitHub Secrets.

3. Secrets Management:
   - Store Azure publishing profiles securely in GitHub Secrets:
     - AZURE_WEBAPP_PUBLISH_PROFILE_DEV
     - AZURE_WEBAPP_PUBLISH_PROFILE_QAS
     - AZURE_WEBAPP_PUBLISH_PROFILE_PRD
   - Reference these secrets in each workflow for authentication.

Deliverables:
- Branch protection rules applied in GitHub.
- Three workflow YAML files in `.github/workflows/`.
- Verified deployments to Dev, QAS, and Production App Services in Azure.
