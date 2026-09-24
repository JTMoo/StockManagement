> Snapshot from 2026-09-24 (main at `c78ad65`). Kept for reference; the living rules are in [CLAUDE.md](../../CLAUDE.md) and
> [decisions.md](../decisions.md). Since then: issue 1 (unclosed `#region`) was fixed in PR #33 and issue 2 (invoice
> update by `Number`) in PR #32.

# StockManagement: codebase analysis and coding guidelines (draft)

Source: `JTMoo/StockManagement`, `main` at `c78ad65` ("UI User login"), 50 commits, all by Jonathan Trefz.
Everything below comes from reading the code. Where something is inferred and not verified by a build, it says so.

---

## 1. Current architecture and tech stack

| Area | What is there |
|---|---|
| Runtime | .NET 8 (`net8.0`, GUI `net8.0-windows`), C# latest, `Nullable` enabled (but CS8618 silenced in `.editorconfig`) |
| UI | WPF, MVVM by hand (no toolkit), `Microsoft.Xaml.Behaviors.Wpf`, `FontAwesome.WPF`, Poppins font, dark theme via resource dictionaries |
| Data | MongoDB (`MongoDB.Driver` 3.0), local `mongodb://127.0.0.1:27017`, DB name hard-coded `LaCosecha_StockManagement` |
| Excel | `ClosedXML` for the Excel import (sheet pick, then column-to-property mapping) |
| i18n | Separate `StockManagement.Language` project with `.resx` files: de-DE, en-US, es-PY (~92 keys each) plus `Numbers.resx` for number-to-words (Spanish-style, used on invoices) |
| Tests | MSTest 3.6 + Moq, 5 tests in 2 classes |
| Packaging | WiX v3 installer; built `.msi` files are committed to the repo |
| CI/CD | GitHub Actions on `windows-latest`: `Integration.yml` runs `dotnet test` on push/PR to `main`; `Delivery.yml` builds the MSI and creates a release on any tag |

### Projects

```
StockManagement.Kernel     business logic + persistence (class library)
StockManagement.Gui        WPF app (views, view models, converters, selectors)
StockManagement.Language   resx resources, referenced by Kernel and Gui
StockManagement.Tests      MSTest, references Kernel only (InternalsVisibleTo)
StockManagement.Installer  WiX
```

### Domain (Kernel/Model)

`StockItem` (Code unique, Name, Amount, Price, Factor, Location, Manufacturer, Misc), `Customer` (CustomerId unique, names, address, contact, Misc), `Invoice` (Number unique, Customer embedded, Items = `ShoppingCartItem` list embedded, Total, Tax, SaleCondition, dates), `Transaction` (stock movement history: Amount / Price / Deletion), `Settings` (language, dialog border, "index created" flags), `User` (empty stub, login is in progress).

All documents derive from `BaseDocument : NotificationBase`, so the persistence models are also the WPF binding models (INotifyPropertyChanged + `[Display(ResourceType=...)]` attributes).

### Flow

`App.OnStartup` -> `MainManager.Initialize()` (singleton; loads Settings, creates unique indexes once, starts the command loop) -> `MainViewModel.CreateAsync(IDatabase)` which news up the four `*ServiceProvider`s and passes them into view models via async factory methods.

Two ways into the Kernel exist side by side:
1. **Service providers** (`IStockItemServiceProvider`, `ICustomerServiceProvider`, `IInvoiceServiceProvider`, `IUserServiceProvider`) over `IDatabase`: used for almost everything.
2. **Command queue** (`ICommand` + `CommandData` + `CommandManager` polling every 500 ms on a background task, reached through `MainManagerFacade.PushCommand`): used only by `ChangeSettingsCommand`.

The business logic that matters (sales reduce stock, invoice numbering, customer id generation, duplicate detection on import, total and tax 1/11 calculation) currently lives in **GUI view models and extension methods**, not in the Kernel services. This is the main thing the API move has to fix.

---

## 2. Observed conventions (the owner's style)

