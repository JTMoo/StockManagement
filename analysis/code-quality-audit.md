# Code quality + dead code audit

Cycle 1, 2026-10-02. Repo: JTMoo/Kora @ main (6ea47fb). Recurring — rerun after each merge batch, append a new dated section below instead of rewriting.

## Scope

- C#: Kernel, `*.Core` + `*.Core.Contracts`, Infrastructure, Api, Api.Tests, Tests, Language
- React: StockManagement.Web
- Electron/scripts/CI: StockManagement.Desktop, scripts/, .github/workflows/, top-level config

Tooling check: no .NET analyzers configured beyond SDK defaults; `tsconfig.json` has `noUnusedLocals`/`noUnusedParameters: true` (catches basic TS dead code in CI already — not re-reported here). No ESLint config.

## Dedup against existing issues

Already filed, not re-reported: #112 (Kernel dead code), #97 (`ISaleService` Singleton/Scoped), #76 (no Ef-prefix).

## Dead code (all grep-verified repo-wide)

| # | File | What | Size | Issue |
|---|------|------|------|-------|
| 1 | `StockManagement.Kernel/Model/ExtensionMethods/ObservableCollectionExtensions.cs` | `EqualizeTo<T>` on `ObservableCollection<T>` — WPF-only type, zero refs | S | #112 |
| 2 | `StockManagement.Kernel/Model/ExtensionMethods/EnumExtensions.cs` | `GetEnumDescription` — zero refs | S | #112 |
| 3 | `StockManagement.Kernel/Model/ExtensionMethods/ListExtensions.cs` | `EqualizeTo<T>`, `RemoveUnavailableItems` — zero refs | S | #112 |
| 4 | `StockManagement.Kernel/Model/ExtensionMethods/IEnumerableExtensions.cs` + `StockManagement.Tests/Kernel/IEnumerableExtensionsTests.cs` | `ConvertToShoppingCartList`, `Where(List<Func<T,bool>>)` — zero production callers, only self-test | S | #112 |
| 5 | `StockManagement.Kernel/Model/ExtensionMethods/StringExtensions.cs` + `StockManagement.Tests/Kernel/StringExtensionsTests.cs` | `ReplaceLineBreakWithWhitespace`, `MatchesSearch` — zero production callers, only self-test | S | #112 |
| 6 | `StockManagement.Kernel/StockManagement.Kernel.csproj` | Unused `PackageReference`: `ClosedXML`, `Microsoft.Extensions.Configuration.Abstractions`, `Microsoft.Extensions.DependencyInjection.Abstractions` | S | #112 |
| 7 | `StockManagement.Web/src/api/stockItems.ts:9` | `listStockItemsBelowMinimum` exported, zero callers — `StockItemList.tsx` reimplements the filter client-side instead | S | new |
| 8 | `StockManagement.Web/src/index.css` | `button.danger`/`.danger` selector — zero className usage | S | new (ties into quality #1 below) |

Not dead, contradicts/refines original #112 framing: `BaseDocument`/`ManufacturerSerializer`/`MongoDB.Driver` are correctly kept, but it's not just `Model/StockItem` reusing the Mongo POCO as EF entity — **every** `Kernel.Model.*` class (`Invoice`, `User`, `Customer`, `CreditNote`, `AppSettings`, `Transaction`, `Supplier`, `ImportBatch`) is a live `DbSet<T>` in `AppDbContext.cs:24-40`. `NotificationBase` (WPF `INotifyPropertyChanged`) still backs `ShoppingCartItem`/`Payment` but nothing in the API consumes `PropertyChanged` — not zero-ref so not listed as dead, worth a call in #112 on dropping it to plain auto-properties.

Electron/scripts/CI: **no dead code found.** Every script/workflow verified referenced; `Delivery.yml` confirmed gone (replaced by `Release.yml`), nothing left pointing at it except a stale CLAUDE.md line (see below).

## Quality issues (ranked, highest impact first)

1. **[HIGH, M] DI lifetime mismatch — same bug class as #97, much wider.** Every `*.Core` module except `CreditNoteService` registers `AddSingleton` while depending on a `Kernel` `I*ServiceProvider` that Infrastructure registers `AddScoped`:
   - `Auth.Core`: `AuthService` → `IUserServiceProvider`
   - `Customers.Core`: `CustomerService` → `ICustomerServiceProvider`
   - `Import.Core`: `StockItemImportService`, all 3 `IImportTargetHandler`s, `ImportBatchService` → scoped providers
   - `Sales.Core`: `SaleService` (#97), `PaymentService` → `IInvoiceServiceProvider`
   - `Settings.Core`: `SettingsService` → `ISettingsServiceProvider`

   Hidden because `StockManagement.Tests/ServiceRegistrationTests.cs:26-32` mocks every provider `AddSingleton` instead of `AddScoped`, so `ValidateOnBuild` never trips in tests. Fix: switch all to `AddScoped`; fix the test to register mocks `AddScoped` inside a scope so it actually catches this class of bug going forward. One PR, resolves alongside #97.

2. **[HIGH, L] Missing `CancellationToken` on async Kernel/Infrastructure methods.** CLAUDE.md requires it; 6 of 8 provider interfaces have zero `CancellationToken` params across ~35 methods (`ICustomerServiceProvider`, `IImportBatchServiceProvider`, `ISettingsServiceProvider`, `IStockItemServiceProvider`, `ISupplierServiceProvider`, `IUserServiceProvider`) and their `Ef*ServiceProvider` implementations don't thread one into EF async calls. FastEndpoints already has a token at the endpoint; it's dropped at the provider boundary. Mechanical but touches interfaces + implementations + call sites + test mocks.

3. **[M] Delete actions bypass the Kora design system's `danger` Button + confirm step.** `StockItemList.tsx:48`, `SupplierList.tsx:24` use `window.confirm()` + `.quiet` button; `UserList.tsx:52` has no confirm step at all before delete. Design doc reserves `danger` fill + an in-page confirm step for exactly this case (see `InvoiceView.tsx:76-90`'s cancel-invoice pattern for a model to reuse).

4. **[M] Stock status shown via border-only CSS, not `StatusBadge`.** `StockItemList.tsx:60-62` marks below-minimum/sold-out rows with a border class only — design doc explicitly says "pair with `StatusBadge`, never the border alone." Same gap on invoice status (`InvoiceView.tsx:52-53`, plain `<span>`; `InvoiceList.tsx` shows no status at all) — one `StatusBadge` component, reused across stock items + invoices + import rows, fixes both.

5. **[L] Create/edit forms rendered permanently inline instead of a `Dialog`.** `StockItemList.tsx:64`, `CustomerList.tsx:33-35` (and by the same shape `SupplierList`, `UserList`) match verbatim the problem the design system's `Dialog` doc was written to replace. Blocked on `Dialog` existing — track as a follow-up once it's built, not urgent alone.

6. **[S]** 3 contract records missing `<summary>` XML doc (CLAUDE.md rule): `Import.Core.Contracts/DetectedColumn.cs:4-6`, `ImportRowError.cs:4-5`, `ImportField.cs:4-5`.

7. **[M]** `Electron main.js` `waitForApi()` only checks for network error, not HTTP status — a 500/404 response is treated as "API ready." Fix: check `response.statusCode` in 200-299.

8. **[S]** `Electron main.js` has no user-facing failure path when the bundled API never starts within the timeout — only `console.error`'d, window still opens on a dead URL. Add `dialog.showErrorBox`.

9. **[S]** `scripts/quickstart.ps1:34-40` — `dotnet restore`/`npm ci` failures aren't checked (`$ErrorActionPreference = Stop` doesn't catch non-zero exit from native `.exe`s); script prints bogus "Setup done." Add `$LASTEXITCODE` checks like the existing `psql` check.

