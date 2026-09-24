import { useState, type FormEvent } from "react";
import { api, type ApiFailure, type Customer, type NewCustomer } from "../../api";
import { FailureMessage } from "../../FailureMessage";
import { useI18n, type TextKey } from "../../i18n";

const fields: (keyof NewCustomer & TextKey)[] = ["name", "lastname", "phoneNumber", "email", "address", "identificationNumber"];
const empty: NewCustomer = { name: "" };

export function CreateCustomerForm({ onCreated }: { onCreated: (customer: Customer) => void })
{
	const { t } = useI18n();
	const [customer, setCustomer] = useState<NewCustomer>(empty);
	const [failure, setFailure] = useState<ApiFailure>();
	const [busy, setBusy] = useState(false);

	async function onSubmit(event: FormEvent)
	{
		event.preventDefault();
		setBusy(true);
		const result = await api.createCustomer(customer);
		setBusy(false);
		if (!result.ok) return setFailure(result.failure);

		setFailure(undefined);
		setCustomer(empty);
		onCreated(result.value);
	}

	return (
		<form onSubmit={onSubmit} className="panel inline-form">
			{fields.map(field => (
				<label key={field}>
					{t(field)}
					<input value={customer[field] ?? ""} required={field === "name"} onChange={event => setCustomer({ ...customer, [field]: event.target.value })} />
				</label>
			))}
			<button type="submit" disabled={busy}>{t("createCustomer")}</button>
			<FailureMessage failure={failure} />
		</form>
	);
}
