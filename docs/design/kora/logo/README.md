The Kora mark: one corral holding three bins (one wide, two small), cut from it as negative space - the same shape and the same "tiles cut from a block" pattern language as the cover.

- `mark.svg` - ink `brand` (#C1502E). Default; use on `surface-page`, `surface-raised` or any light/neutral ground.
- `mark-reversed.svg` - ink white (#FFFFFF). Use on a dark theme surface, or sitting on a `brand`/`accent`/`ink` fill.
- `lockup.svg` - the mark plus the wordmark, flattened, for a context that can't run the real page (a GitHub org avatar, an external README, a favicon source). Ink `brand` on the mark, `ink` on the wordmark, generic bold sans (no font is embedded). Inside the product, don't use this file - place `mark.svg`/`mark-reversed.svg` next to live "Kora" text in `display` so it follows the theme and stays selectable text.

Each is a single-ink cutout mark (transparent holes, one solid fill) - safe on a photo or a pattern behind it, but never recolor, rotate, skew or add a shadow to it. Minimum size 24px (the mark alone) or 96px wide (the lockup) before the cutouts close up. Clear space on every side at least `space-4` (16px) at UI scale, scaling with the mark's own size elsewhere.
