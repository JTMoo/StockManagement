# ADR-0018: Generic import pipeline (batches, preview → commit, undo)

- Status: Proposed
- Date: 2026-09-26

## Context

- #58 wants legacy data imported through the API and web app for stock items, customers, opening stock, later open invoices, each with upload → preview with a per-row report → commit
- ADR-0009 already ships a stock-item-only path: one call parses, dedupes and imports; no preview, no undo, no other targets
- Money-type ADR (0014) is decided but not migrated yet (`double`/`long` still in place); price/amount fields and opening balances stay out of scope until that lands
- Standing decision: import keeps the first row for a repeated code/key, reports the rest as duplicates
- Per-domain layout (Customers.Core, Sales.Core, ...) only reference their own Contracts + Kernel, never each other; `StockManagement.Import.Core` already follows this by talking to `Kernel.Database.Interfaces` providers directly instead of another domain's Core

## Options

- Commit step: keep ADR-0009's one-shot upload-and-import / add a stored preview step with a separate commit call
- Undo: none / delete the rows a commit created, tracked per batch
- Target dispatch: one service per target (as today, stock items only) / one generic pipeline with a small per-target strategy (`IImportTargetHandler`) so a new target is one class, not a new endpoint set
- Candidate storage between preview and commit: re-upload the file on commit / store each row's parsed candidate (serialized) on the batch so commit needs only the batch id
- Row shape across targets: a target-specific response type per target / one generic row (row number, status, message, a field dictionary) so the pipeline and its web screen don't grow per target

## Decision

- New `ImportBatch`/`ImportBatchRow` entities (`StockManagement.Kernel.Model`, EF-mapped, `ImportBatchRow` owned via `OwnsMany`) hold one row per source row: row number, status (`Ready`/`Duplicate`/`Error`), error message, and the row's parsed candidate as JSON; a batch is `Previewed` → `Committed` → optionally `Undone`
- `IImportTargetHandler` (one per `ImportTarget`, `StockItems` and `Customers` for now) parses, splits duplicates, commits and undoes for its own entity type; `IImportBatchService` is generic over the registered handlers and knows nothing about `StockItem`/`Customer` directly
- `StockItemImportTargetHandler` reuses the existing `IStockItemImportService`/`IStockItemServiceProvider`; `CustomerImportTargetHandler` talks to `ICustomerServiceProvider` directly (two new methods: `GetCustomerByIdAsync`, `AddManyCustomersAsync`) and assigns `Customer.CustomerId` the same way `CustomerService.CreateCustomerAsync` does (moved `FirstCustomerId` onto `Customer` so both places share it) — following the existing Import.Core precedent of not depending on another domain's Core
- Customer de-duplication is exact match on `IdentificationNumber` only; a blank number never matches another (most legacy rows have none), so it always imports as new until #5's fuzzy check lands
- `POST /import/batches` (multipart: `Target` + `File`) previews and stores the batch; `POST /import/batches/{id}/commit` writes its `Ready` rows and records each one's new entity id; `POST /import/batches/{id}/undo` deletes them again; `GET /import/batches/{id}` re-reads a batch. A batch in the wrong state (double commit, undo before commit) is a 409 with a reason, not a 500
- The generic Excel column-matching (header → property by name or localized `Display` name) moved into `ExcelEntityParser<T>`, shared by both handlers; `ExcelStockItemParser` (ADR-0009) now delegates to it instead of duplicating the reflection code
- Web: both stock item and customer import screens use the same preview/commit/undo panel (`StockItemImport`, `CustomerImport`); ADR-0009's one-shot `/stock-items/import` endpoint is removed, along with `IExcelStockItemParser` (superseded by `ExcelEntityParser<T>`, already shared)
- Left for follow-up work (commented on #58 rather than closing it): column-mapping UI and an error-report download

## Consequences

- A commit that races a duplicate insert (something else took the key between preview and commit) fails the whole commit with the provider's existing duplicate-key exception; the batch stays `Previewed` and can be retried after the source is fixed
- Undo removes entities by id; if a committed row was edited afterwards, undo still deletes it as-is (no "was it touched" check)
- Candidates are stored as JSON on `ImportBatchRow`, so a commit never needs the original file again, at the cost of one JSON blob per row in Postgres
- Customer import assigns ids from one batch-wide `SequenceNumber.Next` call instead of one query per row, same trade-off `CustomerService` already makes for a single create