10. **[S]** Stale docs: `CLAUDE.md:31` still lists `Delivery.yml (MSI on tag)` and "Windows tests" — both gone since ADR-0011/PR #79 (now `Release.yml`, Electron installers, all-Ubuntu CI). `CLAUDE.md:51,57,59` still say "one `IMongoClient`", "Mongo duplicate key → domain error", "no `.msi` in git" — stale since the Postgres/EF Core (ADR-0008) and Electron (ADR-0013) moves. `.github/workflows/Integration.yml:1,3` has a trailing-space typo in the workflow name and a stale "master branch" comment (trigger itself correctly says `main`).

11. **[S]** `.github/workflows/sync-main-into-prs.yml:29` — `gh pr list` has no `--limit`, silently caps at 30 open PRs. Add `--limit 100` defensively.

## Low-value tests / coverage gaps

- `StockManagement.Tests/Kernel/IEnumerableExtensionsTests.cs`, `StringExtensionsTests.cs` — test only the dead code above; delete alongside it.
- `ServiceRegistrationTests.cs:26-32` — actively masks quality issue #1 (mocks Scoped as Singleton); fix alongside #1, not just a coverage note.
- No unit test for `TypeExtensions.cs` despite real production use in `Import.Core/ExcelEntityParser.cs` — gap, not sized.

