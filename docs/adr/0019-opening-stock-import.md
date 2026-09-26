# ADR-0019: Opening stock as an import target

- Status: Proposed
- Date: 2026-09-26

## Context

- #58 / erp-gaps #6-#7: customers starting mid-year need an initial stock count set without replaying history, as a movement with an audit trail (ADR-0012's `Transaction.Reason`), not a silent field overwrite
- Unlike the two existing targets (`StockItems`, `Customers`, ADR-0018), opening stock rows don't create new entities: each row (`Code`, `Amount`) must match an *existing* stock item and adds to its `Amount` via `CheckInStockItemAsync`
- `IImportTargetHandler.UndoAsync(IReadOnlyList<string> entityIds)` only gets entity ids, matching the "delete what commit created" model — not enough to reverse a quantity change, which needs the amount back

## Options

- Undo: extend `IImportTargetHandler.UndoAsync` to receive each row's candidate alongside its entity id / leave the interface as is and skip undo for this target
- Row not matching an existing `Code`: report as `Duplicate` (reuses the existing "not ready, no distinct reason shown" bucket) / add a third preview outcome just for this case
- Commit: reuse `IStockItemServiceProvider.CheckInStockItemAsync` directly / duplicate its logic inside the handler

## Decision

- `UndoAsync(IReadOnlyList<(string EntityId, object Candidate)> entities, ...)`: widened to carry the candidate back, not just the id. `StockItemImportTargetHandler`/`CustomerImportTargetHandler` ignore the extra field (delete-by-id unchanged); `OpeningStockImportTargetHandler` reads the `Amount` off it and reverses with `TryCheckOutStockItemAsync`, same no-touched-check trade-off ADR-0018 already accepts for delete-based undo (a row edited after commit reverses "as-is")
- New `OpeningStockRow(string Code, int Amount)` (internal, `Import.Core`, not a persisted entity), parsed with the same `ExcelEntityParser<T>`
- `OpeningStockImportTargetHandler.SplitDuplicatesAsync`: candidates whose `Code` repeats within the file keep the first (standing decision); candidates whose `Code` matches no *existing* stock item also land in `Duplicates` — reuses the pipeline's one "not ready" bucket rather than adding a third preview status, at the cost of an imprecise label (a genuinely unmatched code isn't a duplicate)
- `CommitAsync`: looks up each candidate's stock item by `Code`, calls `CheckInStockItemAsync(item, candidate.Amount, "Opening stock import")`, returns the stock item's id (not a new id — several rows could target the same item across two batches, each undoable independently since amounts are per-batch)
- `ImportTarget.OpeningStock` added; `PreviewImportEndpoint` gains it to `Permissions(Permission.StockItemsWrite, ...)` (already covers stock item mutation)

## Consequences

- The `Duplicates` bucket now means two different things (real duplicate vs. unknown code) with no way to tell them apart from the batch response alone; acceptable for a first cut, worth a distinct status if this comes up again for a future target
- `UndoAsync`'s new tuple parameter is a breaking signature change for `IImportTargetHandler`, but a no-op for the two existing handlers
- Reversing a check-in with `TryCheckOutStockItemAsync` can fail silently (insufficient stock, e.g. stock was already sold) — undo then leaves that one item un-reversed while the rest of the batch still flips to `Undone`; no per-item undo failure is surfaced, matching the "no touched-check" trade-off already accepted for the other two targets
