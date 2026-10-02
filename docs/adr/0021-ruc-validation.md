# ADR-0021: RUC validation and name-based duplicate matching

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

## Name-based matching (#64 second half)

- Only kicks in when `IdentificationNumber` is blank, the common legacy case; a row with any identification number still matches on that alone, name is never checked for it
- `NameNormalizer.Normalize(string?)` (`Kernel/Util`, pure, no DI, same shape as `RucValidator`): strips accents, lowercases, drops a closed set of PY company suffixes (`SA`, `SRL`, `SAC`, `EIRL`, `LTDA`, `EAS`) and sorts the remaining words — "José Pérez" / "PEREZ JOSE" and "Acme S.A." / "Acme SA" all collapse to the same key
- `CustomerImportTargetHandler.SplitDuplicatesAsync`: the name key is matched against every existing customer's `Name`+`Lastname` (whether or not that customer has an identification number) plus earlier candidates in the same batch, mirroring the existing-ID check
- No persistence change: this stays an import-time check only (`DuplicateFilterResult.Duplicates`, never silently dropped, never blocked — same as the rest of ADR-0018)

## Consequences (name matching)

- Word-order and suffix stripping can over-match (two different "Jose Perez"); harmless here since a duplicate only means "flagged for review in this batch", not rejected or merged
- The suffix list is fixed, not configurable; a PY suffix missing from it just doesn't get stripped, same trade-off as RUC's own "no rejection" stance
