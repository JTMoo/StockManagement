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
- EF: `HasPrecision(18, 2)` on each money property (2 decimal digits fixed for now; revisit if a zero-decimal currency like PYG is configured in #31)
- Keep existing rounding behavior: `Math.Round(value, 2, MidpointRounding.ToEven)` in `InvoiceCalculator`, same policy, just at 2 decimal places instead of 0
- No `Money` value object, no `Currency` type yet — single currency stays implicit until #31 introduces company settings; revisit then if multi-currency is actually needed

## Consequences

- Unblocks price import (erp-gaps.md #4)
- A migration is needed for existing `double`/`long` columns → `numeric(18,2)`
- Cross-currency mistakes stay uncaught by the compiler until a `Money`/`Currency` type is introduced later, if ever needed
- `TaxDivisor`-style flat-VAT math still lives in `InvoiceCalculator`; #31/#12 (VAT rates in settings) is a separate change
