# ADR-0008: EF Core on PostgreSQL, one context, change handlers

- Status: Proposed
- Date: 2026-09-24
- Issue: [#65](https://github.com/JTMoo/StockManagement/issues/65)

## Context

- Hand-written Mongo layer per entity (`*ServiceProvider`, `DatabaseManager`)
- ERP data is relational: invoices → customers → stock items, ledger, receivables
- Multi-step writes not atomic (#41); Mongo transactions need a replica set
- Owner wants one context with all sets and loosely coupled reactions to entity changes

## Options

| Option | For | Against |
|---|---|---|
| EF Core + MongoDB provider | no data move | no joins/`Include`, no migrations, no `ExecuteUpdate`, replica set for transactions |
| EF Core + PostgreSQL (Npgsql) | FKs, joins, migrations, transactions, `numeric` money | data move from Mongo, Postgres on customer machine |
| Keep Mongo driver | nothing to do | no change tracking, no unit of work |

## Decision

- EF Core + PostgreSQL, new `StockManagement.Infrastructure` project
- One `AppDbContext`, one `DbSet` per entity, mapping via `IEntityTypeConfiguration<T>`
- Reactions: `IEntityChangedHandler<T>` (Kernel), called in `SaveChangesAsync` before the commit
  - Rounds until handlers make no new changes (max 10), then one save = one transaction
  - Handler throws → nothing saved
  - Each entity handled once per change type per save
- Connection: `ConnectionStrings:Postgres`

## Consequences

- Migration per entity: Mongo `*ServiceProvider` → `AppDbContext`, then drop MongoDB.Driver
- Existing installs: one-off Mongo → Postgres import
- `$inc` counters and conditional stock `$inc` (#41) → `ExecuteUpdateAsync` or concurrency tokens
- Duplicate key → `DbUpdateException` (Postgres `23505`) mapped to domain error in Infrastructure
- Schema via EF migrations once the first entity is live (slice 1 uses `EnsureCreated` in tests)
- API tests: Testcontainers PostgreSQL
- Installer ships or requires PostgreSQL
- Sync `SaveChanges` throws: handlers are async

## Slice 2 (StockItem)

- `EfStockItemServiceProvider` (Infrastructure) implements Kernel's `IStockItemServiceProvider`; `BaseDocument.Id` made `public` (was `internal`, per ADR-0004's own follow-up) so Infrastructure can update by Id
- `IStockItemServiceProvider` no longer returns Mongo's `ReplaceOneResult`/`DeleteResult`; both writes now return `int` (1 on success, throws otherwise)
- Migrations: `dotnet ef migrations add`, `StockManagement.Infrastructure/Database/Migrations`, design-time factory since `AppDbContext` takes `IServiceProvider`
- `Transaction.Time` mapped `timestamp without time zone`: `DateTime.Now` is local, Npgsql only accepts UTC for `timestamptz`
- `Transaction.Invoice` not mapped yet (Invoice isn't in Postgres); ignored in `TransactionConfiguration`
- Gotcha: adding a dependent entity before updating/removing its principal marks the principal `Added` via graph fixup; `EfStockItemServiceProvider` updates/removes the `StockItem` first, then adds the `Transaction`
- **Not done**: `EfStockItemServiceProvider` isn't wired into the API or GUI. `InvoiceServiceProvider.TryAddSaleAsync` decrements stock and writes the invoice in one Mongo session transaction (ADR-0007); moving `StockItem` alone would split that atomic write across two databases. Wiring StockItem into the running app needs that crossing solved first — customers/invoices next, or an outbox/saga for the sale — asked the owner rather than picking silently
