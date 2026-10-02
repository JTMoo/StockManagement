# ADR-0034: Open invoices as an import target

- Status: Proposed
- Date: 2026-10-02

## Context

- #58's last remaining target: legacy receivables (open invoices) a customer still owes, so a migrating business doesn't start with an empty invoice list
- Builds on #55 (ADR-0022): `Invoice.Payments` (owned collection), `InvoiceStatus`/`AmountDue` always computed on read, never stored
- Unlike `StockItems`/`Customers` (ADR-0018, create) or `OpeningStock` (ADR-0019, mutate an existing entity's amount), a legacy invoice row has no stock-item lines to re-derive from a flat Excel export — only a customer, a number, dates, a total, and how much is already paid
- `IInvoiceServiceProvider.TryAddSaleAsync` decrements stock per line and is for real-time sales; a legacy invoice must not re-touch stock that's already accounted for (either already sold historically, or set directly via `OpeningStock`)

## Options

- Write path: `TryAddSaleAsync` (stock-deducting, needs `Items`) / `AddInvoiceAsync` (plain insert, no stock movement) — only the latter fits a line-less legacy row
- Customer match: `IdentificationNumber` (what a legacy export carries, same key #87/#64 already use) / `CustomerId` (not known to the source system)
- Unmatched customer: its own preview status / reuse the `Duplicates` bucket (ADR-0019's accepted "not ready, no distinct reason" trade-off)
- Partial payment already made on the legacy invoice: seed one `Payment` row at commit so `AmountDue`/`InvoiceStatus` are right immediately / import everything as fully unpaid and let the owner record it by hand

## Decision

- `OpenInvoiceRow(CustomerIdentificationNumber, Number, Date, ExpirationDate, Total, Tax, AmountPaid)`, internal to `Import.Core`, parsed with the existing `ExcelEntityParser<T>`
- `OpenInvoiceImportTargetHandler(ICustomerServiceProvider, IInvoiceServiceProvider)`:
  - `SplitDuplicatesAsync`: a `Number` already stored or repeated within the file, or a `CustomerIdentificationNumber` matching no existing customer, all land in `Duplicates` (same bucket ADR-0019 already overloads for "not ready")
  - `CommitAsync`: looks the customer up by `IdentificationNumber`, builds an `Invoice` with `SaleCondition = Credit`, `Items = []`, and — when `AmountPaid > 0` — one seeded `Payment` dated `Date`, `Method = Other`; stored with `AddInvoiceAsync` (no stock movement)
  - `UndoAsync`: `DeleteInvoiceAsync` by the id `CommitAsync` returned — a hard delete, same "delete what this import created" model as `StockItems`/`Customers`, not #56's credit-note cancel flow (that's for invoices customers transact against later)
- `ImportTarget.OpenInvoices` added; import endpoints gain `Permission.SalesWrite`/`SalesRead` alongside their existing `StockItemsWrite`/`CustomersWrite` checks
- No `SaleCondition` or line-item columns exposed — a flat legacy export has neither, and `InvoiceStatus`/`AmountDue` already compute off `Total`/`Payments`, never `Items`

## Consequences

- `Duplicates` now means three different "not ready" reasons (real duplicate, ADR-0019's unmatched code, this ADR's unmatched customer) with no way to tell them apart from the batch response; still the cheapest option for a third target, flagged again for whenever a distinct status is finally worth adding
- Undo hard-deletes an imported invoice instead of routing through #56's credit-note cancel; acceptable since it only ever reverses what the same import batch just created
- Seeded `Payment.Method = Other` has no more specific legacy-import method; the enum already covers this case
