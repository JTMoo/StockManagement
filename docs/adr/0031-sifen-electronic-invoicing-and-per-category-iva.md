# ADR-0031: SIFEN electronic invoicing and per-category IVA

- Status: Proposed
- Date: 2026-10-02

## Context

- Issue [#119](https://github.com/JTMoo/Kora/issues/119). Design only — no implementation in this ADR (owner decision).
- Paraguay mandates SIFEN (DNIT's electronic-invoicing system) by taxpayer rollout group through 2027; pre-printed invoices become invalid once a group's cutover date passes. Every serious PY competitor already supports it — table stakes, not a differentiator (`analysis/paraguay-product-research.md`).
- `CompanySettings.VatRatePercent` (ADR-0015) is one flat rate for the whole catalogue. Real catalogues mix 10%/5%/exempt by category; this is a legal requirement for invoicing, not a nicety.
- `InvoiceCalculator.CalculateTax` (ADR-0014 money rounding) takes one `total` and one `vatRatePercent` — assumes a single rate per invoice.
- `Invoice.Number` (`StockManagement.Kernel/Model/Invoice.cs`) is a single sequential int. SIFEN needs a 44-char CDC (control code), a DNIT-issued timbrado, and establishment/point-of-sale codes as part of each document's identity.
- Out of scope: landed cost / goods-import document chain (#120, ADR-0032).

### Verified (current sources, 2026-10-02)

- Transmission is synchronous for single documents and for batches up to 50; a batch is otherwise async in 3 steps, SIFEN taking 5-10 minutes to process. The signed XML must reach SIFEN within 72 hours of the invoice's declared date. ([Edicom](https://edicomgroup.com/electronic-invoicing/paraguay), [ContaFacilPY](https://contafacilpy.com/guia/sifen/))
- IVA: 10% general; 5% reduced for a basic-basket list (rice, pasta, vegetable oil, yerba mate, milk, eggs, flour, iodized salt), medicines, and some agricultural inputs; some goods exempt. The 5% list draws a "basic form vs. processed form" line (plain milk = 5%, yogurt or cheese = 10%) that isn't a clean fixed table. ([DNIT](https://www.dnit.gov.py/en/web/portal-institucional/w/iva-tasas), [ivacalculator.com](https://ivacalculator.com/paraguay/iva-10-5-tasas/))

### Unverified — flag to owner, do not build against this ADR until confirmed

- Correction/cancellation rule for a DE that SIFEN rejects, or one with a transmission error discovered after the 72h window — DNIT's exact process (re-emit vs. nota de crédito) isn't confirmed from a current resolution.
- Direct SIFEN SOAP integration vs. a third-party PSE (facturador habilitado) that handles signing/XML/transmission for a fee — this is a cost/vendor decision, not resolved here.
- How the signing certificate is acquired and held (own qualified cert from a PSC vs. PSE-hosted signing) — affects whether Kora ever holds the private key.

## Options

- IVA rate storage: category lookup table (hardcode the basic-basket list) / a rate field directly on `StockItem`, set by whoever enters the item
- Invoice numbering: keep one sequential `Number`, add CDC/timbrado alongside it / replace it with the DNIT composite (establishment-PV-number)
- Transmission: synchronous, inside `SaleService.CompleteSaleAsync` (blocks the sale on SIFEN) / asynchronous, decoupled from the sale
- SIFEN client: build the DNIT SOAP/XML/signing stack directly / integrate through a third-party PSE from day one

## Decision

### Per-category IVA

- `StockItem` gets `VatRatePercent` (`decimal`, same type/column shape as `CompanySettings.VatRatePercent`), not an enum — the legal category (basic-form vs. processed) isn't a fixed table Kora can hardcode and keep correct; whoever enters the item sets the rate, same trust model as `Price` today
- `CompanySettings.VatRatePercent` becomes the default copied onto a new `StockItem` (keeps today's single-rate behavior as the out-of-the-box default); it stops being used for tax calculation once lines carry their own rate
- `InvoiceCalculator.CalculateTax` changes from `(total, vatRatePercent, digits)` to taking the line list: group by each line's `VatRatePercent`, compute the VAT share per group (`groupTotal * rate / (100 + rate)`, same inclusive-price convention as today), sum, round once to `currencyDecimalDigits`
- `SaleLine`/`ShoppingCartItem` snapshot the stock item's `VatRatePercent` at sale time, same reasoning as any other invoice line value not currently snapshotted (not fixed here, not a new gap)

### SIFEN document identity

- `CompanySettings` gains `Ruc`, `Timbrado` (number + validity dates), `EstablishmentCode`, `PointOfSaleCode` — single-row extension, same pattern as ADR-0015
- `Invoice.Number` is replaced by the DNIT composite (`EstablishmentCode`-`PointOfSaleCode`-sequence, e.g. `001-001-0000001`); `SequenceNumber.Next` scopes per establishment+PV instead of globally
- `Invoice` gains `Cdc` (44-char control code, generated per DNIT's algorithm from RUC/establishment/PV/doc type/number/security code/check digit) and `TransmissionStatus` (`Pending`/`Sent`/`Accepted`/`Rejected`/`Error`)
- KuDE (the printable/QR representation) is generated on demand from the signed XML + CDC, never persisted as a stored file

### New bounded context

- `StockManagement.Sifen.Core` (+ `.Contracts`): CDC generation, XML document builder, XAdES signing, DNIT XSD validation — same layering as every other domain (ADR-0002)
- `ISifenGateway` is the only transmission abstraction application code depends on; which implementation backs it (direct DNIT client vs. a PSE adapter) is the unverified vendor/cost decision above and is picked in a follow-up issue, not here

### Transmission architecture

- Asynchronous, decoupled from the sale: `CompleteSaleAsync`'s existing atomic stock+invoice write (ADR-0012/ADR-0022) is unaffected and does not depend on SIFEN's availability — DNIT's own 72h window makes synchronous-on-sale unnecessary, and a government outage must not block selling
- Outbox pattern: a `PendingTransmission` row is written in the same transaction as the invoice; a background worker polls it, calls `ISifenGateway`, and updates `TransmissionStatus` — no message broker, consistent with the single-process API and the "no fire-and-forget" rule
- Retry: exponential backoff, capped, bounded by the 72h transmission deadline; `Rejected` (a SIFEN validation error) is not auto-retried — it needs a corrected re-send, per the unverified correction rule above; `Error` (network/timeout) retries automatically
- A list of `Rejected`/stuck-`Error` invoices is a required UI surface (not optional) — these are legally unresolved until cleared

## Tests

- Unit: per-line VAT grouping in `InvoiceCalculator` (extends existing `InvoiceCalculatorTests`); CDC construction and its check-digit algorithm as a pure function; XML output validated against DNIT's published XSD — all without network
- Integration (`StockManagement.Api.Tests`, `WebApplicationFactory` + Testcontainers): `ISifenGateway` is faked, never calls real DNIT (sandbox included) from CI — same reasoning as not hitting live third-party services in automated tests
- A local-only manual harness against DNIT's test environment, documented in README, is out of `dotnet test`/CI

## Consequences

- Existing sequential `Invoice.Number` and `SequenceNumber.Next` need rework for the composite format — a follow-up issue, not implemented here
- `CompanySettings` keeps growing as a single row (RUC, timbrado, cert reference); still no new entity, per ADR-0015's pattern
- Until a gateway implementation is chosen, `ISifenGateway` has only a no-op stub — invoicing keeps working exactly as today (non-electronic) and nothing else is blocked by this ADR
- 5%/exempt correctness now depends on correct per-item data entry, not a maintained category table — same risk profile as manual pricing errors today
- No retroactive SIFEN submission for invoices issued before this ships

## Follow-up issues (implementation, after this ADR is accepted)

1. Per-item `VatRatePercent` + per-line `InvoiceCalculator.CalculateTax` rework (ships independently — fixes the compliance gap even before SIFEN transmission exists)
2. `CompanySettings` SIFEN fields (RUC/timbrado/establishment/PV) + composite invoice numbering
3. `StockManagement.Sifen.Core`: CDC generation, XML builder, XAdES signer, XSD validation tests — no live transmission
4. `ISifenGateway` + outbox (`PendingTransmission`) + background retry worker + rejected/stuck-error UI list
5. Blocked on owner: pick direct-DNIT vs. PSE vendor, and how the signing certificate is acquired/held — needed before issue 4's gateway implementation can start
