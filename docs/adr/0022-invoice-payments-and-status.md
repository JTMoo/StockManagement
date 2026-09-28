# ADR-0022: Invoice payments and status

- Status: Proposed
- Date: 2026-09-27

## Context

- #55: `SaleCondition.Credit` + `Invoice.ExpirationDate` exist, but nothing records a payment against a credit invoice
- Needed for ERP basics: invoice status (open/partly paid/paid/overdue), payment (date, amount, method), open items per customer, an overdue list
- Blocked on the money type decision (ADR-0014); that landed as `decimal` + `CompanySettings.CurrencyDecimalDigits` (PR #90)

## Options

- Where a payment lives: its own top-level entity with an `InvoiceId` foreign key / owned collection on `Invoice` (like `Invoice.Items`)
- Invoice status: a stored column updated on every payment / computed on read from `Total` and `Payments`
- Open items / overdue list: new endpoints / a `Status` filter on the existing `GET /invoices`

## Decision

- `Invoice.Payments` is an EF owned collection (`Payment`: `Date`, `Amount`, `Method`), same shape as `Invoice.Items`; own `Payments` table, cascade-deleted with the invoice
- `InvoiceStatus` (Open/PartiallyPaid/Paid/Overdue) is never stored; `InvoiceStatusCalculator` (Domain, `StockManagement.Sales.Core`) derives it from `Total`, `Payments` and `ExpirationDate` vs. "now" on every read - avoids a second source of truth going stale, and re-evaluates `Overdue` without a background job
- `IPaymentService` (Application) is the only writer: `RecordPaymentAsync` rounds the amount to `CompanySettings.CurrencyDecimalDigits` (same `AwayFromZero` rule as invoice totals) and rejects a non-positive amount or one exceeding the amount due - `RecordPaymentResult`, not an exception, per the repo's `Try*` convention for expected failures
- Two new endpoints, `GET /invoices/open` (optionally filtered by `CustomerId`) and `GET /invoices/overdue`, both ordered soonest-due-first; `POST /invoices/{Number}/payments` and `GET /invoices/{Number}/payments` for recording/listing. All under `Permission.Sales*` - payments are part of the sales/invoice lifecycle, not a separate module
- `InvoiceResponse` gains `AmountPaid`, `AmountDue`, `Status` so every existing invoice read (list, get, the response of creating a sale) carries them, not only the new endpoints

## Consequences

- Filtering "open"/"overdue" invoices happens in the application layer, not the database, because the status depends on a computed sum plus "now": `GetOpenInvoicesAsync`/`GetOverdueInvoicesAsync` load every invoice via `IInvoiceServiceProvider.GetInvoicesAsync()` and paginate in memory, same tradeoff `SaleService.GetNextInvoiceNumberAsync` already accepts. Revisit if the invoice table grows large enough for this to matter
- No partial refund / payment reversal endpoint; deleting a wrongly-recorded payment isn't covered here
- No credit note / write-off handling (#56, separate)
