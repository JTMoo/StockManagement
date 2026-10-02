# ADR-0029: Cursor pagination for list endpoints

- Status: Proposed
- Date: 2026-10-02

## Context

- Issue [#107](https://github.com/JTMoo/Kora/issues/107). Scope: list endpoints in general, not one domain.
- Today, two shapes exist:
  - Offset (`Page`/`PageSize` in, `PagedResult<T>` → `{ Items, TotalCount }` out): `ListInvoicesEndpoint`, `ListOpenInvoicesEndpoint`, `ListOverdueInvoicesEndpoint`.
  - Unpaginated (`EndpointWithoutRequest<IReadOnlyList<T>>`, returns everything): `ListStockItemsEndpoint`, `ListCustomersEndpoint`, `ListUsersEndpoint`, `ListSuppliersEndpoint`, `ListStockItemsBelowMinimumEndpoint`.
- Both break down with the import pipeline ([ADR-0018](0018-generic-import-pipeline.md)) landing thousands of rows: offset pages shift under concurrent writes (`OFFSET n` re-scans and can skip/repeat rows), `TotalCount` is a second query, unpaginated endpoints ship the whole table.
- Owner decision (2026-10-02): cursor pagination, across **all** list endpoints. This ADR is domain-agnostic — it fixes the contract every list endpoint follows, not per-entity sort fields.

## Options

- **Keep offset** (`Page`/`PageSize`). Simple, supports "page 3 of 9", but unstable under writes and `TotalCount` doesn't scale.
- **Cursor, opaque token.** Stable under concurrent writes, no count query. Loses arbitrary page jump and `TotalCount`; backward paging needs client-side cursor history.
- **Keyset with client-visible sort column** (e.g. `?after=2026-09-01`). Same stability as cursor but leaks the sort column and breaks if it's ever changed — rejected.

## Decision

- Every list endpoint exposes a fixed, deterministic order: one or more sort columns + the entity's `Id` as the final tie-break column, same direction as the primary sort column. No client-chosen sort for v1 (per-endpoint order is fixed in code).
- Cursor is opaque to the client: base64 of `{ v: <last row's sort-column value(s)>, id: <last row's Id> }`. Endpoints only emit and accept it — never parse a client-constructed one.
- Request: `cursor` (string, optional — omitted/empty = first page), `pageSize` (optional, default 20, clamp 1–100, same clamp already used for offset). Existing domain filter params (`customerId`, `from`, `to`, …) are unchanged.
- Response envelope, replacing `PagedResult<T>` / `{ Items, TotalCount }`:
  ```
  CursorPage<T> { Items: T[], NextCursor: string | null }
  ```
  `NextCursor == null` → no more rows. No `TotalCount` — not computable cheaply with a cursor; UI drops "page x of y".
- `StockManagement.Kernel.Database.PagedResult<T>` → `CursorPage<T>`; same type for every repository/service that lists, offset or previously-unpaginated.
- Backward paging is client-side: the page keeps a stack of cursors it has seen; "previous" pops the stack and re-requests with the prior cursor. No reverse-cursor support on the server.
- Frontend: `DataTable` ([docs/design/kora/components/DataTable](../design/kora/components/DataTable/README.md)) consumes `{ items, nextCursor }` and drives "load more" / next-page, not a page-number control. Existing page-number UI (`InvoiceList`) moves to this pattern as part of the hookup PR.
- Applies to every endpoint listed in Context, offset and unpaginated alike — one contract, no per-domain exceptions.

## Consequences

- `PagedResult<T>` removed from `StockManagement.Kernel`; `CursorPage<T>` added, used everywhere a list endpoint pages.
- `ListInvoicesEndpoint`, `ListOpenInvoicesEndpoint`, `ListOverdueInvoicesEndpoint`: `Page`/`PageSize` request fields → `cursor`; `TotalCount` dropped from the response and from `InvoiceListResponse`.
- `ListStockItemsEndpoint`, `ListCustomersEndpoint`, `ListUsersEndpoint`, `ListSuppliersEndpoint`, `ListStockItemsBelowMinimumEndpoint`: `EndpointWithoutRequest<IReadOnlyList<T>>` → cursor request/response; each repository query gets an explicit `ORDER BY <sort column(s)>, Id`.
- Any UI showing "page x of y" (`InvoiceList`) loses it; replaced with next/previous via the client-side cursor stack above.
- Client-chosen sort (e.g. a sortable `DataTable` column header) needs a cursor per sort order — explicitly out of scope here; revisit if/when a view needs it.
- Unit tests: cursor encode/decode round-trip, stable order under a page straddling identical sort-column values (tie-break by `Id`), `pageSize` clamp. Integration tests: paging through a seeded table yields every row exactly once, `NextCursor` null on the last page.
