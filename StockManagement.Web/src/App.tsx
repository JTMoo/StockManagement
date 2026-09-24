import { BookUser, Inbox, Menu, Settings, ShoppingCart, Wrench, type LucideIcon } from "lucide-react";
import { useState } from "react";
import type { Invoice } from "./api";
import { CustomerList } from "./features/customers/CustomerList";
import { InvoiceBrowser } from "./features/invoices/InvoiceBrowser";
import { SaleForm } from "./features/sales/SaleForm";
import { SettingsPage } from "./features/settings/SettingsPage";
import { StockItemList } from "./features/stock-items/StockItemList";
import { useI18n } from "./i18n";

type View = "stockItems" | "clients" | "newSale" | "invoices" | "settings";

// Same order and icons as the WPF menu (FontAwesome Wrench, AddressBook, Inbox)
const views: { name: View; icon: LucideIcon }[] = [
	{ name: "stockItems", icon: Wrench },
	{ name: "clients", icon: BookUser },
	{ name: "newSale", icon: ShoppingCart },
	{ name: "invoices", icon: Inbox },
	{ name: "settings", icon: Settings }
];

export function App()
{
	const { t } = useI18n();
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
				{view === "settings" && <SettingsPage />}
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
				</div>
			</nav>
		</div>
	);
}
