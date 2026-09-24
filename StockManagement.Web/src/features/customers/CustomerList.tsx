import { api, type Customer } from "../../api";
import { FailureMessage } from "../../FailureMessage";
import { Page } from "../../Page";
import { useI18n } from "../../i18n";
import { useLoad } from "../../useLoad";
import { CreateCustomerForm } from "./CreateCustomerForm";

export function CustomerList()
{
	const { t } = useI18n();
	const { data: customers = [], setData, failure } = useLoad(api.listCustomers);

	const onCreated = (customer: Customer) => setData([...customers, customer]);

	return (
		<Page title={t("clients")}>
			<CreateCustomerForm onCreated={onCreated} />
			<FailureMessage failure={failure} />
			<table>
				<thead>
					<tr><th>{t("customerId")}</th><th>{t("name")}</th><th>{t("lastname")}</th><th>{t("phoneNumber")}</th><th>{t("email")}</th><th>{t("address")}</th></tr>
				</thead>
				<tbody>
					{customers.map(customer => (
						<tr key={customer.customerId}>
							<td>{customer.customerId}</td><td>{customer.name}</td><td>{customer.lastname}</td>
							<td>{customer.phoneNumber}</td><td>{customer.email}</td><td>{customer.address}</td>
						</tr>
					))}
				</tbody>
			</table>
		</Page>
	);
}
