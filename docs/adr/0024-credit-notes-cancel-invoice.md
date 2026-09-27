# ADR-0024: Cancel an invoice with a credit note

- Status: Proposed
- Date: 2026-09-27

## Context

- #56: invoices are never deleted; cancelling one returns the stock (#29) and leaves a record
- `IInvoiceServiceProvider.DeleteInvoiceAsync` exists but has no endpoint or caller today — deleting an invoice would silently lose the sale and leave no trace
- #55 (payments/open balance, PR #95) adds `Invoice.Payments` and a computed status; this ADR does not touch it — a cancelled invoice's balance/status is a follow-up once #95 merges
- "Customers with invoices: deactivate, not delete" (#56) has no code to change yet: there is no customer delete endpoint

## Options

- Call `DeleteInvoiceAsync`: loses the invoice and its stock movement history
- Flip a status field on the invoice directly, no separate record: no fixed total/tax to reconcile against later, no traceable reason
- New `CreditNote` referencing the invoice (full amount, one per invoice), invoice gets `IsCancelled`; restocking reuses the check-in `Transaction` pattern (ADR-0012)

## Decision

- `CreditNote`: `Number` (own sequence, starts at 1), `Date`, `Reason` (required), `Total`/`Tax` copied from the invoice, `Invoice` (required, unique — one credit note per invoice)
- `Invoice.IsCancelled` (bool, default false); the invoice itself is never deleted or otherwise changed
- `ICreditNoteServiceProvider.TryAddCreditNoteAsync`: claims the invoice with a conditional `UPDATE ... WHERE NOT "IsCancelled"` (same guard shape as the sale/check-out oversell guards), then returns every line's stock with a `Transaction.Amount` row carrying the credit note's reason, then stores the credit note — all in one transaction; `false` when the invoice was already cancelled
- `POST /invoices/{Number}/cancel` (`Sales.Write`): 404 unknown invoice, 409 when already cancelled, 200 with the stored credit note; `GET /credit-notes/{Number}` (`Sales.Read`)
- Full cancellation only, no partial credit notes: matches the issue ("cancel invoice"), not a resupply flow
- React: a "Cancel invoice" action with a required reason on `InvoiceView`, replaced by a "Cancelled" label once done

## Consequences

- A cancelled invoice's `AmountDue`/status (once #95 lands) needs a follow-up: crediting it out but not touching `Payments` mixes an unpaid invoice's balance with the invoice being void
- `DeleteInvoiceAsync` stays unused by the API; removing it is a separate cleanup, not part of this change
- Customer deactivation from #56 stays undone until a customer delete endpoint exists
