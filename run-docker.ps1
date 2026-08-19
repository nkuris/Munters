# Run the full Munters docker environment (build & run)
# Usage: Open PowerShell in repo root and run: .\run-docker.ps1

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Definition
$envFile = Join-Path $scriptDir '.env'
$exampleFile = Join-Path $scriptDir '.env.example'

if (-not (Test-Path $envFile)) {
	if (Test-Path $exampleFile) {
		Copy-Item -Path $exampleFile -Destination $envFile
		Write-Host "Created .env from .env.example. Please edit .env to add your real GIPHY__APIKEY if required."
	} else {
		Write-Host "No .env or .env.example found. Creating empty .env. Please edit it to add required variables."
		"GIPHY__APIKEY=your_giphy_api_key_here" | Out-File -FilePath $envFile -Encoding utf8
	}
}

# Check for placeholder key
$envText = Get-Content $envFile -Raw
if ($envText -match 'GIPHY__APIKEY\s*=\s*your_giphy_api_key_here') {
	Write-Host "WARNING: .env contains the placeholder GIPHY__APIKEY. The server may not return GIFs without a valid key." -ForegroundColor Yellow
	$answer = Read-Host "Press Enter to continue anyway, or Ctrl+C to abort and edit .env"
}

Write-Host "Starting docker compose (will build images). This may take a few minutes..."
# Use docker compose (v2+) command
docker compose up --build

# To run in background, use: docker compose up --build -d

