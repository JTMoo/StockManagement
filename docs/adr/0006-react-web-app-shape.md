# ADR-0006: React web app shape

- Status: Proposed
- Date: 2026-09-24

## Context

- [ADR-0003](0003-fastendpoints-api-and-react-frontend.md): React + TypeScript, desktop-first, browser too; shell open
- [ADR-0004](0004-api-host-shape-and-tests.md): API under `/api`, error codes, no auth, local only
- Texts live in `StockManagement.Language` resx (de-DE, en-US, es-PY)

## Options

- Build: Vite / Next.js / Create React App (deprecated)
- Hosting: own dev server + CORS / API serves the build (same origin)
- Client: hand-written typed `fetch` / generated from OpenAPI (not exposed yet)
- Texts: copy into JSON / generate JSON from resx / i18next
- Navigation, state, UI kit: router, state library, component library / none yet
- Tests: Vitest + RTL + Playwright / Jest + Cypress

## Decision

- `StockManagement.Web`: Vite + React + TypeScript (strict), static SPA, no SSR
- Relative `/api` only; dev: Vite proxy to `localhost:5080`; build: into `StockManagement.Api/wwwroot`, API serves it
- Desktop shell (still open) only has to open the local API URL
- `src/api.ts`: hand-written types + `fetch`; expected failures as `Result` (`notFound`, `invalid` codes, `conflict`, `unexpected`), no throws
- Texts: `scripts/resx-to-json.mjs` generates `src/i18n/*.json` from resx before dev/build/test (gitignored); keys = resx names; API error codes = text keys
- No router, state library or UI kit until a screen needs one
- Feature folders like the API: `src/features/<feature>/`
- Tests: Vitest + React Testing Library with stubbed `fetch`; Playwright smoke test against the real API + MongoDB (seeded, own database); own CI job on `ubuntu-latest`

## Consequences

- One process, one origin: no CORS, same URL on desktop and browser
- `wwwroot` is a build output (gitignored); run `npm run build` before serving the app from the API
- API contract changes need a matching edit in `src/api.ts` until OpenAPI exists
- New UI texts go into all four resx files first
- E2E seeds stock items straight into MongoDB until a stock item endpoint exists
