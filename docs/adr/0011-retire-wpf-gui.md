# ADR-0011: Retire the WPF GUI

- Status: Proposed
- Date: 2026-09-25

## Context

- [ADR-0003](0003-fastendpoints-api-and-react-frontend.md): FastEndpoints API + React, desktop-first, shell open
- [analysis/react-parity-gaps.md](../../analysis) (project files): parity gap list; all items closed (#72, #73, #74, #75, #77, #78)
- `StockManagement.Gui` (WPF) still read/wrote MongoDB directly via Kernel `*ServiceProvider` classes with no API endpoint; `StockManagement.Web` + `StockManagement.Api` are EF Core on PostgreSQL
- `StockManagement.Installer` (WiX) and `Delivery.yml` packaged `StockManagement.Gui.exe` as the only desktop entry point
- Open question in `docs/decisions.md`: desktop shell (Tauri / Electron / WebView2), unresolved

## Options

- Keep WPF until a desktop shell replaces it (blocks retiring the Mongo-only code paths indefinitely)
- Retire WPF now, ship browser-only, decide the desktop shell separately
- Retire WPF now and build a desktop shell (WebView2/Tauri/Electron) in the same change

## Decision

- Remove `StockManagement.Gui`, `StockManagement.Installer`, and the GUI-only Mongo `Kernel` code (`DatabaseManager`, `*ServiceProvider` Mongo implementations, `MainManager`, `Commands/`, `Diagnostics/`, `ReflectionManager`) that had no other caller
- Keep `BaseDocument`, `ManufacturerSerializer`, `Model/StockItem` and the `MongoDB.Driver` package reference in `StockManagement.Kernel` — the EF Core entity still reuses this POCO and its Bson attributes are harmless there; decoupling it from Mongo attributes is a separate cleanup, not part of this removal
- Drop the Windows/WPF build job from `Integration.yml` and delete `Delivery.yml` (nothing left to package); unit tests now run on `ubuntu-latest`
- Ship browser-only for now; the desktop-shell choice stays open and is tracked separately, not decided by this ADR

## Consequences

- No installable desktop app until a shell is chosen; the app is used from a browser against the API
- CI no longer needs `windows-latest` runners or MSBuild/WiX tooling
- MongoDB is no longer used anywhere in this repo; `README.md` MongoDB setup sections removed
- Any future desktop shell only needs to open the API's URL (per ADR-0006)
