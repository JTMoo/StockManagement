# ADR-0025: Import column mapping

- Status: Proposed
- Date: 2026-09-28

## Context

- #58: "Web: column mapping, preview, error report download" — the pipeline stages in docs/decisions.md are `read → map → clean → validate → preview → commit`; mapping is its own stage, ahead of preview
- Today `ExcelEntityParser<T>` auto-matches each header to a property by exact name or localized `[Display]` value (case-insensitive) and silently drops anything that doesn't match — no error, no report, just missing/default fields. A legacy file with renamed or foreign-language headers imports with empty columns and no indication why
- `IImportTargetHandler.ParseAsync(Stream, CancellationToken)` takes only the raw file; there's no way for a caller to influence or even see the match

## Options

- Column identity sent back to the server: 1-based column number / header text (ambiguous on duplicate headers)
- Field identity in the mapping: target property's C# `Name` (stable, locale-independent) / its localized `Display` value (breaks across cultures, same problem as today)
- UI shape: a separate "map columns" step between file upload and preview (matches the documented pipeline order) / fold mapping into the existing single upload-and-preview call with inline correction after the fact

## Decision

- New `IImportTargetHandler` members: `GetFields()` (no I/O; the target's importable fields as `Name` + localized `DisplayName`, from `ExcelEntityParser<T>` reflection) and `DetectColumnsAsync(Stream, CancellationToken)` (reads only the header row; returns each column's 1-based number, header text, and the auto-matched field `Name` or `null`)
- `ParseAsync` widens to `ParseAsync(Stream, IReadOnlyDictionary<int, string>? columnMapping, CancellationToken)`: column number → property `Name`. `null` keeps today's auto-match (`ExcelEntityParser`'s existing `MatchHeadersToProperties`); a supplied mapping parses by column number against the named property only, unknown/missing entries treated as ignored columns
- New `GET /import/batches/fields?target=` and `POST /import/batches/columns` (Target + File, same shape as preview) endpoints expose fields and detected columns to the web app. `POST /import/batches` (preview) gains an optional `Mapping` form field: a JSON object of column number → field name, built from the confirmed mapping
- Web: `ImportPanel`'s "Preview" click now runs column detection first and shows a mapping table (header → dropdown of target fields, defaulted to the auto-match, plus "ignore this column"); a second "Preview" click sends the confirmed mapping and shows the existing Ready/Duplicate/Error preview unchanged
- Column identity is the column *number*, not header text: duplicate headers stay distinguishable. Field identity is the property `Name`, not `DisplayName`: stable across the three cultures

## Consequences

- `ParseAsync`'s new parameter is a breaking signature change for `IImportTargetHandler`, same shape as ADR-0019's `UndoAsync` widening; all three handlers pass it straight through to `ExcelEntityParser<T>`, unchanged otherwise
- Column mapping is still silent about fields nothing was mapped to and about mapped-but-empty cells — same as today; only the previously-invisible "this header didn't match anything" case becomes visible and correctable
- One more round trip before preview (detect, then confirm) for every import, even when auto-match already covers the whole file; accepted since the header row is cheap to read and the pipeline stage order was already documented this way
