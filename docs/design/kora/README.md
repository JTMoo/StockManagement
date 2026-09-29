# Kora

`StockManagement.Web`'s design system, decided in [ADR-0025](../../adr/0025-kora-design-system.md) (look, navigation) and [ADR-0026](../../adr/0026-cross-domain-search.md) (search). This directory is the source of truth; a live, editable version with contrast-checked color previews lives at <https://claude.ai/artifact/BUpwjaRezNtGkkiTSc6DTj> (share it from its page's Share menu before pointing anyone else at it) - if the two ever disagree, this directory wins, and the artifact should be brought back in line with it.

`tokens.css` is compiled by hand from `tokens.json` - edit the JSON, then re-derive the CSS custom properties and `.display`/`.heading`/etc. classes the same way (see the class list at the bottom of `tokens.css`). Every `components/<Name>/preview.html` is a standalone document - open it directly in a browser, no build step, no server.

## Voice

Write in the interface's own configured language (es-PY, en-US or de-DE) - never invent Guaraní copy for the UI; "Kora" (Guaraní for a livestock corral - a place that holds and orders stock) names only this design system, not a language choice. Sentences are short and direct, second person, imperative for anything that acts: "Guardar", "Agregar producto", "Anular factura" - never a question, a hedge ("¿Está seguro?" only where undoing costs data, see Confirmation below) or an exclamation. No emoji, no marketing adjectives ("¡Rápido y fácil!"). An error names what's wrong and the exact fix in one line: "El RUC ya existe en Clientes" - not "Ocurrió un error". Numbers are never rounded silently; a currency amount always shows its two-letter code (`Gs` for PYG) once per column, not per row. Where a second currency applies (a purchase price with its exchange rate, per ADR-0020), the converted amount repeats directly beneath the primary one in `small`/`ink-faint` - same column, same alignment, never a separate column or an inline parenthetical.

## Logo

The mark is one corral (a `radius-lg` shape) holding three bins, cut from it as negative space - one wide bin, two small, the same "tiles cut from a block" language as the cover. It's a single-ink cutout, stored as `assets/Logos/mark.svg` (ink `brand`) and `assets/Logos/mark-reversed.svg` (ink white, for a dark surface or sitting on a `brand`/`accent`/`ink` fill) - never recolored, rotated or skewed, never given a shadow. Clear space on every side at least `space-4` at UI scale; don't run it under 24px. `assets/Logos/lockup.svg` flattens the mark with the wordmark for contexts outside the product (an org avatar, an external README) - inside the product, set "Kora" as live `display` text beside the mark image instead, so it follows the theme.

## Color

Two themes, `light` (default - a bright counter or back office in daylight) and `dark` (a warehouse or a night shift). Every text/ground pairing named in a token's `usage` holds at least 4.5:1 in both themes (3:1 for `ink-faint`, which is placeholder and disabled text only and never the sole carrier of required content). `border-strong` is a structural divider - it carries no state and is exempt from the contrast floor; anything that DOES carry meaning (an error outline, the focus ring, a below-minimum row) uses `danger` or `focus-ring`, never `border-strong` alone.

`brand` (a tierra colorada terracotta) is the one fill for the primary action per screen - the button that finishes the task ("Cobrar", "Guardar", "Confirmar importación"). `accent` (a mate green) is for the one secondary action beside it ("Guardar y nuevo", "Exportar"); everything else is a quiet text button. Never both a `brand` and an `accent` fill in the same view for unrelated actions - pick the one thing this screen wants finished.

`success`, `warning`, `danger` and `info` each pair a saturated text/icon color with a `-subtle` tinted background for the same status, always shown with a word or icon, never color alone (an out-of-stock row reads "Agotado" in `danger` next to a filled dot, not just a red row). `success` is pulled toward blue-green and `danger` toward red-orange specifically so they read apart under deuteranopia/protanopia even side by side; a real value that must stay exact (there are none in this from-scratch palette) would be flagged here instead of quietly re-tinted.

## Type

One family, Archivo (Google Fonts: `<link rel="stylesheet" href="https://fonts.googleapis.com/css2?family=Archivo:wght@400;600;700&display=swap">`, weights 400/600/700) - a grotesque with true tabular figures, chosen so the interface's own name doesn't borrow the look of a dozen other dashboards. Seven styles cover every screen: `display` for the one page title, `heading` for a panel or section title, `body-strong` for table headers and emphasized cells, `body` for everything else, `numeric` for every money/quantity/RUC column (always with `font-variant-numeric: tabular-nums`, right-aligned, so digits stack), `small` for labels and helper text, `control` for button and nav labels (uppercase, +0.02em tracking only for `control`, never for `body`). Don't add a second family for "the numbers" - `numeric` is `body` with tabular figures switched on, not a different font.

## Spacing & radius

A 4px unit, seven steps (`space-1` 4px to `space-7` 48px). `space-3` sits between a label and its field; `space-4` between fields in a form grid and inside a table cell; `space-5` between a page's header and its body; `space-6` between stacked panels and, on narrow screens, the page's own side padding; `space-7` is the page's side padding at normal width and the sidebar's width unit (`4 × space-7` = 192px expanded). Three radii: `radius-sm` for badges and checkboxes, `radius-md` for buttons, inputs and the sidebar's items, `radius-lg` for panels, dialogs and the command palette - nothing sharper than `radius-sm`, nothing rounder than `radius-lg`.

## Layout & navigation

