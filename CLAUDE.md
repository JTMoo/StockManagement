# CLAUDE.md

## Rules

1. **KIS.** Keep it simple: code, docs, comments.
2. **No prose. Never.** Bullets, tables, code.
3. **Cut whenever possible.** Dead code, outdated comments, decisions that need revisiting.

## Before an important decision

Important = hard to undo or spans features (layers, frameworks, persistence, API contracts, auth, money, import, tests, packaging).

1. Read [docs/decisions.md](docs/decisions.md). No silent contradiction of an accepted ADR.
2. Grill it: `/grill-with-docs` (`~/.claude/skills/grill-with-docs`, owner's machine). Not available: ask the owner directly.
3. Record it: ADR in [docs/adr/](docs/adr/README.md) + row in `docs/decisions.md`. Small rules: *Standing decisions*.

## Commands

- Build: `dotnet build StockManagement.sln` (Gui needs Windows)
- Test: `dotnet test StockManagement.Tests`
- CI: `Integration.yml` (test on PR), `Delivery.yml` (MSI on tag)

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

## Known bugs (fix when touched)

| Where | Bug |
|---|---|
| `ConversionHelper.Ones[9]` | `ten` instead of `nine` |
| `SaleService.CompleteSaleAsync` | Stock updated before invoice insert, not atomic |
| `StockItemServiceProvider` delete/amount change | Transaction insert not awaited |
| `DatabaseManager` | `new MongoClient` per call; hard-coded connection + `LaCosecha_` DB name |
| `MainManager.CreateCollectionIndeces` | `*IndexCreated` flags never saved |
| `CustomerViewModel`, `InvoiceViewModel` search | Filters the filtered list; cleared search shows nothing new |
| Search filters | Raw user text into `Regex.IsMatch`; invalid pattern throws |
| Identifiers | `Collabsed`, `Indeces`, `Nineth`, `Twelveth`, `Eight` |

## Tests

- MSTest + Moq, `Method_Scenario_ExpectedResult`, `// Arrange` `// Act` `// Assert`
- Unit test every rule moved out of the UI
- API integration: `WebApplicationFactory` + Testcontainers MongoDB
- Public behavior only, no reflection

## Conventions learned from review comments

Owner comments on code → add one line. Newest last. Conflicts with a rule above → change the rule.

`- YYYY-MM-DD: <rule> (<PR/thread link>)`

- 2026-09-24: XML `<summary>` one line, rest in `<remarks>` ([#34](https://github.com/JTMoo/StockManagement/pull/34))
- 2026-09-24: Public contract members need XML docs ([#34](https://github.com/JTMoo/StockManagement/pull/34))
- 2026-09-24: Rules 1-3 above ([#35](https://github.com/JTMoo/StockManagement/pull/35#issuecomment-5813587865))
