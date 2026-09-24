# ADR-0003: FastEndpoints REST API with a React frontend, desktop-first

- Status: Accepted (desktop shell still open)
- Date: 2026-09-24
- Deciders: Jonathan Trefz

## Context

The current app is WPF, so it runs only on Windows and only on the machine it is installed on. The owner wants to
keep it mainly a desktop application but make it viewable from a browser, with unit and integration tests covering
the codebase, and a base for ERP features.

## Options considered

1. **Keep WPF**: no rewrite, but no browser access and hard to test the UI.
2. **ASP.NET Core API + React frontend**: one UI for desktop and browser; API is testable end to end.
   For the API: minimal APIs, MVC controllers, or FastEndpoints.
3. **Blazor**: stays in C#, but the owner chose React.

## Decision

Option 2 with **FastEndpoints** for the API (REPR pattern: one endpoint per file with its request, response and
validator, grouped by feature folder) and **React + TypeScript** for the frontend. The frontend only talks to the
API; business logic stays behind it ([ADR-0002](0002-business-logic-in-domain-and-application.md)).

For desktop use, the API runs locally and a shell hosts the React app. Which shell (Tauri, Electron or WebView2) is
still an open question; see [../decisions.md](../decisions.md).

## Consequences

- API integration tests with `WebApplicationFactory` and a real MongoDB; frontend tests with Vitest and React
  Testing Library, Playwright for a few end-to-end flows.
- CI can move to Linux runners once WPF is retired; the installer changes with the desktop shell choice.
- Localization must be shared between API and React (one source of resource keys).
- The WPF app stays in place until the React frontend covers its features.
