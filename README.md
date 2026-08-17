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
- There is no test project in this workspace yet. When a test project (for example, "GoogleBooksApp.Tests") is added, run tests with:
  dotnet test

Contributing
------------
- Follow the coding standards recorded in .squad/decisions.md.
- Do not add secrets to appsettings.json. Use the user-secrets store or environment configuration for keys.

Contact / Help
--------------
If you need help, check the squad decisions and history files under .squad/ for implementation notes and contracts, or ask the project owner.

(Documentation last updated: 2026-08-16)

##Resune
copilot --resume=9671a713-a317-4bcc-93bb-118b87b21dc6