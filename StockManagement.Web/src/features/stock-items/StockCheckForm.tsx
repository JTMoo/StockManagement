import { useState, type FormEvent } from "react";
import { api, type ApiFailure, type StockItem } from "../../api";
import { FailureMessage } from "../../FailureMessage";
import { useI18n } from "../../i18n";

export function StockCheckForm({ stockItem, onChecked, onCancel }: { stockItem: StockItem; onChecked: (stockItem: StockItem) => void; onCancel: () => void })
{
	const { t } = useI18n();
	const [amount, setAmount] = useState(1);
	const [reason, setReason] = useState("");
	const [failure, setFailure] = useState<ApiFailure>();
	const [busy, setBusy] = useState(false);

	async function check(direction: "in" | "out")
	{
		setBusy(true);
		const result = direction === "in" ? await api.checkInStockItem(stockItem.id, amount, reason) : await api.checkOutStockItem(stockItem.id, amount, reason);
		setBusy(false);
		if (!result.ok) return setFailure(result.failure);

		setFailure(undefined);
		onChecked(result.value);
	}

	function onSubmit(event: FormEvent)
	{
		event.preventDefault();
		check("in");
	}

	return (
		<form onSubmit={onSubmit} className="panel">
			<div className="form-grid">
				<label>
					{t("amount")}
					<input type="number" min={1} required value={amount} onChange={event => setAmount(Number(event.target.value))} />
				</label>
				<label>
					{t("reason")}
					<input type="text" required value={reason} onChange={event => setReason(event.target.value)} />
				</label>
			</div>
			<FailureMessage failure={failure} />
			<div className="form-actions">
				<button type="submit" disabled={busy}>{t("checkIn")}</button>
				<button type="button" disabled={busy} onClick={() => check("out")}>{t("checkOut")}</button>
				<button type="button" onClick={onCancel}>{t("cancel")}</button>
			</div>
		</form>
	);
}
