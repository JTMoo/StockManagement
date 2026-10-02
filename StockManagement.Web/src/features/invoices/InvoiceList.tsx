import { useEffect, useState } from "react";
import { api, type ApiFailure, type Invoice } from "../../api";
import { FailureMessage } from "../../FailureMessage";
import { invoiceStatusBadge } from "./invoiceStatus";
import { useI18n } from "../../i18n";

const pageSize = 20;

export function InvoiceList({ onSelect, refreshToken }: { onSelect: (invoice: Invoice) => void; refreshToken?: number })
{
	const { t, formatNumber, formatDate } = useI18n();
	const [customerId, setCustomerId] = useState("");
	const [from, setFrom] = useState("");
	const [to, setTo] = useState("");
	// Cursor history (ADR-0029): cursorHistory[pageIndex] is the cursor used to fetch the current page; "previous" pops it
	const [cursorHistory, setCursorHistory] = useState<(string | undefined)[]>([undefined]);
	const [pageIndex, setPageIndex] = useState(0);
	const [items, setItems] = useState<Invoice[]>([]);
	const [nextCursor, setNextCursor] = useState<string | null>(null);
	const [failure, setFailure] = useState<ApiFailure>();

	useEffect(() =>
	{
		const controller = new AbortController();
		const filter = { customerId: customerId ? Number(customerId) : undefined, from: from || undefined, to: to || undefined, cursor: cursorHistory[pageIndex], pageSize };
		api.listInvoices(filter, controller.signal).then(result =>
		{
			if (controller.signal.aborted) return;
			setFailure(result.ok ? undefined : result.failure);
			setItems(result.ok ? result.value.items : []);
			setNextCursor(result.ok ? result.value.nextCursor : null);
		});
		return () => controller.abort();
	}, [customerId, from, to, cursorHistory, pageIndex, refreshToken]);

	function resetToFirstPage()
	{
		setCursorHistory([undefined]);
		setPageIndex(0);
	}

	function goToNextPage()
	{
		if (!nextCursor) return;
		setCursorHistory([...cursorHistory.slice(0, pageIndex + 1), nextCursor]);
		setPageIndex(pageIndex + 1);
	}

	return (
		<>
			<form className="panel" onSubmit={event => event.preventDefault()}>
				<div className="form-grid">
					<label>
						{t("customerId")}
						<input type="number" min={1} value={customerId} onChange={event => { setCustomerId(event.target.value); resetToFirstPage(); }} />
					</label>
					<label>
						{t("from")}
						<input type="date" value={from} onChange={event => { setFrom(event.target.value); resetToFirstPage(); }} />
					</label>
					<label>
						{t("to")}
						<input type="date" value={to} onChange={event => { setTo(event.target.value); resetToFirstPage(); }} />
					</label>
				</div>
			</form>
			<FailureMessage failure={failure} />
			<table>
				<thead>
					<tr><th>{t("invoiceId")}</th><th>{t("creationDate")}</th><th>{t("customerName")}</th><th className="number">{t("total")}</th><th>{t("saleCondition")}</th><th>{t("status")}</th></tr>
				</thead>
				<tbody>
					{items.map(item => (
						<tr key={item.number}>
							<td><button type="button" className="quiet" onClick={() => onSelect(item)}>{item.number}</button></td>
							<td>{formatDate(item.date)}</td><td>{item.customerName}</td>
							<td className="number">{formatNumber(item.total)}</td><td>{t(item.saleCondition === "Cash" ? "cash" : "credit")}</td>
							<td>{invoiceStatusBadge(item, t)}</td>
						</tr>
					))}
				</tbody>
			</table>
			<div className="form-actions">
				<button type="button" disabled={pageIndex <= 0} onClick={() => setPageIndex(pageIndex - 1)}>{t("previous")}</button>
				<button type="button" disabled={!nextCursor} onClick={goToNextPage}>{t("next")}</button>
			</div>
		</>
	);
}