Every screen is a `Page`: title in `display` on the left, one toolbar of at most three actions on the right, content below - the same shape whether it's a list, a form or the day's sales. The primary action for that screen is always in that toolbar, always in the same corner, never buried in a menu.

Navigation is a single, always-visible left `Sidebar` - never the old right-hand menu, and never a second, nested level of pages: every screen is one click from the sidebar, full stop. It opens with the single most-repeated action of the day ("Venta rápida") above a divider, then four or five named sections grouped by how often staff touch them (daily: Ventas, Facturas, Stock; occasional: Clientes, Proveedores; rare, bottom-anchored: Importar, Usuarios, Config) - never more than nine top-level items, never a "More" overflow.

The `CommandPalette` (`Ctrl`/`Cmd`+`K`, and a visible search field pinned atop the sidebar for a mouse-only user) is search, full stop - the nine-or-fewer screens under "Pantallas", then one grouped, ranked result list per domain (Facturas, Clientes, Productos, Proveedores) searched across every identity field at once (name, code, RUC, invoice number, phone), typo- and accent-tolerant. Typing "gs 4521" jumps straight to invoice #4521; "cemento" lists matching stock items without leaving whatever screen you're on. Results default to active records only (no `Anulada` invoices, no filters excluding sold-out items) with one filter chip to widen to everything - the same "daily use first" framing as the sidebar. Each domain group shows at most five matches ranked by relevance then recency, with a "Ver los N resultados en Facturas →" line that opens that domain's own list, already filtered - the overlay is a fast preview, not a second copy of every list's pagination. It never introduces an action the sidebar or a page toolbar doesn't already have; it is a faster door to the same rooms, not a second app.

Inside a page: forms are one `form-grid`, labels in `small` stacked above their field, actions bottom-left. Creating or editing a record opens a `Dialog` from the page's own "+ Nuevo" toolbar button or a row's "Editar" action - never an inline form permanently taking up the top of a list page fighting the search box for attention. A form that's filled often twice in a row ("agregar producto" while stocktaking) always offers "Guardar y nuevo" beside "Guardar" inside that same dialog, so it reopens empty instead of a round trip through the list. Lists are a `DataTable` with row actions inline (edit, duplicate, anular) - never a click-through to a separate detail page to do a one-field edit. A destructive or hard-to-undo action (anular factura, eliminar cliente) is the one place a confirm step belongs; a save, a filter or an "agregar otro" never gets one.

## Iconography

lucide-react, 1.5px stroke throughout: 20px in the sidebar and page toolbars, 16px inline with `body`/`small` text. An icon that conveys state (a status dot, an alert triangle) always sits beside its word - never stands alone as the only signal.

## States & motion

Hover darkens a fill or tints a quiet surface one step (`surface-raised` → `surface-sunken`); active/pressed goes one step further. Disabled drops to 45% opacity and stops accepting focus. `focus-ring` (2px, 2px offset) shows on every interactive element on keyboard focus and is never suppressed, only restyled together as a set. No shadows anywhere - depth comes only from `border` and the surface tiers (`surface-page` / `surface-raised` / `surface-sunken`), never a drop shadow. No motion beyond an instant (~100ms) hover/focus transition; nothing animates to explain itself.

A screen that lacks permission for an action shows that action disabled (45% opacity, same as above), never hidden - a missing button reads as a bug or a broken layout; a disabled one reads as "not for you." Its `small`/`ink-faint` tooltip or adjacent line says why ("Solo Administrador"), never just refuses silently.

## Empty & loading

A `DataTable` with no rows (a new business, or a search with no matches) replaces the table with one `body`/`ink-muted` line stating exactly that ("Todavía no hay clientes" / "Sin resultados para \"...\""), and, only for the true-empty case (not a search miss), the page's own "+ Nuevo" action repeated inline - no illustration, no icon, the same action the toolbar already offers. Loading state is one `small`/`ink-faint` "Cargando..." where the table would be - no skeleton rows, no spinner; the wait is short enough at this scale that a skeleton would only add motion for nothing.

## Components

Each has guidelines (`README.md`) and a standalone live preview (`preview.html`) - open the preview directly in a browser.

| Component | Guidelines | Preview |
|---|---|---|
| Button | [README](components/Button/README.md) | [preview](components/Button/preview.html) |
| Field | [README](components/Field/README.md) | [preview](components/Field/preview.html) |
| StatusBadge | [README](components/StatusBadge/README.md) | [preview](components/StatusBadge/preview.html) |
| Sidebar | [README](components/Sidebar/README.md) | [preview](components/Sidebar/preview.html) |
| Page | [README](components/Page/README.md) | [preview](components/Page/preview.html) |
| Panel | [README](components/Panel/README.md) | [preview](components/Panel/preview.html) |
| Segmented | [README](components/Segmented/README.md) | [preview](components/Segmented/preview.html) |
| DataTable | [README](components/DataTable/README.md) | [preview](components/DataTable/preview.html) |
| Toast | [README](components/Toast/README.md) | [preview](components/Toast/preview.html) |
| Dialog | [README](components/Dialog/README.md) | [preview](components/Dialog/preview.html) |
| CommandPalette | [README](components/CommandPalette/README.md) | [preview](components/CommandPalette/preview.html) |
| Receipt | [README](components/Receipt/README.md) | [preview](components/Receipt/preview.html) |
| Cover (brand mark) | - | [preview](components/Cover/preview.html) |

Logo files (`mark.svg`, `mark-reversed.svg`, `lockup.svg`) and their usage rules: [logo/README.md](logo/README.md).
