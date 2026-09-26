# ADR-0013: Electron desktop shell

- Status: Proposed
- Date: 2026-09-26

## Context

- [ADR-0003](0003-fastendpoints-api-and-react-frontend.md): desktop-first, browser too; shell left open
- [ADR-0006](0006-react-web-app-shape.md): shell only has to open the API's URL (same origin, React build served from `wwwroot`)
- [ADR-0011](0011-retire-wpf-gui.md): WPF retired, no installable desktop app right now, browser-only
- Owner needs macOS support for the packaged desktop app, not just "a browser works" (2026-09-26)
- WebView2 was the first option considered but is Windows-only, ruling it out

## Options

- WebView2: thin, matches the team's .NET skills, but Windows-only — rejected
- Tauri: smallest install (~3-10MB), cross-platform, but adds a Rust toolchain the team doesn't have
- Electron: cross-platform, no new language (JS/TS, matches `StockManagement.Web`), heaviest install (~100MB+, bundles Chromium + Node)

## Decision

- New `StockManagement.Desktop` app: a minimal Electron main process that opens one `BrowserWindow` pointed at the API's URL (`STOCKMANAGEMENT_URL` env var, default `http://localhost:5080`)
- MVP scope only: the shell does not start or bundle the .NET API process — it expects the API to already be running, same as opening the app in a browser today. Whether/how to launch the API from the shell is a separate decision, not made here
- Packaging: `electron-builder`, per-OS targets (Windows NSIS, macOS dmg); own CI job (`ubuntu-latest` build check only — signed installers are a release-time concern, not CI)
- Lives alongside `StockManagement.Web`, not inside it — different runtime (Electron vs. browser), own `package.json`

## Consequences

- Cross-platform desktop entry point (Windows/macOS/Linux) with no backend changes
- Larger install than WebView2/Tauri would have been
- New Node-based build pipeline in CI, same shape as the existing `StockManagement.Web` job
- Until a follow-up decides otherwise, users still start the API manually before opening the desktop app
