
## This project is the basis of a Stock Management Tool.

This basis has:
- API on PostgreSQL (EF Core); GUI still on MongoDB
- Seperated BusinessLogic from UI
- Creation of Stock Items
- Checking Stock Items in and out
- Create a transaction history of all those checkin /-outs
- Coming soon: Multilanguage support through settings

# For WiX Installer project to work::
1. WiX v3 Extension for VS: Extensions -> Manage Extensions... -> Browse -> install "WiX v3 - Visual Studio 2022 Extension"
2. Download (and install) WiX Toolset (Min v3.11) from: https://github.com/wixtoolset/wix3/releases
3. Restart Visual Studio (maybe PC).

Getting an Installer as follows:
1. Rebuild the Installer project with the desired configuration (Release x64 for example)
2. Output will be in the following folder: Inside the solution directory -> StockManagement.Installer -> Installer

# Mongo-DB Installer Download
https://www.mongodb.com/try/download/community

# Database config
- GUI `appsettings.json` next to the exe: `ConnectionStrings:Mongo`, `Mongo:DatabaseName`
- API `appsettings.json`: `ConnectionStrings:Postgres`
- Per install: `appsettings.local.json` with the keys to override (kept on upgrade)

# PostgreSQL (ADR-0008)
- API's data layer is EF Core on PostgreSQL (StockItem, Customer, Invoice, Transaction); Mongo removed from the API
- Migrations run automatically on API start (`Database.MigrateAsync()`)
- API integration tests need Docker (Testcontainers `postgres:16`)
- GUI still reads/writes MongoDB directly, unchanged (not yet cut over)

# Authentication (ADR-0010)
- Every API endpoint needs a JWT bearer token except `POST /api/auth/login`
- Fresh database: the `AddUsers` migration seeds one admin user (`admin` / `ChangeMe123!`) - no self-service change yet, change it directly in the database if that matters to you
- Config: `Jwt:SigningKey` (override per install in `appsettings.local.json`, see above), `Jwt:ExpiryHours`
- Local dev / CI: `StockManagement.Api.Tests` logs in via `ApiFactory.CreateAuthenticatedClientAsync()`; `npm run e2e` logs in as the seeded admin at the start of the spec; the React app shows a login screen until you log in

# MongoDB (GUI only)
- Still used by the WPF GUI: `User`/`Settings` and, until the GUI is cut over, its own copy of Stock/Customer/Sale data

# MongoDB replica set (ADR-0007, superseded by ADR-0008 for the API)
- Was needed for the API's multi-step sale transaction; that's now the Postgres transaction in `EfInvoiceServiceProvider`
- Still applies to the GUI, which is unchanged and needs it for the same reason
- `mongod.cfg` (Windows: `C:\Program Files\MongoDB\Server\<version>\bin\mongod.cfg`):
```yaml
replication:
  replSetName: rs0
```
- Restart the `MongoDB` service
- Once: `mongosh --eval "rs.initiate()"`
- Docker: `docker run -d -p 27017:27017 mongo:7 --replSet rs0` + `docker exec <id> mongosh --eval "rs.initiate()"`
