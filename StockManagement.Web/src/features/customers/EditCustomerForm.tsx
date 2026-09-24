import { useState, type FormEvent } from "react";
import { api, type ApiFailure, type Customer } from "../../api";
import { FailureMessage } from "../../FailureMessage";
import { useI18n, type TextKey } from "../../i18n";

const fields: (keyof Customer & TextKey)[] = ["name", "lastname", "phoneNumber", "email", "address", "identificationNumber"];

export function EditCustomerForm({ customer, onSaved, onCancel }: { customer: Customer; onSaved: (customer: Customer) => void; onCancel: () => void })
{
	const { t } = useI18n();
	const [draft, setDraft] = useState<Customer>(customer);
	const [failure, setFailure] = useState<ApiFailure>();
	const [busy, setBusy] = useState(false);

	async function onSubmit(event: FormEvent)
	{
		event.preventDefault();
		setBusy(true);
		const result = await api.updateCustomer(draft);
		setBusy(false);
		if (!result.ok) return setFailure(result.failure);

		setFailure(undefined);
		onSaved(result.value);
	}

	return (
		<form onSubmit={onSubmit} className="panel">
			<div className="form-grid">
				{fields.map(field => (
					<label key={field}>
						{t(field)}
						<input value={draft[field] ?? ""} required={field === "name"} onChange={event => setDraft({ ...draft, [field]: event.target.value })} />
					</label>
				))}
			</div>
			<FailureMessage failure={failure} />
			<div className="form-actions">
				<button type="submit" disabled={busy}>{t("saveCustomer")}</button>
				<button type="button" onClick={onCancel} disabled={busy}>{t("cancel")}</button>
			</div>
		</form>
	);
}
