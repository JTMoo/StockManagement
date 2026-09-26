import { useState } from "react";
import { api, type Customer } from "../../api";
import { FailureMessage } from "../../FailureMessage";
import { Page } from "../../Page";
import { useI18n } from "../../i18n";
import { useLoad } from "../../useLoad";
import { CreateCustomerForm } from "./CreateCustomerForm";
import { CustomerImport } from "./CustomerImport";
import { EditCustomerForm } from "./EditCustomerForm";

export function CustomerList()
{
	const { t } = useI18n();
	const { data: customers = [], setData, failure } = useLoad(api.listCustomers);
	const [editing, setEditing] = useState<Customer>();
	const [showImport, setShowImport] = useState(false);

	const onCreated = (customer: Customer) => setData([...customers, customer]);
	const onSaved = (customer: Customer) =>
	{
		setData(customers.map(existing => existing.customerId === customer.customerId ? customer : existing));
		setEditing(undefined);
	};

	async function onImported()
	{
		const result = await api.listCustomers();
		if (result.ok) setData(result.value);
	}

	return (
		<Page title={t("clients")} toolbar={<button type="button" className="quiet" aria-pressed={showImport} onClick={() => setShowImport(!showImport)}>{t("excelImport")}</button>}>
			{editing
				? <EditCustomerForm customer={editing} onSaved={onSaved} onCancel={() => setEditing(undefined)} />
				: <CreateCustomerForm onCreated={onCreated} />}
			{showImport && <CustomerImport onImported={onImported} />}
			<FailureMessage failure={failure} />
			<table>
				<thead>
					<tr><th>{t("customerId")}</th><th>{t("name")}</th><th>{t("lastname")}</th><th>{t("phoneNumber")}</th><th>{t("email")}</th><th>{t("address")}</th><th /></tr>
				</thead>
				<tbody>
					{customers.map(customer => (
						<tr key={customer.customerId}>
							<td>{customer.customerId}</td><td>{customer.name}</td><td>{customer.lastname}</td>
							<td>{customer.phoneNumber}</td><td>{customer.email}</td><td>{customer.address}</td>
							<td><button type="button" onClick={() => setEditing(customer)}>{t("edit")}</button></td>
						</tr>
					))}
				</tbody>
			</table>
		</Page>
	);
}
