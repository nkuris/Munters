Munters - Back-End C# Developer Exam
===================================

Overview
--------
This repository implements the Munters exam: a small .NET 10 backend that integrates with the Giphy API and a React frontend that consumes the backend. The app provides two HTTP endpoints:

- GET /api/giphy/trending — returns trending GIFs
- GET /api/giphy/search?q={term} — searches GIFs by query

Both endpoints return simple DTOs with GIF URLs. A caching wrapper prevents redundant calls to the Giphy API.

Quick start (Docker)
--------------------
1. Copy example env and set your API key:

   copy .env.example .env
   (edit .env and set GIPHY__APIKEY)

2. Build and run the stack (repo root):

   docker compose up --build

   or use the helper:
   .\run-docker.ps1

3. Open the UI: http://localhost:3000
   Server API: http://localhost:8080/api/giphy/trending

Project structure (implementation highlights)
--------------------------------------------

Server (Munters.Server)
- Program.cs
  - Configuration, logging, CORS policy and typed HttpClient registration. CORS allowed origins are configurable via Cors:AllowedOrigins or Cors__AllowedOrigins env var.
- Controllers/GiphyController.cs
  - Exposes the two endpoints (/trending and /search). Handles exceptions and logs errors.
- Services/GiphyApiClient.cs
  - Low-level client that calls Giphy developer API (v1/gifs/trending and v1/gifs/search). Uses a typed HttpClient and maps responses to GifResultDto.
- Services/CachedGiphyService.cs
  - A decorator around IGiphyService that adds caching using IMemoryCache. This is the primary cache wrapper used to avoid redundant calls to Giphy.
  - Cache keys are normalized (search terms trimmed + lowercased) and include the limit parameter.
  - Cache duration is configurable via Giphy:CacheDurationMinutes.
- Models/GiphyOptions.cs
  - Strongly typed options for ApiKey, BaseUrl and CacheDurationMinutes.

Client (munters.client)
- src/App.jsx
  - Simple React UI that calls the backend endpoints (trending & search), shows results, and displays errors and loading state.
- Dockerfile + nginx.conf
  - Production client is served via nginx and proxies /api/* to the server service inside docker-compose.

Caching (important)
-------------------
The cache is implemented as a wrapper (CachedGiphyService) that decorates the actual GiphyApiClient. Benefits:

- Separation of concerns: API client focuses on fetching and mapping data; caching concerns are isolated in the decorator.
- Safe concurrent population: uses IMemoryCache.GetOrCreateAsync to ensure only one fetch populates a cache entry.
- Easy to test: the decorator implements IGiphyService so it can be mocked or replaced in tests.

Scaling note
------------
IMemoryCache is an in-memory cache suitable for single-instance deployments and the scope of this exam. For horizontal scaling (multiple server instances) switch to a distributed cache (example: IDistributedCache using Redis):

- Replace IMemoryCache with IDistributedCache (or use a hybrid approach).
- Use a Redis instance and configure the cache duration and eviction policies appropriately.
- Keep the same cache key strategy (normalized keys) so different instances share the same keys.

Testing and validation
----------------------
- Manual: run the stack and use the UI to execute Trending and Search operations.
- API tests: curl or Invoke-RestMethod against http://localhost:8080/api/giphy/search?q=cat

Notes & recommendations
-----------------------
- I did not commit the api_key. .env is local only. The repository includes .env.example.
- The project uses a decorator pattern and DI to keep code modular and maintainable.
- Optional improvements (if you have time): add unit tests for CachedGiphyService, add retry/Polly policies for transient network errors, add paging support.

Files of interest
-----------------
- Munters.Server/Controllers/GiphyController.cs
- Munters.Server/Services/GiphyApiClient.cs
- Munters.Server/Services/CachedGiphyService.cs
- Munters.Server/Models/GiphyOptions.cs
- Munters.Server/Program.cs
- munters.client/src/App.jsx
- munters.client/Dockerfile and munters.client/nginx.conf
- docker-compose.yml, run-docker.ps1, .env.example

