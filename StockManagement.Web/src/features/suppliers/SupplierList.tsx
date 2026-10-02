import { useState } from "react";
import { api, type ApiFailure, type Supplier } from "../../api";
import { FailureMessage } from "../../FailureMessage";
import { Page } from "../../Page";
import { useI18n } from "../../i18n";
import { useLoad } from "../../useLoad";
import { useToast } from "../../Toast";
import { SupplierForm } from "./SupplierForm";

export function SupplierList()
{
	const { t, formatNumber } = useI18n();
	const { show: showToast } = useToast();
	const { data: suppliers = [], setData, failure } = useLoad(api.listSuppliers);
	const [editing, setEditing] = useState<Supplier>();
	const [deleteFailure, setDeleteFailure] = useState<ApiFailure>();

	function onSaved(supplier: Supplier)
	{
		setData(editing ? suppliers.map(existing => existing.id === supplier.id ? supplier : existing) : [...suppliers, supplier]);
		setEditing(undefined);
		showToast(t("savedToast").replace("{0}", t("supplier")));
	}

	async function onDelete(supplier: Supplier)
	{
		if (!window.confirm(t("supplierDeletionPrompt").replace("{0}", supplier.name))) return;
		const result = await api.deleteSupplier(supplier);
		if (!result.ok) return setDeleteFailure(result.failure);

		setDeleteFailure(undefined);
		setData(suppliers.filter(existing => existing.id !== supplier.id));
		if (editing?.id === supplier.id) setEditing(undefined);
	}

	return (
		<Page title={t("suppliers")}>
			<SupplierForm editing={editing} onSaved={onSaved} onCancel={() => setEditing(undefined)} />
			<FailureMessage failure={failure ?? deleteFailure} />
			<table>
				<thead>
					<tr><th>{t("name")}</th><th>{t("contactName")}</th><th>{t("country")}</th><th>{t("currency")}</th><th className="number">{t("leadTimeDays")}</th><th></th></tr>
				</thead>
				<tbody>
					{suppliers.map(supplier => (
						<tr key={supplier.id}>
							<td>{supplier.name}</td><td>{supplier.contactName}</td><td>{supplier.country}</td><td>{supplier.currency}</td>
							<td className="number">{formatNumber(supplier.leadTimeDays)}</td>
							<td className="row-actions">
								<button type="button" className="quiet" onClick={() => setEditing(supplier)}>{t("edit")}</button>
								<button type="button" className="quiet" onClick={() => onDelete(supplier)}>{t("deleteSupplier")}</button>
							</td>
						</tr>
					))}
				</tbody>
			</table>
		</Page>
	);
}
