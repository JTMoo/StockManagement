# ADR-0003: FastEndpoints API + React, desktop-first

- Status: Accepted (desktop shell open)
- Date: 2026-09-24

## Context

- WPF: Windows only, no browser access, hard to test
- Goal: mainly desktop, also viewable in a browser, tested

## Options

- Keep WPF
- ASP.NET Core API (minimal APIs / MVC / FastEndpoints) + React
- Blazor

## Decision

- FastEndpoints (one endpoint per file with request, response, validator; feature folders)
- React + TypeScript, talks to the API only
- Desktop: local API + shell (Tauri / Electron / WebView2, open)

## Consequences

- Tests: `WebApplicationFactory` + Testcontainers; Vitest, RTL, Playwright
- CI can move to Linux once WPF is gone
- Localization keys shared between API and React
- WPF stays until React covers it
