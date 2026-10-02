# ADR-0026: Cross-domain search via Postgres full-text + trigram

- Status: Accepted
- Date: 2026-09-28

## Context

- [ADR-0025](0025-kora-design-system.md)'s `CommandPalette` is now scoped as full cross-domain search (Facturas, Clientes, Productos, Proveedores), not just navigation - typing "cemento" or "gs 4521" from anywhere must return ranked, grouped matches fast.
- [ADR-0008](0008-ef-core-on-postgresql.md): one EF Core context, PostgreSQL.
- Grilled with the owner: stock items and customers - thousands of rows; invoices/orders - tens of thousands and growing, called out as needing special attention. Typo/accent tolerance matters (Paraguayan Spanish input, RUC/name misspellings). Default to active records only (no `Anulada` invoices, no filtered-out stock), with a filter to widen.

## Options

- **Postgres full-text search** (`tsvector` generated column + GIN index) for prefix/word matching, plus `pg_trgm` (trigram similarity + a GIN/GiST index) for typo and accent tolerance - one datastore, no new infrastructure.
- **A dedicated search engine** (Meilisearch, Typesense, Elasticsearch) - better relevance tuning and horizontal scale, but a second system to run, back up, and keep in sync with Postgres (dual-write or CDC), and new infra for an SMB-scale app to operate.
- **In-memory/client-side filtering** (today's stock-item search: `Array.filter` in the browser) - breaks down long before "tens of thousands" of invoices; no ranking, no typo tolerance, ships the whole table to the client.

## Decision

- Postgres full-text search + `pg_trgm`, no new infrastructure:
  - Each searchable table (`StockItems`, `Customers`, `Invoices`, `Suppliers`) gets a generated `search_vector tsvector` column over its identity fields (name, code, description; RUC, name, phone; invoice number, customer name; RUC, name), `GIN`-indexed, plus a `pg_trgm` `GIN` index on the same concatenated text for fuzzy/typo/accent-tolerant matches when the plain-text query yields too few hits.
  - One `GET /api/search?q=` endpoint queries all four tables (each still its own repository/query - no cross-table join), ranks each domain's matches by `ts_rank`/trigram similarity then recency, returns at most 5 per domain plus each domain's total count.
  - Filtered to active records by default (`Invoices.Status != Cancelled`, current stock-item visibility rules); a `includeInactive=true` query param widens it, driven by the `CommandPalette`'s filter chip.
  - **Invoices get the most attention**, per the owner's flag on scale: the `GIN` index and `ts_rank`-then-recency ordering keep a common query (a repeated customer name) from surfacing years of history before this week's invoice; pair with the existing `CreatedAt` index already implied by list sorting.
- Revisit trigger, not a premature switch: if any single table passes roughly 500k rows, or p95 search latency exceeds ~200ms in production, re-evaluate a dedicated search engine then - not before.

## Consequences

- New EF Core migration per searchable table: generated `tsvector` column + trigger/generated-always-as, `GIN` index, `pg_trgm` extension enabled once at the database level.
- `search_vector` must be kept in every insert/update path for these entities (or generated as a stored computed column so EF/Postgres keeps it in sync automatically - preferred, avoids an application-level bug class).
- Query-side: one new endpoint, one new `search.ts` client function; no change to existing list endpoints or their pagination.
- No new service to deploy, back up, or monitor; search quality is bounded by what Postgres FTS/trigram can do - materially worse relevance than a dedicated engine at very large scale, accepted per the revisit trigger above.

## Amendments (implementation, 2026-10-02)

- `word_similarity()`, not `similarity()`: a query like "cemento" is short next to a long concatenated identity-field string, and whole-string `similarity()` is diluted by length - `word_similarity()` matches against the best substring and is what actually finds typos in practice. Threshold 0.3.
- Invoices' generated column can only cover its own table (`Number`) - Postgres generated columns can't reference another table. Customer name/RUC match is a join to `Customers` done live in the query, not pre-baked into `Invoices.search_vector`.
- No separate recency index needed: `Invoices.Date` already supports `ORDER BY ... DESC`; StockItems/Customers/Suppliers have no natural recency column and are ranked by score then name - recency only mattered for Invoices per the scale concern above.
