# CLAUDE.md

## Rules

1. **KIS.** Keep it simple: code, docs, comments.
2. **No prose. Never.** Bullets, tables, code.
3. **Cut whenever possible.** Dead code, outdated comments, decisions that need revisiting.

## Before an important decision

Important = hard to undo or spans features (layers, frameworks, persistence, API contracts, auth, money, import, tests, packaging).

1. Read [docs/decisions.md](docs/decisions.md). No silent contradiction of an accepted ADR.
2. Grill it: `/grill-with-docs` (`~/.claude/skills/grill-with-docs`, owner's machine). Not available: ask the owner directly.
3. Record it: ADR in [docs/adr/](docs/adr/README.md), next free number (check the index and open PRs). Then run `./scripts/generate-adr-index.sh` and commit the result — CI fails if `docs/decisions.md`'s table is stale. Never hand-edit the table. Small rules: *Standing decisions*.

## Merging (ADR-0027)

- No owner review required. A PR merges itself once every required check is green: CI + `claude-review` (blocking findings only)
- After pushing, enable auto-merge on your own PR
- `main` gets merged into every open PR branch after each push to `main`; a real conflict gets a PR comment, not a silent merge
- Owner is pulled in for architecture/scope calls or when the next step is unclear — not for a green PR

## Commands

- Build: `dotnet build StockManagement.sln` (Gui needs Windows)
- Test: `dotnet test StockManagement.Tests`
- API tests (Docker): `dotnet test StockManagement.Api.Tests`
- Run API: `dotnet run --project StockManagement.Api` (PostgreSQL on `127.0.0.1:5432`, see README)
- Web (`StockManagement.Web`): `npm ci`, `npm run dev` (API running), `npm test`, `npm run e2e` (PostgreSQL), `npm run build` (→ API `wwwroot`)
- CI: `Integration.yml` (Windows tests + Linux API and web tests on PR), `Delivery.yml` (MSI on tag), `claude-review.yml` (blocking review), `sync-main-into-prs.yml`

## Style (existing)

- Tabs, Allman, file-scoped namespaces = folder path
- `[]`, target-typed `new()`, primary constructors
- One-line guard clauses first: `if (x is not Foo foo) return false;`
- `this.` for members, `_camelCase` fields, `*Async` suffix
- Role suffixes: `*Service` (logic), `*Repository` (data), `*Helper`, `*Extensions`, `*ViewModel`, `*Type` (enums)
- XML docs: public contract members; `<summary>` one line, details in `<remarks>`
- User-facing text only via `StockManagement.Language`
- Expected failures: `Try*` + `out` or result, not exceptions
- Commands: `XxxCommand` → private `OnXxxCommand`; handlers `OnXxx`

## Target (new code)

- Logic in Domain/Application only ([ADR-0002](docs/adr/0002-business-logic-in-domain-and-application.md))
- FastEndpoints, one endpoint per file, feature folders; React + TS ([ADR-0003](docs/adr/0003-fastendpoints-api-and-react-frontend.md))
- `StockManagement.Web` UI: Kora design system ([ADR-0025](docs/adr/0025-kora-design-system.md); search backend [ADR-0026](docs/adr/0026-cross-domain-search.md)) — tokens, brand book, component guidelines and previews in [docs/design/kora/](docs/design/kora/README.md), the source of truth (open a `components/<Name>/preview.html` directly, no build step)
- `Request`/`Response` records, not persistence models
- Built-in DI; one `IMongoClient`; config for connection/DB name
- Update by `Id`; business keys = unique indexes
- Atomic multi-step writes; counters via `$inc`
- `CancellationToken` on async; no `async void`, no fire-and-forget, no empty `catch`
- `ILogger<T>`, log the exception
- Import errors: per-row report; keep source (file, sheet, row)
- Mongo duplicate key → domain error in Infrastructure
- Money: never `double`
- No binaries (`.msi`) in git; no customer names or connection strings in code

## Tests

- MSTest + Moq, `Method_Scenario_ExpectedResult`, `// Arrange` `// Act` `// Assert`
- Unit test every rule moved out of the UI
- API integration: `WebApplicationFactory` + Testcontainers PostgreSQL
- Public behavior only, no reflection

## Conventions learned from review comments

Owner comments on code → add one line. Newest last. Conflicts with a rule above → change the rule.

`- YYYY-MM-DD: <rule> (<PR/thread link>)`

- 2026-09-24: XML `<summary>` one line, rest in `<remarks>` ([#34](https://github.com/JTMoo/StockManagement/pull/34))
- 2026-09-24: Public contract members need XML docs ([#34](https://github.com/JTMoo/StockManagement/pull/34))
- 2026-09-24: Rules 1-3 above ([#35](https://github.com/JTMoo/StockManagement/pull/35#issuecomment-5813587865))
- 2026-09-24: Bugs → GitHub issues (match existing first), not docs ([#40](https://github.com/JTMoo/StockManagement/pull/40#discussion_r4093518187))
- 2026-09-24: Addressed review comment → reply on the PR saying if/why ([#72](https://github.com/JTMoo/StockManagement/pull/72#issuecomment-5819888414))
- 2026-09-25: UI change → PR body has at least one screenshot embedded (not just the CI artifact) and the test steps taken, one sentence each ([#80](https://github.com/JTMoo/StockManagement/pull/80))
