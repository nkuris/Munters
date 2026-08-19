# React + Vite

This template provides a minimal setup to get React working in Vite with HMR and some ESLint rules.

Currently, two official plugins are available:

- [@vitejs/plugin-react](https://github.com/vitejs/vite-plugin-react/blob/main/packages/plugin-react) uses [Oxc](https://oxc.rs)
- [@vitejs/plugin-react-swc](https://github.com/vitejs/vite-plugin-react/blob/main/packages/plugin-react-swc) uses [SWC](https://swc.rs/)

## React Compiler

The React Compiler is not enabled on this template because of its impact on dev & build performances. To add it, see [this documentation](https://react.dev/learn/react-compiler/installation).

## Expanding the ESLint configuration

If you are developing a production application, we recommend using TypeScript with type-aware lint rules enabled. Check out the [TS template](https://github.com/vitejs/vite/tree/main/packages/create-vite/template-react-ts) for information on how to integrate TypeScript and [`typescript-eslint`](https://typescript-eslint.io) in your project.

Local tests and Giphy API key

If you run local tests or start the application locally and the server requires a Giphy API key, provide your key using one of the options below. Do NOT commit secret values.

1) Set environment variable (recommended for Docker/local runs)

PowerShell (current session):

$env:GIPHY__APIKEY = 'YOUR_API_KEY'

To persist for the user: setx GIPHY__APIKEY "YOUR_API_KEY"

2) Use dotnet user-secrets (recommended for .NET local development):

cd ..\Munters.Server
dotnet user-secrets init
dotnet user-secrets set "Giphy:ApiKey" "YOUR_API_KEY"

3) Change the API key directly in docker-compose (not recommended to commit):

- Open docker-compose.yml
- Under services -> munters.server -> environment replace the Giphy__ApiKey line with:

  - Giphy__ApiKey=YOUR_API_KEY

Or create a docker-compose.override.yml (uncommitted) that sets the environment variable for local runs.
