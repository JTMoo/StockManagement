import { useState } from "react";
import type { Invoice } from "./api";
import { CustomerList } from "./features/customers/CustomerList";
import { InvoiceView } from "./features/invoices/InvoiceView";
import { SaleForm } from "./features/sales/SaleForm";
import { StockItemList } from "./features/stock-items/StockItemList";
import { cultures, useI18n, type Culture, type TextKey } from "./i18n";

type View = "stockItems" | "clients" | "newSale" | "invoices";

const views: View[] = ["stockItems", "clients", "newSale", "invoices"];
const cultureNames: Record<Culture, TextKey> = { "de-DE": "german", "en-US": "english", "es-PY": "spanish" };

export function App()
{
	const { t, culture, setCulture } = useI18n();
	const [view, setView] = useState<View>("stockItems");
	const [invoice, setInvoice] = useState<Invoice>();

	function onSold(sold: Invoice)
	{
		setInvoice(sold);
		setView("invoices");
	}

	return (
		<>
			<header>
				<nav>
					{views.map(name => <button key={name} aria-pressed={view === name} onClick={() => setView(name)}>{t(name)}</button>)}
				</nav>
				<select aria-label={t("selectLanguage")} value={culture} onChange={event => setCulture(event.target.value as Culture)}>
					{cultures.map(name => <option key={name} value={name}>{t(cultureNames[name])}</option>)}
				</select>
			</header>
			<main>
				{view === "stockItems" && <StockItemList />}
				{view === "clients" && <CustomerList />}
				{view === "newSale" && <SaleForm onSold={onSold} />}
				{view === "invoices" && <InvoiceView invoice={invoice} />}
			</main>
		</>
	);
}
