# ADR-0025: Kora design system, full replacement of the WPF look

- Status: Proposed
- Date: 2026-09-28

## Context

- [ADR-0006](0006-react-web-app-shape.md) set the current web look: "follows WPF - brandboard colors, Poppins, right-side icon menu, plain CSS tokens in `src/index.css`... no UI kit until a screen needs one."
- Goal (owner): a real design system for `StockManagement.Web`, optimized for small/medium Paraguayan businesses - easy navigation, fewest clicks per flow.
- Grilled directly with the owner (`/grill-with-docs` is a local skill, not available in this remote session - CLAUDE.md's fallback is to ask the owner directly):
  - Full replacement of ADR-0006's look clause, not an evolution of it.
  - Primary device: desktop/laptop, mouse and keyboard (matches ADR-0003/0013's desktop-first direction).
  - Component approach: a headless UI kit (Radix/Ark primitives) skinned with the new tokens, not hand-rolled-only and not a full styled library.
  - Brand: blank slate, no existing brandboard to anchor to.

## Options

- Evolve ADR-0006's WPF-derived look (keep brandboard colors/Poppins/right menu, formalize spacing and components around them)
- Full replacement: new palette, type, spacing/radius scale, and IA, written up as a design system
- Adopt an existing open-source design system as-is (Mantine, Chakra, shadcn/ui defaults) with no product-specific identity

## Decision

- New design system, "Kora" (Guaraní for a livestock corral - holding and ordering stock), authored as a Design-System-type artifact: two themes (`light` default, `dark`), a 24-token color set (brand/accent + neutrals + 4 color-blind-safe semantic statuses), one type family (Archivo, Google Fonts) in 7 styles, a 7-step 4px spacing scale, 3 radii, no shadows.
- Navigation moves from the WPF-derived right-side menu to a persistent **left** sidebar (max 9 top-level items, grouped by frequency of use) plus a **command palette** (`Ctrl`/`Cmd`+K) as the fast path to any screen or record by name/number - the concrete answer to "fewest clicks."
- Ten components specified with guidelines + a live preview each: `Button`, `Field`, `StatusBadge`, `Sidebar`, `Page` (title-left/toolbar-right/content-below, kept from the current app - it's a sound, skin-independent layout, not part of the WPF look), `Panel`, `Segmented`, `DataTable`, and two intentional additions the current app has none of - `Toast` (non-blocking save confirmation) and `CommandPalette`.
- Component behavior (dialogs, comboboxes, the command palette's listbox) is expected to be built on headless primitives (e.g. `@radix-ui/react-*`) skinned with Kora's tokens, not a full styled component library - this ADR records the direction; the concrete package choice is a normal implementation PR, not a separate ADR.
- Supersedes the "Look follows WPF..." paragraph of ADR-0006's Decision section; the rest of ADR-0006 (Vite/React/TS, `/api` proxy, `src/api.ts`, feature folders, texts via resx→JSON) is unaffected.

## Consequences

- `src/index.css`'s current WPF-derived tokens (`--navy`, `--slate`, `--steel`, `--paper`, right-side `.menu`) are replaced, not extended; every screen that references them needs restyling, not just new screens.
- `@fontsource/poppins` is dropped in favor of Archivo (Google Fonts, no local font files to vendor).
- A UI kit enters the dependency tree for the first time (Radix or Ark primitives) - a change from ADR-0006's "no UI kit until a screen needs one."
- The design system itself (tokens, brand book, component guidelines and previews) lives at <https://claude.ai/artifact/BUpwjaRezNtGkkiTSc6DTj> (private; ask the owner to share it before pointing anyone else at the link) - implementing it in `StockManagement.Web` (tokens.css, the sidebar, the command palette, restyled screens) is separate, future work.
