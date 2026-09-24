# ADR-0004: API host shape and integration tests

- Status: Proposed
- Date: 2026-09-24

## Context

- First API slice on top of [ADR-0003](0003-fastendpoints-api-and-react-frontend.md)
- Desktop (local API) and browser reach the same endpoints
- Testcontainers needs Linux containers; `windows-latest` runners have none

## Options

- Tests: in `StockManagement.Tests` on Windows / own project + Linux job / in-memory fake DB
- Errors: text messages / error codes + status codes
- Keys in routes: Mongo `Id` / business key

## Decision

- `StockManagement.Api`, routes under `/api`, plural kebab-case: `/stock-items/{code}`, `/customers/{customerId}`, `/invoices/{number}`, `POST /sales`
- Reads: Kernel `*ServiceProvider`; writes: Core services only
- `Request` / `Response` records per feature folder, no persistence models on the wire
- Status: 400 validation (ProblemDetails, error codes like `customerNotFound`, no UI text), 404 unknown key, 409 business conflict with typed body
- JSON: camelCase, enums as strings
- Config: `ConnectionStrings:Mongo`, `Mongo:DatabaseName`; unique indexes created at startup
- No auth yet (`AllowAnonymous`); local only
- Tests: `StockManagement.Api.Tests`, one Mongo container per run, one database per test, own CI job on `ubuntu-latest`

## Consequences

- React maps error codes to its own localized text
- Updates by `Id` need `BaseDocument.Id` exposed first (internal today)
- Default database name differs from the desktop's; set `Mongo:DatabaseName` to share data
- Auth before any non-local deployment
