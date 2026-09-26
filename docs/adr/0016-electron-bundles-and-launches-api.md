# ADR-0016: Electron shell bundles and launches the API

- Status: Proposed
- Date: 2026-09-26

## Context

- [ADR-0013](0013-electron-desktop-shell.md) left "launch the API from the shell" as a separate decision; MVP required the API already running
- Customers (ERP goal) shouldn't need the .NET SDK/runtime or a manual `dotnet run` step
- Postgres itself stays a separate service either way (no in-process embedded Postgres exists for .NET); quickstart scripts already assume it's running locally

## Options

- Framework-dependent publish, spawn via installed `dotnet` runtime: smallest package, but requires the .NET runtime pre-installed — same friction as today
- Self-contained publish per OS/arch (`dotnet publish -r <RID> --self-contained`), bundled as Electron `extraResources`, spawned as a child process: larger install, zero runtime dependency
- Keep MVP behavior (user starts API manually): simplest, but leaves the "desktop app" unable to just open and work

## Decision

- `StockManagement.Desktop` bundles a self-contained `StockManagement.Api` publish for the target OS/arch as `extraResources` (`resources/api/`)
- On `app.whenReady`, the main process spawns `resources/api/StockManagement.Api[.exe]` as a child process, polls `GET /health` until it answers (or a timeout), then opens the window; the child is killed on `before-quit`
- Packaged builds only: in `npm start` (dev, unpackaged) the shell does not spawn the API — same as ADR-0013's MVP, dev keeps using `dotnet run` per the quickstart scripts
- Postgres is still external and unmanaged by the shell; `ConnectionStrings:Postgres` keeps its `127.0.0.1:5432` default (matches `scripts/quickstart.*`) — customers still need Postgres reachable, tracked separately
- Publishing per RID happens in a Node script (`scripts/publish-api.js`) invoked before `electron-builder`, one RID per host OS: `win-x64`, `osx-x64`/`osx-arm64`, `linux-x64`

## Consequences

- Packaged desktop app is a true double-click launch (API + window), closer to the ERP "easy to start using" goal
- Package size grows substantially (self-contained .NET runtime per OS, ~70-100MB) on top of Electron/Chromium
- Release CI needs one job per OS (can't cross-publish self-contained builds reliably) instead of the single `ubuntu-latest` build check
- Postgres is still a manual prerequisite; bundling/auto-provisioning it is out of scope here
