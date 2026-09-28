# ADR-0021: RUC validation and duplicate matching

- Status: Proposed
- Date: 2026-09-27

## Context

- #64: `Customer.IdentificationNumber` is free text; legacy data has typos, missing check digits, and the same customer twice (`"1234567-8"` vs `"12345678"`)
- Paraguay's SET check digit: base digits x weights 2..11 (cycling, right to left), `remainder = sum % 11`, digit = `remainder <= 1 ? 0 : 11 - remainder`
- `CustomerImportTargetHandler.DuplicateKey` (ADR-0018) already de-dupes on `IdentificationNumber`; verified against two independent worked examples (`5530638` → `1`, `1946520` → `3`)

## Options

- Reject a non-empty, wrong-check-digit `IdentificationNumber` (create/update `FluentValidation`, import per-row error) / only normalize when it verifies, leave everything else exactly as entered
- Uniqueness: unique index on the stored (post-normalization) value / no index, dedupe only in the import pipeline

## Decision

- **No rejection.** `Customer.IdentificationNumber` holds either a C.I. (plain digits, no check digit) or a RUC — the label (`"Identification number (C.I. / RUC)"`) and existing tests both use arbitrary non-RUC strings. Rejecting anything that fails the RUC checksum would reject legitimate C.I. entries, so validation is normalize-when-possible, never block
- `RucValidator.TryNormalize(string?, out string)` (`Kernel/Util`, pure, no DI — same shape as `ConversionHelper`): strips non-digits, verifies the check digit, writes `"digits-checkDigit"` on success; returns `false` for anything else (including a plain C.I.) and leaves the value untouched
- Create/update endpoints and `CustomerImportTargetHandler.ParseAsync` all call it the same way: normalize when it verifies, otherwise keep the value as entered. This silently converts `"12345678"` and `"1234567-8"` to the same stored form so they collide as duplicates, without touching non-RUC data
- `CustomerConfiguration`: filtered unique index on `IdentificationNumber` (`WHERE "IdentificationNumber" <> ''`), so blank stays unconstrained; this is what actually catches the "same customer twice" case now that both RUC spellings normalize identically. New `CustomerIdentificationNumberAlreadyExistsException`, told apart from `CustomerIdAlreadyExistsException` by Postgres constraint name (same `SaveChangesAsync` pattern as the existing one). Create/update endpoints catch it and return `409 Conflict`
- `DuplicateFilter`'s key is unchanged: `ParseAsync` already normalizes before candidates reach it, so RUC-shaped duplicates match regardless of `-DV` formatting; non-RUC values keep matching by raw value (case-insensitive), as before

## Consequences

- A RUC with a genuinely wrong check digit is stored as-is, unflagged — there is no per-row report for it. Simpler and safer than guessing which values are "meant to be" a RUC (see Options)
- Existing stored `IdentificationNumber` values are not backfilled to the normalized form; the new unique index only catches clashes going forward
- Duplicate matching by normalized customer name (#64's second half) is not covered here; left for a follow-up