### Formatting
- **Tabs** for indentation (90 of ~100 `.cs` files; a few older files use 4 spaces).
- **File-scoped namespaces** everywhere, namespace mirrors folder path.
- Two blank lines after the `using` block/namespace and between the field block and the constructor.
- Opening braces on their own line (Allman).
- `this.` used for instance members most of the time (not always).
- Guard clauses on one line: `if (x == null) return;` / `if (x is not Foo foo) return false;`.
- Pattern matching with `is not T name` is used heavily as a guard and cast in one step.
- Collection expressions `[]` and target-typed `new()` are preferred.
- Primary constructors used for services, simple models, exceptions and some view models.
- `#region Properties ... #endregion Properties` around property blocks in models and view models.
- Big banner comment above central classes:
  ```csharp
  /// ****************************************************************
  /// <summary>
  /// Singleton - Main - Central hub for logic inside the Kernel
  /// </summary>
  /// ****************************************************************
  ```
- XML doc comments on the less obvious public methods (`EqualizeTo`, `TryConvertFromString`, `Add*Async` noting the thrown exception); simple members are undocumented.

### Naming
- Types, methods, properties: PascalCase. Interfaces `I*`. Async methods end in `Async` (mostly; `GetAll`, `Delete` in `IDatabase` do not, and there is a typo `GetInvoiceAync`).
- Private fields: mostly `_camelCase`; older models (`Customer`, `Invoice`, `ShoppingCartItem`) use plain `camelCase`.
- Suffixes carry the role: `*Manager` (singletons/central logic), `*ServiceProvider` (data access per aggregate), `*Helper` (static utilities), `*Extensions`, `*ViewModel`, `*DialogViewModel`, `*Converter`, `*Selector`, `*Command`, `*Exception`, `*Type` for enums.
- Commands on view models: `XxxCommand` property bound to a private `OnXxxCommand` handler.
- Event handlers: `OnXxx` (`OnPropertyChangedEvent`, `OnSettingsChanged`, `OnDialogClosing`).
- Tests: `Method_Scenario_ExpectedResult` (`Add_InsertNull_ReturnsFalse`) with `// Assign`, `// Act`, `// Assert` comments.

### Structure and patterns
- Folder per concern: `Model`, `Model/Types` (enums), `Model/ExtensionMethods`, `Database`, `Database/Interfaces`, `Commands`, `Exceptions`, `Util`, `Diagnostics`; GUI: `ViewModel/Primary`, `ViewModel/Dialogs`, `View/Primary`, `View/Dialogs`, `View/Controls`, `View/Dictionaries`, `Converter`, `Selectors`.
- **Async factory pattern** for view models: private ctor + `public static Task<T> CreateAsync(...)` + `private async Task<T> InitializeAsync()`. Consistently applied.
- **Constructor injection by interface** for service providers; composition root is `MainViewModel` (manual `new`, no DI container).
- **Singletons** via `static readonly Instance` (`MainManager`, `GuiManager`) and a static facade (`MainManagerFacade`) to limit what other projects see.
- **Filter-list pattern** for search: `List<Func<T,bool>>` built in `SetupFilterConditions()` and applied with a custom `Where(filters)` extension.
- **Reflection-driven UI**: template selectors map view model type to DataTemplate; Excel import maps columns to `PropertyInfo`; display names come from `[Display]` + resx.
- Unique constraints enforced by Mongo unique indexes (`UniquePropertyHelper`), with the "created" flag kept in Settings.
- Enums use `[Description]` for a technical value (culture code, manufacturer name) and `[Display]` for a localized label.
- All user-facing strings go through `Language.Resources` (a few exceptions are hard-coded, e.g. the Excel import confirmation text).

