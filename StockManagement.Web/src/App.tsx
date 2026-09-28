import { LogOut, Menu } from "lucide-react";
import { useState } from "react";
import type { Invoice } from "./api";
import { useAuth } from "./auth";
import { LoginPage } from "./features/auth/LoginPage";
import { customersRoute } from "./features/customers/route";
import { invoicesRoute } from "./features/invoices/route";
import { salesRoute } from "./features/sales/route";
import { companySettingsRoute, settingsRoute } from "./features/settings/routes";
import { stockItemsRoute } from "./features/stock-items/route";
import { suppliersRoute } from "./features/suppliers/route";
import { usersRoute } from "./features/users/route";
import { useI18n } from "./i18n";
import type { NavRoute, View } from "./routes";

// Same order and icons as the WPF menu (FontAwesome Wrench, AddressBook, Inbox)
const routes: NavRoute[] = [stockItemsRoute, customersRoute, suppliersRoute, salesRoute, invoicesRoute, companySettingsRoute, settingsRoute, usersRoute];

export function App()
{
	const { t } = useI18n();
	const { username, logout, hasPermission } = useAuth();
	const [view, setView] = useState<View>("stockItems");
	const [invoice, setInvoice] = useState<Invoice>();
	const [menuExtended, setMenuExtended] = useState(true);

	function onSold(sold: Invoice)
	{
		setInvoice(sold);
		setView("invoices");
	}

	if (!username) return <LoginPage />;

	const visibleRoutes = routes.filter(route => !route.permission || hasPermission(route.permission));
	const activeRoute = visibleRoutes.find(route => route.name === view) ?? visibleRoutes[0];

	return (
		<div className="shell">
			<main>{activeRoute.render({ invoice, onSold })}</main>
			<nav className={menuExtended ? "menu" : "menu collapsed"}>
				<button className="menu-item" aria-label="Menu" aria-expanded={menuExtended} onClick={() => setMenuExtended(!menuExtended)}>
					<Menu />
				</button>
				<div className="menu-bottom">
					{visibleRoutes.map(({ name, icon: Icon }) => (
						<button key={name} className="menu-item" aria-label={t(name)} aria-pressed={view === name} onClick={() => setView(name)}>
							<Icon />{menuExtended && <span>{t(name)}</span>}
						</button>
					))}
					<button className="menu-item" aria-label={t("logout")} onClick={logout}>
						<LogOut />{menuExtended && <span>{t("logout")}</span>}
					</button>
				</div>
			</nav>
		</div>
	);
}
