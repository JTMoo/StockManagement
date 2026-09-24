import { useEffect, useState, type FormEvent } from "react";
import { api, type ApiFailure, type StockItem } from "../../api";
import { FailureMessage } from "../../FailureMessage";
import { useI18n, type TextKey } from "../../i18n";

const fields: (keyof StockItem & TextKey)[] = ["code", "name", "description", "location", "amount", "price", "manufacturer"];
const numberFields = new Set<string>(["amount", "price"]);
const empty: StockItem = { id: "", code: "", name: "", description: "", location: "", amount: 0, price: 0, manufacturer: "" };

export function StockItemForm({ editing, onSaved, onCancel }: { editing?: StockItem; onSaved: (stockItem: StockItem) => void; onCancel: () => void })
{
	const { t } = useI18n();
	const [stockItem, setStockItem] = useState<StockItem>(editing ?? empty);
	const [failure, setFailure] = useState<ApiFailure>();
	const [busy, setBusy] = useState(false);

	useEffect(() => setStockItem(editing ?? empty), [editing]);

	async function onSubmit(event: FormEvent)
	{
		event.preventDefault();
		setBusy(true);
		const result = editing ? await api.updateStockItem(stockItem) : await api.createStockItem(stockItem);
		setBusy(false);
		if (!result.ok) return setFailure(result.failure);

		setFailure(undefined);
		if (!editing) setStockItem(empty);
		onSaved(result.value);
	}

	return (
		<form onSubmit={onSubmit} className="panel">
			<div className="form-grid">
				{fields.map(field => (
					<label key={field}>
						{t(field)}
						<input
							type={numberFields.has(field) ? "number" : "text"}
							value={stockItem[field]}
							required={field === "code" || field === "name"}
							disabled={field === "code" && !!editing}
							onChange={event => setStockItem({ ...stockItem, [field]: numberFields.has(field) ? Number(event.target.value) : event.target.value })}
						/>
					</label>
				))}
			</div>
			<FailureMessage failure={failure} />
			<div className="form-actions">
				<button type="submit" disabled={busy}>{t(editing ? "save" : "createItem")}</button>
				{editing && <button type="button" onClick={onCancel}>{t("cancel")}</button>}
			</div>
		</form>
	);
}