## Files verified clean (checked, nothing found)

- Kernel: `BaseDocument`, `ManufacturerSerializer`, `PagedResult`, `EntityChangeType`, `IEntityChangedHandler`, all 8 `*ServiceProvider` interfaces, all 11 exception types, all `Model/*` entities + enums, `StockItemExtensions`, `TypeExtensions`, `ConversionHelper`, `RucValidator`, `SequenceNumber`
- Infrastructure: all `Ef*ServiceProvider`, `*Configuration`, `AppDbContext`, design-time factory
- All `*.Core`/`*.Core.Contracts` modules, `Api/Features/**`, `Language/**`
- No `async void`, empty `catch`, fire-and-forget, or `double`-for-money found anywhere in scope
- Web: `src/api/*.ts`, `App.tsx`, `routes.ts`, i18n pipeline, `Page.tsx`/`FailureMessage.tsx`/`useLoad.ts`/`test-utils.tsx`/`main.tsx`/`auth.tsx`, `permissionOptions.ts`/`PermissionCheckboxes.tsx`, all test files, e2e suite, CSS (except `.danger`)
- Electron/scripts/CI: `Desktop/package.json` + lockfile, `.gitignore`, `quickstart.sh`, `generate-adr-index.sh`, `Release.yml`, `sync-main-into-prs.yml` (aside from #11), `.editorconfig`, `README.md`; PR #111's claude-review.yml fix verified coherent, no infinite-loop risk

## Actions taken this cycle

- Dead code items #1-#8 above: PR opened (zero behavior change, see PR link in issue tracker)
- Quality issues #1-#11: filed as GitHub issues (see issue numbers in tracker)

## Cycle 2, 2026-10-02

Repo @ main (42b3afa). Delta since cycle 1 - merged: cursor pagination (ADR-0029), #52 validation codes, #113 CancellationToken, per-item VAT, invoice composite numbering, Sifen.Core + gateway/outbox worker, landed cost (#120), open-invoice import, name-matching, design-system rollout (Dialog/CommandPalette/DataTable/StatusBadge/Toast), `/api/search`, `sync-main-into-prs.yml` fix.

### Findings (new)

| # | File | What | Issue |
|---|------|------|-------|
| 1 | `StockItems/CheckInStockItemEndpoint.cs`, `CheckOutStockItemEndpoint.cs` | No `Permissions()` call - any authenticated user can mutate stock | #151 |
| 2 | `IInvoiceServiceProvider`, `ICreditNoteServiceProvider` | Missing `CancellationToken`, same bug class as #113 but excluded from its "6 of 8"; includes the new ADR-0029 cursor-pagination method, so `EfInvoiceServiceProvider`'s `.ToListAsync()` for it also drops the token | #152 |
| 3 | `StockItemList.tsx`, `SupplierList.tsx`, `UserList.tsx` | `window.confirm()` / no confirm at all on delete, no `danger` Button exists - cycle-1 finding, #105 closed without fixing it | #153 |
| 4 | `Invoices.resx` (`cash`, `credit`, `exceptionShoppingCartItemOutOfRange`, `invoice`), `StockItems.resx` (`checkin`, `checkout`) | More of #137's pattern: `Designer.cs` getter, no matching `<data>` entry in any locale | comment on #137 |

### Verified clean / no new dead code

- `SifenTransmissionWorker`: correct `IServiceScopeFactory` use, no fire-and-forget, exceptions caught + logged
- DI lifetimes (#97/#1 fix): all `*.Core` + `Sifen.Core` services now `AddScoped`, matches their Kernel/EF provider dependencies
- Cursor pagination (#107/ADR-0029): `Customer`/`StockItem`/`Supplier`/`User` providers all correctly threaded with `CancellationToken`; only `Invoice` missed (finding #2 above)
- `SearchEndpoint`: no endpoint-level `Permissions()` by design (filters per-domain internally, documented in its `<summary>`) - not a gap
- `/api/search`, `sync-main-into-prs.yml`, `AllocateLandedCostEndpoint`, open-invoice import target, name-matching: no dead code, no missing CancellationToken, Permission checks present
- No new `async void`, empty `catch`, fire-and-forget, or `double`-for-money

### Actions taken this cycle

- No dead code to remove (nothing newly zero-ref found)
- Findings above filed as #151-#153 + comment on #137
