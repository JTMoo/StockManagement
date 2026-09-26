# Decisions

Read before an important decision. Update in the same PR.

## ADRs

| ADR | Decision | Status |
|---|---|---|
| [0001](adr/0001-record-architecture-decisions.md) | Decisions live in the repo as ADRs | Accepted |
| [0002](adr/0002-business-logic-in-domain-and-application.md) | Business logic in Domain and Application | Accepted |
| [0003](adr/0003-fastendpoints-api-and-react-frontend.md) | FastEndpoints API + React, desktop-first | Accepted |
| [0004](adr/0004-api-host-shape-and-tests.md) | API routes, errors, config, integration tests | Proposed |
| [0005](adr/0005-manufacturer-as-free-text.md) | Manufacturer as free text, old enum numbers read as names | Proposed |
| [0006](adr/0006-react-web-app-shape.md) | Vite + React web app served by the API, resx texts, Vitest + Playwright | Proposed |
| [0007](adr/0007-mongo-replica-set-and-transactions.md) | MongoDB single-node replica set, multi-step writes in transactions | Superseded by 0008 |
| [0008](adr/0008-ef-core-on-postgresql.md) | EF Core on PostgreSQL, one `AppDbContext`, change handlers | Proposed |
| [0009](adr/0009-web-stock-item-excel-import.md) | Web Excel import: auto-matched headers, one upload-and-import call | Proposed |
| [0010](adr/0010-jwt-authentication.md) | JWT bearer authentication | Proposed |
| [0011](adr/0011-retire-wpf-gui.md) | Retire the WPF GUI, browser-only for now | Proposed |
| [0012](adr/0012-stock-check-in-check-out.md) | Stock check-in/check-out: `Transaction.Reason`, conditional-update check-out | Proposed |
| [0013](adr/0013-electron-desktop-shell.md) | Electron desktop shell, opens the API's URL | Proposed |
| [0014](adr/0014-decimal-money-type.md) | `decimal` for money, no separate Money type | Proposed |
| [0017](adr/0017-roles-and-permissions.md) | Roles (Admin/Standard) + per-module Read/Write permission strings | Proposed |
| [0018](adr/0018-generic-import-pipeline.md) | Generic import pipeline: batches, preview → commit, undo | Proposed |

## Standing decisions

- Update by `Id`, never by business key ([#32](https://github.com/JTMoo/StockManagement/pull/32))
- Cultures: de-DE, en-US, es-PY; all UI text via resources
- Import: code repeated in one file → keep first, rest reported as duplicates (owner, 2026-09-24)

## Open

| Question | Options |
|---|---|
| Sequential numbers | `$inc` counter / `Max + 1` |
| `#region` + banners in new code | keep / drop |
| Nullable warnings as errors | yes / no |
| Format check in CI | `dotnet format --verify-no-changes` + `.editorconfig` naming rules / none |