### Error handling
- Mostly **return `bool`/`null` instead of throwing** (`Push`, `Add`, `TryConvertFromString`, `DeepClone`), `Try*` + `out` for conversions.
- `try/catch (Exception)` at UI entry points, then `MessageBox.Show(localized text)` and `Trace.WriteLine(ex.Message)`.
- Logging is `System.Diagnostics.Trace` to a monthly file (`MM_log.txt`) through a custom `DateTimeTextWriterTraceListener`.
- A few custom exceptions exist (`FailedExcelConversionException`), two are empty and unused (`InvoiceNumberAlreadyExistsException`, `StockItemCodeAlreadyExistsException`).
- Duplicate keys are left to surface as `MongoBulkWriteException` (documented in XML comments), not translated into domain errors.

### Testing
- Only `CommandQueue` and one `IEnumerable` extension are tested. Tests reach private fields by reflection. No integration tests, no tests for services, import, invoicing, or number-to-words.
- `[assembly: Parallelize(Scope = MethodLevel)]` is on.

---

## 3. Issues worth knowing before the redesign

Found by reading, not by running; the first one I could not verify with a build here (no Windows/WPF build in this environment).

1. **Probably does not compile at HEAD**: `LoginViewModel.cs` opens `#region Properties` twice and never closes it (CS1038). Inferred from the source.
2. `InvoiceServiceProvider.UpdateInvoiceAsync` filters `Id` by `invoice.Number`, so with upsert it would insert a duplicate instead of updating.
3. `ConversionHelper.Ones[9]` is `ten` instead of `nine`, so amounts ending in 9 are written wrong on invoices.
4. Sale is not atomic: stock is decremented item by item, then the invoice is inserted; a failure midway leaves stock changed without an invoice. After the catch in `InvoiceCreationDialogViewModel.Confirm`, `Cancel()` and then `base.Confirm()` both run.
5. Several `InsertOneAsync` calls for transactions are fire-and-forget (not awaited), so errors are lost and ordering is not guaranteed.
6. `new MongoClient(...)` on every call; the driver expects one shared client.
7. Invoice and customer numbers are `Max + 1` over the whole collection (race-prone, loads everything).
8. Search filters are applied to the already-filtered list in `CustomerViewModel` and `InvoiceViewModel`, so clearing a search does not bring rows back. User text is passed straight into `Regex.IsMatch` (invalid pattern throws).
9. `async void` is used widely (command handlers, `Confirm` overrides, `StartObservedTask`), so exceptions escape.
10. Money is `double` on `StockItem.Price` and `long` on Invoice totals, with rounding to whole units.
11. Index-created flags are set on `Settings` but not saved back, so creation reruns each start (harmless, but misleading).
12. Hard-coded connection string and customer-specific DB name; committed `.msi` binaries; CI does not build the app itself.
13. Spelling in identifiers: `Collabsed`, `Indeces`, `Aync`, `Nineth`, `Twelveth`, `Eight`.

---

## 4. Draft guidelines for the new solution

These keep what is clearly the owner's style and replace what will not carry over to an API + React setup. Items marked **(keep)** codify existing habits; **(change)** are proposals.

### 4.1 C# style
1. **(keep)** Tabs, Allman braces, file-scoped namespaces matching folders, collection expressions, target-typed `new()`, primary constructors for services and simple types.
2. **(keep)** Guard clauses first, one line each, using `is not T x` patterns.
3. **(keep)** `this.` for instance members; `_camelCase` for private fields everywhere (fix the older plain-camelCase fields).
4. **(keep)** Role suffixes: `*Service`, `*Repository`, `*Endpoint`, `*Request`, `*Response`, `*Validator`, `*Mapper`, `*Extensions`, `*Helper`. Replace `*ServiceProvider` with `*Repository` for data access and `*Service` for business logic, since `ServiceProvider` collides with `IServiceProvider` in ASP.NET.
5. **(keep)** Every async method ends in `Async` and takes a `CancellationToken`.
6. **(change)** Turn on nullable warnings as errors; drop the CS8618 suppression and use `required` members instead.
7. **(change)** Add a fuller `.editorconfig` (indent_style = tab, naming rules above) and enforce it with `dotnet format --verify-no-changes` in CI.
8. **(keep, lighter)** XML doc comments on public API surface and non-obvious methods; drop the `****` banners and `#region`s in new code (endpoints are small enough not to need them). Owner's call.

