import { useEffect, useState, type FormEvent } from "react";
import { api, type ApiFailure, type Supplier } from "../../api";
import { FailureMessage } from "../../FailureMessage";
import { useI18n, type TextKey } from "../../i18n";

const fields: (keyof Supplier & TextKey)[] = ["name", "contactName", "country", "currency", "leadTimeDays", "miscellaneous"];
const numberFields = new Set<string>(["leadTimeDays"]);
const empty: Supplier = { id: "", name: "", contactName: "", country: "", currency: "", leadTimeDays: 0, miscellaneous: "" };

export function SupplierForm({ editing, onSaved, onCancel }: { editing?: Supplier; onSaved: (supplier: Supplier) => void; onCancel: () => void })
{
	const { t } = useI18n();
	const [supplier, setSupplier] = useState<Supplier>(editing ?? empty);
	const [failure, setFailure] = useState<ApiFailure>();
	const [busy, setBusy] = useState(false);

	useEffect(() => setSupplier(editing ?? empty), [editing]);

	async function onSubmit(event: FormEvent)
	{
		event.preventDefault();
		setBusy(true);
		const result = editing ? await api.updateSupplier(supplier) : await api.createSupplier(supplier);
		setBusy(false);
		if (!result.ok) return setFailure(result.failure);

		setFailure(undefined);
		if (!editing) setSupplier(empty);
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
							value={supplier[field]}
							required={field === "name"}
							onChange={event => setSupplier({ ...supplier, [field]: numberFields.has(field) ? Number(event.target.value) : event.target.value })}
						/>
					</label>
				))}
			</div>
			<FailureMessage failure={failure} duplicate="supplierAlreadyExists" />
			<div className="form-actions">
				<button type="submit" disabled={busy}>{t(editing ? "save" : "createSupplier")}</button>
				{editing && <button type="button" onClick={onCancel}>{t("cancel")}</button>}
			</div>
		</form>
	);
}
