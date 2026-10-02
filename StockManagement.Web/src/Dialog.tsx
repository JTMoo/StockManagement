import { useEffect, useRef, type ReactNode } from "react";
import { useI18n } from "./i18n";

/** Centered modal over a scrim. Esc and a backdrop click both close it. */
export function Dialog({ open, onClose, title, children }: { open: boolean; onClose: () => void; title: string; children: ReactNode })
{
	const { t } = useI18n();
	const ref = useRef<HTMLDialogElement>(null);

	useEffect(() =>
	{
		// jsdom (unit tests) has no showModal/close yet - fall back to the plain "open" attribute.
		const dialog = ref.current;
		if (!dialog) return;
		if (open && !dialog.open) dialog.showModal ? dialog.showModal() : dialog.setAttribute("open", "");
		if (!open && dialog.open) dialog.close ? dialog.close() : dialog.removeAttribute("open");
	}, [open]);

	if (!open) return null;

	return (
		<dialog ref={ref} className="dialog" onClose={onClose} onCancel={onClose} onClick={event => { if (event.target === ref.current) onClose(); }}>
			<div className="dialog-head">
				<h2 className="display">{title}</h2>
				<button type="button" className="quiet" aria-label={t("close")} onClick={onClose}>&times;</button>
			</div>
			{children}
		</dialog>
	);
}