### 4.2 Architecture
1. Projects: `StockManagement.Domain` (entities, value objects, domain rules, no framework refs), `StockManagement.Application` (use cases/services, interfaces), `StockManagement.Infrastructure` (Mongo, Excel/CSV readers), `StockManagement.Api` (FastEndpoints), `StockManagement.Language` (keep resx), `web/` (React + TypeScript + Vite). Desktop shell: wrap the React app (e.g. Tauri, Electron or WebView2) with the API running locally; decide in the next thread.
2. **Business logic lives in Domain/Application, never in endpoints or UI.** Everything currently in view models (sale, invoice numbering, customer id, import dedupe, tax) moves there first and gets tests.
3. Persistence models are not API contracts: endpoints use `Request`/`Response` records; no `INotifyPropertyChanged` in the domain.
4. Use the built-in DI container; no singletons or static facades. One `IMongoClient` registered as singleton; connection string and DB name from configuration.
5. Drop the polling command queue; FastEndpoints plus a background job/queue only where a real async job is needed (large imports).
6. One endpoint per file, grouped by feature folder (`Features/StockItems/CreateStockItem.cs` with its request, response, validator), mirroring the existing "folder per concern" habit.
7. Multi-step writes (sale = stock decrement + transactions + invoice) run in a Mongo transaction or a single atomic document update; sequential numbers come from a counter document with `$inc`, not `Max + 1`.
8. Money as `decimal` (or integer minor units) with an explicit currency; one rounding rule in one place.

### 4.3 Error handling and logging
1. **(keep)** `Try*` + `out` or result types for expected failures (parsing, import rows). **(change)** Validation via FastEndpoints validators (FluentValidation); domain errors map to RFC 7807 problem details with a localized message key.
2. Translate Mongo duplicate-key errors into domain errors (`StockItemCodeAlreadyExists`) in Infrastructure.
3. No `async void` outside true event handlers; no fire-and-forget writes; no empty `catch`.
4. Replace `Trace` with `ILogger<T>` (Serilog or the built-in provider writing to a rolling file, which keeps today's monthly log file idea). Log the exception, not only `ex.Message`.
5. Import errors are collected per row and returned as a report, never swallowed.

### 4.4 Localization
1. **(keep)** All user-facing text is a resource key; de-DE, en-US, es-PY stay the supported cultures.
2. API returns keys or localized text per `Accept-Language`; the React app uses the same keys (export resx to JSON at build time so there is one source).
3. Number-to-words stays server-side, with tests per language.

### 4.5 Testing
1. **(keep)** MSTest + Moq, `Method_Scenario_ExpectedResult` names, Arrange/Act/Assert comments (rename `// Assign` to `// Arrange`).
2. Unit tests for Domain and Application: every business rule moved out of the view models gets tests before or with the move.
3. Integration tests for the API with `WebApplicationFactory` (FastEndpoints `AppFixture`) against a real MongoDB in Testcontainers; one test class per feature.
4. Test through public behavior; no reflection into private fields.
5. Frontend: Vitest + React Testing Library for components, Playwright for a few end-to-end flows (sale, import).
6. CI runs build, format check, unit, integration and frontend tests on every PR (Linux runner is fine once WPF is gone); the installer stays on the tag-triggered workflow.

### 4.6 Data import (ERP groundwork)
1. Import is a pipeline: read (Excel/CSV) -> map columns (keep today's header-to-property mapping idea, saved as reusable profiles) -> normalize/clean -> validate -> preview with per-row errors and duplicates -> commit.
2. Every imported record keeps its source (file, sheet, row) for traceability.
3. Duplicate detection and cleaning rules are pure functions with unit tests on real-world dirty samples.

### 4.7 Repository hygiene
1. No built binaries (`.msi`) in git; publish them as release assets only.
2. No customer-specific names or connection strings in code.
3. Conventional, descriptive commit messages (history today is short labels like "Bugfix Search"); one feature or fix per PR.
