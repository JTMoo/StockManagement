import { useEffect, useState, type FormEvent } from "react";
import { api, type ApiFailure, type Invoice } from "../../api";
import { FailureMessage } from "../../FailureMessage";
import { useI18n } from "../../i18n";

export function InvoiceView({ invoice: initial }: { invoice?: Invoice })
{
	const { t, formatNumber, formatDate } = useI18n();
	const [number, setNumber] = useState(initial ? String(initial.number) : "");
	const [invoice, setInvoice] = useState(initial);
	const [failure, setFailure] = useState<ApiFailure>();

	useEffect(() => setInvoice(initial), [initial]);

	async function onSubmit(event: FormEvent)
	{
		event.preventDefault();
		const result = await api.getInvoice(Number(number));
		setInvoice(result.ok ? result.value : undefined);
		setFailure(result.ok ? undefined : result.failure);
	}

	return (
		<section>
			<h2>{t("invoices")}</h2>
			<form onSubmit={onSubmit} className="inline-form">
				<label>
					{t("invoiceId")}
					<input type="number" min={1} required value={number} onChange={event => setNumber(event.target.value)} />
				</label>
				<button type="submit">{t("show")}</button>
			</form>
			<FailureMessage failure={failure} notFound="invoiceNotFound" />
			{invoice && (
				<article className="paper" aria-label={`${t("invoice")} ${invoice.number}`}>
					<header>
						<h3>{t("invoice")} {invoice.number}</h3>
						<span>{t(invoice.saleCondition === "Cash" ? "cash" : "credit")}</span>
					</header>
					<dl>
						<dt>{t("customerId")}</dt><dd>{invoice.customerId}</dd>
						<dt>{t("creationDate")}</dt><dd>{formatDate(invoice.date)}</dd>
						<dt>{t("expirationDate")}</dt><dd>{formatDate(invoice.expirationDate)}</dd>
					</dl>
					<table>
						<thead>
							<tr><th>{t("code")}</th><th>{t("name")}</th><th className="number">{t("quantity")}</th><th className="number">{t("price")}</th></tr>
						</thead>
						<tbody>
							{invoice.lines.map(line => (
								<tr key={line.code}>
									<td>{line.code}</td><td>{line.name}</td><td className="number">{formatNumber(line.amount)}</td><td className="number">{formatNumber(line.unitPrice)}</td>
								</tr>
							))}
						</tbody>
						<tfoot>
							<tr><th colSpan={3}>{t("tax")}</th><td className="number">{formatNumber(invoice.tax)}</td></tr>
							<tr className="total"><th colSpan={3}>{t("total")}</th><td className="number" data-testid="invoice-total">{formatNumber(invoice.total)}</td></tr>
						</tfoot>
					</table>
				</article>
			)}
		</section>
	);
}
