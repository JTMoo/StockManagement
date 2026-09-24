# CLAUDE.md

Guidance for Claude (and humans) working in this repository. This file is meant to **grow**: whenever the owner
comments on code, the lesson goes into [Conventions learned from review comments](#conventions-learned-from-review-comments).

## Project in one paragraph

StockManagement is a stock, customer and invoicing tool (.NET 8, WPF, MongoDB, ClosedXML, resx i18n for de-DE,
en-US, es-PY). It is being redesigned into a FastEndpoints REST API plus a React web frontend that runs mainly as a
desktop app but can be opened in a browser, with unit and integration tests. After that come ERP basics, focused on
importing messy legacy customer data. See [docs/decisions.md](docs/decisions.md) for where things stand.

## Before making an important decision

1. **Consult the knowledge base first:** read [docs/decisions.md](docs/decisions.md) and the ADRs it links. Do not
   contradict an accepted ADR silently; if it no longer fits, propose a new ADR that supersedes it.
2. **Grill the decision:** run the owner's *grill me* skill (the "grill me with docs" skill on the owner's computer)
   against the knowledge base before committing to the decision. In a session where that skill is not available,
   ask the owner the hard questions directly instead: alternatives, what it costs to undo, what it means for tests,
   data migration and the desktop/browser split.
3. **Record it:** write an ADR in [docs/adr/](docs/adr/README.md) and add a row to the index in
   [docs/decisions.md](docs/decisions.md). Small standing rules go in the *Standing decisions* list there instead.

A decision is *important* when it is hard to undo or shapes more than one feature: project/layer structure,
frameworks and libraries, persistence and data model, API contracts, authentication, money/number handling,
import pipeline design, test strategy, packaging and deployment.

## Commands

- Build: `dotnet build StockManagement.sln` (the WPF `StockManagement.Gui` needs Windows, or
  `-p:EnableWindowsTargeting=true` plus the WindowsDesktop SDK on Linux).
- Test: `dotnet test StockManagement.Tests`
- CI: `.github/workflows/Integration.yml` runs `dotnet test` on Windows for pushes and PRs to `main`;
  `Delivery.yml` builds the MSI on tags.

## Coding guidelines

Derived from the owner's existing code (see [docs/knowledgebase/codebase-analysis-2026-09.md](docs/knowledgebase/codebase-analysis-2026-09.md)).
**Keep** rules describe how the owner already writes code. **Target** rules are the draft direction for the new API +
React solution; they hold until the owner changes them (record changes as review conventions or ADRs).

### C# style (keep)

- Tabs for indentation, Allman braces, file-scoped namespaces that mirror the folder path.
- Two blank lines after the `using` block/namespace and between the field block and the constructor.
- Collection expressions `[]`, target-typed `new()`, primary constructors for services, simple types and exceptions.
- Guard clauses first, one line each: `if (x == null) return;`, `if (x is not Foo foo) return false;`.
- `this.` for instance members; private fields are `_camelCase`.
- PascalCase for types and members, `I*` for interfaces, every async method ends in `Async`.
- Role suffixes carry meaning: `*Manager`, `*Service`, `*Repository`, `*Helper`, `*Extensions`, `*ViewModel`,
  `*Converter`, `*Command`, `*Exception`, `*Type` for enums. In new code use `*Repository` for data access and
  `*Service` for business logic instead of `*ServiceProvider`.
- View model commands: `XxxCommand` property bound to a private `OnXxxCommand` handler; event handlers are `OnXxx`.
- XML doc comments on public API and non-obvious methods; simple members stay undocumented.
- Enums: `[Description]` for a technical value, `[Display]` for a localized label.
- All user-facing text goes through `StockManagement.Language` resources, never hard-coded.

### C# style (target, new code)

- Async methods take a `CancellationToken`.
- No `async void` outside real event handlers; no fire-and-forget writes; no empty `catch`.
- `required` members instead of relying on the CS8618 suppression; nullable warnings are meant to become errors.
- `#region` blocks and `****` banner comments are not needed in new, small files (owner's call, see open questions).
- Money is `decimal` (or integer minor units), with one rounding rule in one place.

### Architecture (target)

- Business logic lives in Domain/Application, never in endpoints, view models or React components
  ([ADR-0002](docs/adr/0002-business-logic-in-domain-and-application.md)).
- API is FastEndpoints; UI is React + TypeScript
  ([ADR-0003](docs/adr/0003-fastendpoints-api-and-react-frontend.md)). One endpoint per file, grouped by feature
  folder with its request, response and validator.
- Persistence models are not API contracts: endpoints use `Request`/`Response` records.
- Built-in DI container, no new singletons or static facades; one shared `IMongoClient`; connection string and
  database name come from configuration.
- Entities are updated by their `Id`; business keys (invoice number, stock item code, customer id) are unique
  indexes, not lookup keys for updates.
- Multi-step writes (a sale) are atomic; sequential numbers come from a counter document with `$inc`.

### Errors and logging

- Expected failures: `Try*` + `out` or result types (keep). Validation through FastEndpoints validators; domain errors
  map to problem details with a localized message key.
- Translate Mongo duplicate-key errors into domain errors in the infrastructure layer.
- Log through `ILogger<T>` and log the exception, not only `ex.Message`.
- Import errors are collected per row and returned as a report, never swallowed.

### Tests

- MSTest + Moq; names `Method_Scenario_ExpectedResult`; `// Arrange`, `// Act`, `// Assert` comments.
- Every business rule moved out of the view models gets unit tests with the move.
- API integration tests with `WebApplicationFactory` against a real MongoDB (Testcontainers), one class per feature.
- Test public behavior; no reflection into private fields.

### Repository hygiene

- No built binaries (`.msi`) in git; no customer-specific names or connection strings in code.
- Descriptive commit messages; one feature or fix per PR.

## Conventions learned from review comments

Append one entry whenever the owner comments on code (in a PR review or in chat). Newest at the bottom. If an entry
contradicts a guideline above, update the guideline too and link the entry. If it is an important decision, write an
ADR instead and link it here.

Format:

```
### YYYY-MM-DD: <short rule>
- Rule: <what to do / not do, one or two sentences>
- Why: <the owner's reason>
- Source: <PR link or thread, file:line if useful>
```

_No entries yet._
