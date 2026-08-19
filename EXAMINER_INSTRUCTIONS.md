Examiner instructions - running the project in a clean environment

1) Clone repository

- PowerShell (from workspace root):
- git clone https://github.com/nkuris/Munters.git Munters-clone

2) Open solution

- Open the solution file in Visual Studio or use dotnet commands:
- Start Visual Studio and open Munters-clone/Munters.slnx (or the .sln in the folder)
- or: dotnet restore Munters-clone

3) Add API key (local development / examiner environment)

Option A - per-project user-secrets (for .NET projects):
- cd into the project that requires the secret (example: Munters-clone/src/munters.server)
- dotnet user-secrets init    # if not initialized
- dotnet user-secrets set "MUNTERS:ApiKey" "YOUR_API_KEY"

Option B - environment variable (process or system):
- PowerShell (current session):  = 'YOUR_API_KEY'
- Persist for user: setx MUNTERS_API_KEY "YOUR_API_KEY"

4) Add API key to Docker Compose

Option A - environment variable in docker-compose.yml:

- Edit docker-compose.yml and add under the service:
  environment:
    - MUNTERS_API_KEY=${MUNTERS_API_KEY}

- Export the variable before running compose:  = 'YOUR_API_KEY'

Option B - Docker secrets (optional):

- Create a secret file: echo "YOUR_API_KEY" | Out-File -Encoding utf8 ./munters_api_key.txt
- Reference it in docker-compose.yml under secrets and service.secrets

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
