Examiner instructions - running the project in a clean environment

1) Clone repository

- PowerShell (from workspace root):
- git clone https://github.com/nkuris/Munters.git Munters-clone

2) Open solution

- Open the solution file in Visual Studio or use dotnet commands:
- Start Visual Studio and open Munters-clone/Munters.slnx (or the .sln in the folder)
- or: dotnet restore Munters-clone

3) Add API key (local development / examiner environment)

The project expects the Giphy API key to be configured as the Giphy:ApiKey configuration value. You can provide this in several ways:

Option A - per-project user-secrets (recommended for local .NET development):
- cd into the Munters.Server project folder (example: Munters-clone/Munters.Server)
- dotnet user-secrets init    # if not already initialized
- dotnet user-secrets set "Giphy:ApiKey" "YOUR_API_KEY"

Option B - environment variable (for Docker Compose or system-level):
- Set the environment variable GIPHY__APIKEY to your key (double underscore maps to a colon in IConfiguration).
- Example (PowerShell, current session): $env:GIPHY__APIKEY = 'YOUR_API_KEY'
- To persist for the current user: setx GIPHY__APIKEY "YOUR_API_KEY"

Option C - docker-compose/local .env:
- Create a local .env file at the repository root (copy .env.example to .env) and set:
  GIPHY__APIKEY=YOUR_API_KEY
- docker compose will automatically load .env and pass the variable into the container.

4) Add API key to Docker Compose

The repository's docker-compose.yml reads the environment variable ${GIPHY__APIKEY} and passes it into the Munters.Server container as Giphy__ApiKey. Provide the value in one of these ways:

- Create a local .env (copy .env.example -> .env) and set GIPHY__APIKEY=YOUR_API_KEY. Do NOT commit .env.
- Create docker-compose.override.yml (not committed) and add the GIPHY__APIKEY under service.environment.
- Use your shell to export GIPHY__APIKEY before running docker compose.

5) Build & run (clean examiner flow)

- From repo root:
- dotnet restore
- dotnet build
- dotnet test

If using Docker Compose:
- Ensure the env var or secret file is present, then:
- docker compose up --build --abort-on-container-exit

6) Cleanup and notes

- Do not commit secret values. Remove temporary secret files: Remove-Item ./munters_api_key.txt -Force
- To clear user-secrets: dotnet user-secrets clear
- If a service cannot find the key, verify the variable name expected by the code and map it accordingly.

I can inspect the project and update this file with exact project paths and expected environment variable names if you want.
