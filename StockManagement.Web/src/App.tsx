import { BookUser, Inbox, Languages, Menu, ShoppingCart, Wrench, type LucideIcon } from "lucide-react";
import { useState } from "react";
import type { Invoice } from "./api";
import { CustomerList } from "./features/customers/CustomerList";
import { InvoiceBrowser } from "./features/invoices/InvoiceBrowser";
import { SaleForm } from "./features/sales/SaleForm";
import { StockItemList } from "./features/stock-items/StockItemList";
import { cultures, useI18n, type Culture, type TextKey } from "./i18n";

type View = "stockItems" | "clients" | "newSale" | "invoices";

// Same order and icons as the WPF menu (FontAwesome Wrench, AddressBook, Inbox)
const views: { name: View; icon: LucideIcon }[] = [
	{ name: "stockItems", icon: Wrench },
	{ name: "clients", icon: BookUser },
	{ name: "newSale", icon: ShoppingCart },
	{ name: "invoices", icon: Inbox }
];
const cultureNames: Record<Culture, TextKey> = { "de-DE": "german", "en-US": "english", "es-PY": "spanish" };

export function App()
{
	const { t, culture, setCulture } = useI18n();
	const [view, setView] = useState<View>("stockItems");
	const [invoice, setInvoice] = useState<Invoice>();
	const [menuExtended, setMenuExtended] = useState(true);

	function onSold(sold: Invoice)
	{
		setInvoice(sold);
		setView("invoices");
	}

	return (
		<div className="shell">
			<main>
				{view === "stockItems" && <StockItemList />}
				{view === "clients" && <CustomerList />}
				{view === "newSale" && <SaleForm onSold={onSold} />}
				{view === "invoices" && <InvoiceBrowser invoice={invoice} />}
			</main>
			<nav className={menuExtended ? "menu" : "menu collapsed"}>
				<button className="menu-item" aria-label="Menu" aria-expanded={menuExtended} onClick={() => setMenuExtended(!menuExtended)}>
					<Menu />
				</button>
				<div className="menu-bottom">
					{views.map(({ name, icon: Icon }) => (
						<button key={name} className="menu-item" aria-label={t(name)} aria-pressed={view === name} onClick={() => setView(name)}>
							<Icon />{menuExtended && <span>{t(name)}</span>}
						</button>
					))}
					<label className="menu-item">
						<Languages />
						<select aria-label={t("selectLanguage")} value={culture} onChange={event => setCulture(event.target.value as Culture)}>
							{cultures.map(name => <option key={name} value={name}>{t(cultureNames[name])}</option>)}
						</select>
					</label>
				</div>
			</nav>
		</div>
	);
}
