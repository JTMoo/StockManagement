# ADR-0020: Stock item purchase price, additional cost, and exchange rate

- Status: Proposed
- Date: 2026-09-27

## Context

- #28: stock item data needs Price (Purchase), Additional Cost of Purchase, and Exchange Rate (date of purchase), sale price still derived via the existing (currently unused) `Factor`
- #28 also lists discounts on sale and partial sales — unrelated to pricing storage, out of scope here
- `StockItem.Factor` exists on the model/migrations already but no code reads or writes it; no API field exposes it

## Options

- Store purchase price per purchase lot (supports "individually" or "average over all purchases" from #28) / store one current purchase price per stock item, overwritten on each purchase
- Derive `Price` automatically from purchase data + `Factor` / keep `Price` always manually entered, purchase fields for record only
- Round the derived price using `CompanySettings.CurrencyDecimalDigits` (ADR-0014) / a fixed precision

## Decision

- One current value per stock item, not per-lot: `PurchasePrice`, `PurchaseExchangeRate`, `AdditionalPurchaseCost` (all `decimal`, default 0) alongside the existing `Factor`. A new purchase overwrites these; no purchase history. Averaging and per-lot tracking are deferred — not needed for a single-location shop, revisit if requested.
- `StockItemExtensions.CalculateSalePrice(purchasePrice, exchangeRate, additionalPurchaseCost, factor, currencyDecimalDigits)`: `Round((purchasePrice * exchangeRate + additionalPurchaseCost) * factor, currencyDecimalDigits, AwayFromZero)`, same rounding as `InvoiceCalculator` (ADR-0014).
- `Create`/`UpdateStockItemEndpoint`: when `Factor > 0`, `Price` is overwritten with the calculated value (reads `CompanySettings` via `ISettingsService`, same pattern as `SaleService`). When `Factor == 0` (default for stock items with no purchase data, e.g. ones created before this change), `Price` stays manually entered — backward compatible.
- `PurchaseExchangeRate` precision `(18, 6)` (a rate, not itself money); `PurchasePrice`/`AdditionalPurchaseCost` precision `(18, 2)` like other money fields.
- `Factor` is now exposed on create/update/response (was model-only, unused).

## Consequences

- Editing purchase fields after a purchase silently loses the previous purchase's numbers (no lot history); acceptable for now, would need a new entity if per-lot data is ever required
- A stock item with `Factor > 0` can no longer have `Price` edited directly — the UI submits the same `Price` value regardless, so this only matters for direct API callers
