# ADR-0014: `decimal` for money, no separate Money type

- Status: Proposed
- Date: 2026-09-26

## Context

- `StockItem.Price`/`Factor`, `SaleLine.UnitPrice`: `double` — binary float, rounding errors on prices
- `Invoice.Total`/`Tax`: `long`, whole units only, rounded with `Math.Round` (ToEven) in `InvoiceCalculator`
- No currency field anywhere; VAT flat 10% hardcoded (`TaxDivisor = 11`)
- #31 (open): company settings will hold currency + VAT rate, single currency per company
- Price import from Excel not implemented; blocked on this decision (erp-gaps.md #4)
- No multi-currency requirement or code exists today

## Options

- Keep `double`: wrong tool for money, silent precision loss, blocks price import from decimal-priced legacy data
- Integer minor units (`long` cents): current `Total`/`Tax` pattern already does this at 0 decimals; needs a per-currency decimal-digit count to scale correctly once prices have cents — same information a currency type would carry, without the type safety
- `decimal` scalar: fixes precision, maps to Postgres `numeric` directly, matches single-currency scope
- `Money` value object (`decimal` + `Currency`): type-safe against cross-currency arithmetic bugs; no such bug is possible today with one currency; adds operators, an EF owned-type mapping, and touches every call site for no current benefit — premature per CLAUDE.md KIS

## Decision

- Replace `double`/`long` money fields with `decimal`: `StockItem.Price`, `StockItem.Factor`, `SaleLine.UnitPrice`, `Invoice.Total`, `Invoice.Tax`
- EF: `HasPrecision(18, 2)` storage on each money property — fixed column width, independent of rounding digits below
- `CompanySettings.CurrencyDecimalDigits` (int, default 0): rounding precision for invoice totals/tax, set per company. Owner's main market is Paraguay (PYG), a zero-decimal currency, so the default keeps today's whole-unit behavior instead of assuming 2
- `InvoiceCalculator.CalculateTotal`/`CalculateTax` take `currencyDecimalDigits` and round to it with `Math.Round(value, digits, MidpointRounding.AwayFromZero)` (changes current ToEven/banker's behavior); `StockItem.Price`/`Factor` stay unrounded — no currency-digit enforcement on entry
- No `Money` value object, no `Currency`-linked type yet — `CompanySettings.Currency` (#31, ISO 4217 code) and `CurrencyDecimalDigits` are independent settings, not derived from each other; single currency per company stays implicit

## Consequences

- Unblocks price import (erp-gaps.md #4)
- A migration is needed for existing `double`/`long` columns → `numeric(18,2)`, plus the new `CurrencyDecimalDigits` column (default 0)
- Rounding behavior changes from ToEven to AwayFromZero: totals/tax on existing invoices may recompute slightly differently if ever re-run
- Cross-currency mistakes stay uncaught by the compiler until a `Money`/`Currency` type is introduced later, if ever needed
- `TaxDivisor`-style flat-VAT math still lives in `InvoiceCalculator`; #31/#12 (VAT rates in settings) is a separate change
- Setting `Currency` to a 2-decimal code does nothing on its own — `CurrencyDecimalDigits` must be set to match; no validation ties the two together yet
