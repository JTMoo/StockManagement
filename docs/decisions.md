# Decisions knowledge base

**Consult this page before making an important decision** (see *Before making an important decision* in
[../CLAUDE.md](../CLAUDE.md)). It lists what has been decided, the smaller standing rules, and what is still open.
Keep it in sync in the same PR that adds or changes a decision.

## Architecture Decision Records

| ADR | Decision | Status |
|---|---|---|
| [0001](adr/0001-record-architecture-decisions.md) | Record important decisions as ADRs in a repo knowledge base | Accepted |
| [0002](adr/0002-business-logic-in-domain-and-application.md) | Business logic moves out of view models into Domain and Application | Accepted |
| [0003](adr/0003-fastendpoints-api-and-react-frontend.md) | FastEndpoints REST API with a React frontend, desktop-first | Accepted (desktop shell open) |

New ADRs: copy [adr/template.md](adr/template.md), next free number, add a row here.

## Standing decisions

Smaller rules that do not need a full ADR. Each has a date and a source.

- **Update entities by `Id`, never by business key.** Invoice number, stock item code and customer id are unique
  indexes, not update filters. (2026-09-24, PR [#32](https://github.com/JTMoo/StockManagement/pull/32): invoice
  updates matched `Id` against `Number` and would have inserted duplicates.)
- **Business rules get unit tests when they move** out of the UI. (2026-09-24, [ADR-0002](adr/0002-business-logic-in-domain-and-application.md))
- **All user-facing text is a resource key**; supported cultures stay de-DE, en-US, es-PY. (2026-09-24, existing code)
- **The owner's review comments become conventions** in the *Conventions learned from review comments* section of
  [../CLAUDE.md](../CLAUDE.md). (2026-09-24, [ADR-0001](adr/0001-record-architecture-decisions.md))

## Open questions

Decisions still to make. When one is decided, write an ADR (or a standing decision) and remove it here.

| Question | Options | Notes |
|---|---|---|
| Desktop shell for the React app | Tauri, Electron, WebView2 | Affects installer, packaging, how the local API starts. [ADR-0003](adr/0003-fastendpoints-api-and-react-frontend.md) |
| Money representation | `decimal` with currency, integer minor units | Today `double` on stock items, `long` on invoices. |
| Sequential numbers (invoice, customer) | Counter document with `$inc`, keep `Max + 1` | `Max + 1` is race-prone and loads the whole collection. |
| Duplicate codes inside one import file | Drop all copies (current), keep the first | Raised in PR [#34](https://github.com/JTMoo/StockManagement/pull/34). |
| Import pipeline shape | read, map, clean, validate, preview, commit | Draft in the [codebase analysis](knowledgebase/codebase-analysis-2026-09.md), section 4.6. |
| `#region` and banner comments in new code | keep, drop | Owner habit today; draft guideline drops them. |
| Nullable warnings as errors | yes, no | Would remove the CS8618 suppression in `.editorconfig`. |

## Other knowledge

- [knowledgebase/codebase-analysis-2026-09.md](knowledgebase/codebase-analysis-2026-09.md): analysis of the WPF app
  as of September 2026, the owner's observed style, known issues and the draft guidelines behind `CLAUDE.md`.
