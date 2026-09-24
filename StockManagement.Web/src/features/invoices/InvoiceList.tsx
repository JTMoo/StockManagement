import { useEffect, useState } from "react";
import { api, type ApiFailure, type Invoice } from "../../api";
import { FailureMessage } from "../../FailureMessage";
import { useI18n } from "../../i18n";

const pageSize = 20;

export function InvoiceList({ onSelect }: { onSelect: (invoice: Invoice) => void })
{
	const { t, formatNumber, formatDate } = useI18n();
	const [customerId, setCustomerId] = useState("");
	const [from, setFrom] = useState("");
	const [to, setTo] = useState("");
	const [page, setPage] = useState(1);
	const [items, setItems] = useState<Invoice[]>([]);
	const [totalCount, setTotalCount] = useState(0);
	const [failure, setFailure] = useState<ApiFailure>();

	useEffect(() =>
	{
		const controller = new AbortController();
		const filter = { customerId: customerId ? Number(customerId) : undefined, from: from || undefined, to: to || undefined, page, pageSize };
		api.listInvoices(filter, controller.signal).then(result =>
		{
			if (controller.signal.aborted) return;
			setFailure(result.ok ? undefined : result.failure);
			setItems(result.ok ? result.value.items : []);
			setTotalCount(result.ok ? result.value.totalCount : 0);
		});
		return () => controller.abort();
	}, [customerId, from, to, page]);

	const lastPage = Math.max(1, Math.ceil(totalCount / pageSize));

	return (
		<>
			<form className="panel" onSubmit={event => event.preventDefault()}>
				<div className="form-grid">
					<label>
						{t("customerId")}
						<input type="number" min={1} value={customerId} onChange={event => { setCustomerId(event.target.value); setPage(1); }} />
					</label>
					<label>
						{t("from")}
						<input type="date" value={from} onChange={event => { setFrom(event.target.value); setPage(1); }} />
					</label>
					<label>
						{t("to")}
						<input type="date" value={to} onChange={event => { setTo(event.target.value); setPage(1); }} />
					</label>
				</div>
			</form>
			<FailureMessage failure={failure} />
			<table>
				<thead>
					<tr><th>{t("invoiceId")}</th><th>{t("creationDate")}</th><th>{t("customerName")}</th><th className="number">{t("total")}</th><th>{t("saleCondition")}</th></tr>
				</thead>
				<tbody>
					{items.map(item => (
						<tr key={item.number}>
							<td><button type="button" className="quiet" onClick={() => onSelect(item)}>{item.number}</button></td>
							<td>{formatDate(item.date)}</td><td>{item.customerName}</td>
							<td className="number">{formatNumber(item.total)}</td><td>{t(item.saleCondition === "Cash" ? "cash" : "credit")}</td>
						</tr>
					))}
				</tbody>
			</table>
			<div className="form-actions">
				<button type="button" disabled={page <= 1} onClick={() => setPage(page - 1)}>{t("previous")}</button>
				<button type="button" disabled={page >= lastPage} onClick={() => setPage(page + 1)}>{t("next")}</button>
			</div>
		</>
	);
}
