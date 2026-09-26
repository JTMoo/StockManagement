# ADR-0015: Company settings and configurable VAT rate

- Status: Proposed
- Date: 2026-09-26

## Context

- Every onboarding-relevant value is a hardcoded constant: `InvoiceCalculator.TaxDivisor = 11` (flat 10 % VAT), `InvoiceCalculator.PaymentTermInDays = 30`, `InvoiceCalculator.FirstInvoiceNumber = 1`, `CustomerService.FirstCustomerId = 1001` ([erp-gaps.md](../../analysis/erp-gaps.md) #3, #12; issue [#31](https://github.com/JTMoo/StockManagement/issues/31))
- A `StockManagement.Settings.Core` / `.Core.Contracts` slice and a single-row `AppSettings` entity already exist for the UI language ([PR #74](https://github.com/JTMoo/StockManagement/pull/74))
- Money type is a separate decision (ADR-0014, decided on another branch as `decimal`/`numeric(18,2)`, not yet merged): amounts here stay `decimal`/`long` as already used; this ADR does not revisit that
- No currency field exists anywhere; the money-type ADR expects company settings to hold it

## Options

- New `StockManagement.CompanySettings.*` bounded context / extend the existing `AppSettings` row and `ISettingsService`
- VAT: one rate for the whole catalogue / a rate per stock item (gap #12's "10 %/5 %" case)

## Decision

- Extend the existing single-row `AppSettings` (Kernel) and `ISettingsService` (Settings.Core) rather than adding a new project pair; add `CompanySettings` (Settings.Core.Contracts) as the read/write shape for `CompanyName`, `TaxId`, `Currency` (ISO 4217 code, display only), `VatRatePercent`, `PaymentTermInDays`, `FirstInvoiceNumber`, `FirstCustomerId`
- One VAT rate for now, not per item; defaults (`VatRatePercent = 10`, `PaymentTermInDays = 30`, `FirstInvoiceNumber = 1`, `FirstCustomerId = 1001`) reproduce today's hardcoded behavior exactly
- `InvoiceCalculator.CalculateTax`/`CalculateExpirationDate` take the rate/term as parameters instead of constants; `SaleService`/`CustomerService` read them from `ISettingsService` per call
- `GET`/`PUT /company-settings` (FastEndpoints), behind auth like other endpoints; React "Company settings" screen next to the existing language settings screen

## Consequences

- VAT is still one rate for the whole catalogue; gap #12's per-item rates need a follow-up (a rate on `StockItem`, plus `InvoiceCalculator` summing per line)
- `SaleService`/`CustomerService` now depend on `ISettingsService` and fetch settings on every call; acceptable at this scale, revisit if it shows up as a hot path
- `Currency` is a plain string, no formatting/rounding behavior tied to it yet
