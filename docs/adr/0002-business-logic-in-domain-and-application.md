# ADR-0002: Move business logic out of view models into Domain and Application layers

- Status: Accepted
- Date: 2026-09-24
- Deciders: Jonathan Trefz

## Context

The rules that matter (a sale reduces stock, invoice numbering, customer id generation, duplicate detection on
import, total and tax calculation) live in WPF view models and extension methods in `StockManagement.Gui`. They
cannot be reused by an API, are not unit tested, and some have bugs (non-atomic sale, `Max + 1` numbering).
See the [codebase analysis](../knowledgebase/codebase-analysis-2026-09.md).

## Options considered

1. **Keep logic in the UI layer and port it to React later**: fastest now, but duplicates rules per frontend and
   keeps them untested.
2. **Move logic into `StockManagement.Kernel` services**: Kernel already mixes persistence, WPF binding models and a
   command queue, so the rules would stay tangled with Mongo.
3. **New `StockManagement.Domain` (pure rules, no framework references) and `StockManagement.Application` (use
   cases over repository interfaces)**, with an Infrastructure layer for Mongo and Excel later.

## Decision

Option 3. Pure rules go to Domain, use cases to Application; the WPF view models and later the API endpoints only
call into them. Every rule gets unit tests as it moves. Started in PR
[#34](https://github.com/JTMoo/StockManagement/pull/34) (Application still references Kernel until an
Infrastructure project exists).

## Consequences

- The WPF app keeps working during the migration and the API can reuse the same services.
- Rules become unit testable without Mongo or WPF.
- Follow-up: extract `StockManagement.Infrastructure` (Mongo repositories, Excel reader) and drop the Kernel
  dependency from Application.
