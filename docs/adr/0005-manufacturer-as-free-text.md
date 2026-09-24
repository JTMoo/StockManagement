# ADR-0005: Manufacturer as free text

- Status: Proposed
- Date: 2026-09-24

## Context

- `ManufacturerType` enum hardcoded three manufacturers of one customer
- Any other catalogue can't be entered or imported
- Mongo stored the enum as a number

## Options

- Free text on `StockItem` (like `Location`), suggestions from names in use
- Own `Manufacturer` collection referenced by id
- Keep enum, add values per customer

## Decision

- Free text `StockItem.Manufacturer`, trimmed
- Suggestions = distinct names in use, case-insensitive
- `ManufacturerSerializer` reads old numbers (0-3) as names; saves write names

## Consequences

- No migration step; old documents convert on next save
- Spelling variants ("Samasz" / "SAMASZ") count as one in lists; no rename-all yet
- Own collection stays possible later (e.g. supplier data, #25)
