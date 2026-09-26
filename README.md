
## This project is the basis of a Stock Management Tool.

This basis has:
- FastEndpoints API on PostgreSQL (EF Core) + a React web app (`StockManagement.Web`), served by the API
- Seperated BusinessLogic from UI
- Creation of Stock Items
- Checking Stock Items in and out
- Create a transaction history of all those checkin /-outs
- Multilanguage support through settings

# Quick start
- PostgreSQL running locally on `127.0.0.1:5432`, user/password `postgres`/`postgres` (or override, see below)
- macOS / Linux: `./scripts/quickstart.sh`
- Windows: `./scripts/quickstart.ps1`
- Checks prerequisites, restores the .NET solution, installs the web app's dependencies, then prints the two commands to run the API and the web app

# Database config
- API `appsettings.json`: `ConnectionStrings:Postgres`
- Per install: `appsettings.local.json` with the keys to override (kept on upgrade)

# PostgreSQL (ADR-0008)
- API's data layer is EF Core on PostgreSQL (StockItem, Customer, Invoice, Transaction, Settings, User)
- Migrations run automatically on API start (`Database.MigrateAsync()`)
- API integration tests need Docker (Testcontainers `postgres:16`)

# Authentication (ADR-0010)
- Every API endpoint needs a JWT bearer token except `POST /api/auth/login`
- Fresh database: the `AddUsers` migration seeds one admin user (`admin` / `ChangeMe123!`) - no self-service change yet, change it directly in the database if that matters to you
- Config: `Jwt:SigningKey` (override per install in `appsettings.local.json`, see above), `Jwt:ExpiryHours`
- Local dev / CI: `StockManagement.Api.Tests` logs in via `ApiFactory.CreateAuthenticatedClientAsync()`; `npm run e2e` logs in as the seeded admin at the start of the spec; the React app shows a login screen until you log in

# Desktop app (ADR-0013)
- `StockManagement.Desktop`: Electron shell, opens the API's URL in a native window (Windows/macOS/Linux)
- Start the API first, then: `cd StockManagement.Desktop && npm ci && npm start`
- Override the target URL with the `STOCKMANAGEMENT_URL` env var (default `http://localhost:5080`)
- `npm run build` packages an installer (`electron-builder`); the shell doesn't start the API itself yet
