# Squad Prompt — Book Search Web App

## Goal
Design and implement a web application in .NET 10 (Razor Pages) that allows users to search for books by title and author using the Google Books API (https://www.googleapis.com/books/v1).

## Architecture
- ASP.NET Core Razor Pages project.
- API Key stored in appsettings.json under "GoogleBooks".
- Use IHttpClientFactory for API calls.
- Models: BookResult, VolumeInfo, ImageLinks.
- Razor Page: Books.cshtml with a search form.
- Display results: title, authors, thumbnail cover, short description, and preview link.

## Squad Roles
- **Cinnamon (Backend Developer):** Implements API integration, defines models and services.
- **Creta (Frontend Developer):** Designs the UI for search and results, ensuring thumbnails and links are displayed.
- **Malta (Tester):** Validates queries by title and author, tests error cases (no results, missing images).
- **Toru (Architect):** Defines project structure, configuration strategy, and coding standards.
- **Nutmeg (Documenter):** Creates and mantains the application technical and functional documentation


## Features
- Search by title (e.g., "War and Peace").
- Search by author (e.g., "Haruki Murakami").
- Display book covers and preview links.
- Handle multiple results with basic pagination.
- Clear messages when no results or missing data.

## Success Criteria
- Application compiles and runs locally.
- API Key is correctly read from configuration.
- Search form supports both title and author queries.
- Results show covers, authors, and preview links.
- Documentation explains setup and usage clearly.
