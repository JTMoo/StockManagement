import { useState } from "react";
import { useI18n } from "./i18n";

/** In-page confirm step for a destructive action (Button spec: Danger only inside its own confirm; same model as InvoiceView's cancel-invoice flow). */
export function ConfirmAction({ label, message, onConfirm, triggerClassName = "quiet" }: { label: string; message: string; onConfirm: () => void; triggerClassName?: string })
{
	const { t } = useI18n();
	const [confirming, setConfirming] = useState(false);

	if (!confirming) return <button type="button" className={triggerClassName} onClick={() => setConfirming(true)}>{label}</button>;

	return (
		<span className="confirm-action">
			<span className="small">{message}</span>
			<button type="button" className="danger" onClick={() => { setConfirming(false); onConfirm(); }}>{t("confirm")}</button>
			<button type="button" className="quiet" onClick={() => setConfirming(false)}>{t("cancel")}</button>
		</span>
	);
}
