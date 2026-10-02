# ADR-0030: Frontend testing strategy

- Status: Accepted
- Date: 2026-10-02

## Context

- #109: document the existing frontend test stack, confirm it runs in CI, set conventions so [ADR-0025](0025-kora-design-system.md)'s restyle (#105) doesn't silently break tests
- Already in place, not proposed: Vitest + React Testing Library for unit/component, Playwright for e2e ([[react-web-app]])
- Owner (2026-10-02): keep this stack, write the ADR, verify CI coverage, fill gaps in critical flows (auth, import, invoices)

## Options

- Keep Vitest/RTL/Playwright vs. switch (Jest, Cypress) — no reported pain with the current stack, switching is pure churn
- Query style: `getByRole`/`getByLabelText` (resilient to CSS/markup changes) vs. `getByTestId` (resilient to text changes, blind to a11y regressions) — role/label already used throughout

## Decision

- Keep Vitest + RTL for unit/component tests, Playwright for e2e. No new tooling.
- Levels:
  - Unit: pure functions (`src/api/*.test.ts` — request/response shaping, no DOM)
  - Component: one `*.test.tsx` per component under test, RTL + `jsdom`, API calls mocked via `mockApi`/`test-utils.tsx` (`src/test-setup.ts`)
  - E2E: Playwright against the real API + a real Postgres (`StockManagementE2E`), full build served from `wwwroot` (`playwright.config.ts`) — reserved for flows that cross the network boundary, not a restatement of component tests
- Query convention (already followed): `getByRole`/`getByLabelText`/`getByText` first — breaks on an a11y regression, survives a class/markup restyle. `data-testid`/`getByTestId` only as fallback for a node with no accessible role/name (e.g. `InvoiceView`'s total cell, cancelled badge) — never CSS selectors. #105's Dialog/list rework must not touch test files to stay green.
- `describe`/`it` naming and Arrange/Act/Assert: same convention as `StockManagement.Tests` (CLAUDE.md "Tests").
- CI (`web-tests` job, `Integration.yml`): `npm test` (Vitest, all component+unit) then `npm run e2e` (Playwright, builds first) on every PR and push to `main`. Confirmed running, not just configured.

## Consequences

- No coverage threshold enforced (no `--coverage` gate) — CLAUDE.md's "unit test every rule moved out of the UI" is the bar, not a %; revisit if gaps recur
- E2E stays minimal by design (one spec today, `sale.spec.ts`) — slow and flaky-prone; new e2e specs only for flows RTL can't reach (real auth round-trip, real DB import commit)
- `getByTestId`/`data-testid` used where a role/label query would work is a bug against this ADR
