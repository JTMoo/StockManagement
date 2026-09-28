The one action that finishes a task, styled by how much weight it should carry - never by how big the click target feels.

- **Primary** (`brand` fill, `on-fill` text): exactly one per screen - "Cobrar", "Guardar", "Confirmar importación". If a page toolbar needs a second primary-looking button, one of them is wrong.
- **Secondary** (`accent` text + 1px `accent` border, transparent fill): the one action beside the primary - "Guardar y nuevo", "Exportar".
- **Quiet** (`ink-muted` text, no fill or border, `surface-sunken` on hover): everything else - "Cancelar", table row actions.
- **Danger** (`danger` fill, `on-fill` text): only for a destructive, hard-to-undo action, and only inside its own confirm step - "Eliminar", "Anular factura".

All four: `radius-md`, `control` type style, `space-3`/`space-4` horizontal padding, 40px min height (a comfortable mouse target, not a touch one - this system targets desktop/laptop first). Disabled drops to 45% opacity and stops accepting focus, on every variant. Never disable a button to explain why it can't be pressed yet - show the reason in a `small` line beside it instead.
