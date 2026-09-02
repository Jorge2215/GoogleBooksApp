GoogleBooksApp
==============

Project overview
----------------
GoogleBooksApp is a small ASP.NET Core (Razor Pages) app that lets you search the Google Books API by title and/or author and browse results with basic pagination.

Prerequisites
-------------
- .NET 10 SDK (install from https://dotnet.microsoft.com/)

Quick setup
-----------
1. Clone or open the repository and change directory into the project root:
   - git clone <repo-url>
   - cd GoogleBooksApp

2. Obtain a Google Books API key:
   - In the Google Cloud Console, create or select a project, enable the "Books API" (Google Books API), and create an API key under Credentials. Do not paste the key into source control.

3. Configure your local API key (safe, per-project local secrets):
   - From the project directory run:
     dotnet user-secrets set "GoogleBooks:ApiKey" "YOUR_KEY"
   - This project already includes a UserSecretsId in the .csproj, so the above command will store the key for local development. Replace YOUR_KEY with the API key you created.

4. Run the app:
   - dotnet run
   - Open https://localhost:5001 (or the URL printed by dotnet run) and navigate to the "Book Search" page.

Security note
-------------
Do not commit API keys or other secrets to the repository. Use dotnet user-secrets for local development and environment variables or a secrets manager for deployed environments.

Functional usage
----------------
- Search by title: enter a title (e.g. "Dune") into the Title field and press Search.
- Search by author: enter an author name (e.g. "Toni Morrison") in the Author field and press Search.
- Combined search: fill both fields to narrow results by title and author.
- Pagination: results are shown in pages of 10 (PageSize = 10). Use the Previous/Next controls to move between pages. The application uses a zero-based PageIndex (pageIndex query parameter) so the first page is pageIndex=0.

Empty / error states
--------------------
- If the page is submitted with no title or author the app surface a user-friendly message (e.g. "Enter a title or author to search.").
- If a search runs but no matches are found, the UI shows a no-results message (e.g. "No books matched your search." or the page may render a fallback "No results found for your search.").
- On upstream/API failures the app surfaces a safe error message to the user and logs details server-side.

Project structure
-----------------
- Configuration/GoogleBooksOptions.cs — configuration binding for the GoogleBooks section.
- Models/GoogleBooks/ — DTOs for the Google Books API (BookResult, VolumeInfo, ImageLinks, etc.).
- Services/GoogleBooks/ — IGoogleBooksService and the HTTP client implementation that talks to the Google Books API.
- Pages/Books.cshtml and Pages/Books.cshtml.cs — Razor Page UI and page model for searching and displaying results.
- Program.cs — application startup, DI, and typed HttpClient registration (base address: https://www.googleapis.com/books/v1/).

Testing
-------
- Fast suite (default):
  dotnet test GoogleBooksApp.slnx --nologo

- Real Google Books API integration tests:
  1. Set an API key in your environment, for example:
     setx GoogleBooks__ApiKey "YOUR_KEY"
  2. Open a new shell.
  3. Run:
     dotnet test GoogleBooksApp.slnx --nologo --filter Category=Integration

- Integration tests are marked with `Category=Integration`, run serially to keep call volume low, and skip automatically when no API key is configured in the environment.

Contributing
------------
- Follow the coding standards recorded in .squad/decisions.md.
- Do not add secrets to appsettings.json. Use the user-secrets store or environment configuration for keys.

Contact / Help
--------------
If you need help, check the squad decisions and history files under .squad/ for implementation notes and contracts, or ask the project owner.

(Documentation last updated: 2026-08-16)

##Prueba de proteccion de ramas en Github
git checkout qas
git branch
echo "Prueba branch protection" > prueba.txt
git add prueba.txt
git commit -m "Prueba de branch protection"
git push origin qas
(El push debe ser rechazado)

##Para dejar la rama como estaba antes de la Prueba	
git reset --hard HEAD~1
git status

##Comandos Pull Request y Merge
##Versión Completa:
# Crear PR QAS -> MAIN
gh pr create --base main --head qas --title "Prueba de aprobación" --body "Solicitud de promoción de cambios de QAS a Main"
O 
gh pr create
Y completar los campos de forma interactiva

# Ver PRs abiertos
gh pr list

# Luego de la Aprobacion
Se puede aprobar con linea de comando con:
gh pr review <numero_PR> --approve

# Merge
gh pr merge <numero_PR> --merge

##Resume
copilot --resume=3a51757e-8b78-4fbe-ac54-2049f89b875a