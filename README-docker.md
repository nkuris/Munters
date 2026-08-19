Run the Munters solution using Docker

Prerequisites:
- Docker Desktop installed and running
- Optional: a GIPHY API key if you want actual GIFs

Quick steps:
1. Copy .env.example to .env and set your GIPHY key (or use the provided script):
   - copy .env.example .env
   - edit .env and set GIPHY__APIKEY

2. From repository root run (PowerShell):
   .\run-docker.ps1

3. Wait for build and services to start.
   - Server: http://localhost:8080
   - Client (nginx): http://localhost:3000

4. To stop and remove containers:
   docker compose down

Notes:
- The docker-compose.yml includes two services (munters.server and munters.client) on a shared network. The client nginx proxies /api/ to the server by service name (http://munters.server:8080).
- CORS configured via Cors__AllowedOrigins environment variable or Cors:AllowedOrigins in appsettings. See .env.example.

If you want a background run: docker compose up --build -d

If you prefer to run without Docker: run the server with dotnet and client with npm as documented earlier.
