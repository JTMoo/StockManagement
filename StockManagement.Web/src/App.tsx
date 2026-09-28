import { LogOut, Menu } from "lucide-react";
import { useState } from "react";
import type { Invoice } from "./api";
import { useAuth } from "./auth";
import { LoginPage } from "./features/auth/LoginPage";
import { useI18n } from "./i18n";
import { type ViewName, views } from "./viewRegistry";

export function App()
{
	const { t } = useI18n();
	const { username, logout, hasPermission } = useAuth();
	const [view, setView] = useState<ViewName>("stockItems");
	const [invoice, setInvoice] = useState<Invoice>();
	const [menuExtended, setMenuExtended] = useState(true);

	function onSold(sold: Invoice)
	{
		setInvoice(sold);
		setView("invoices");
	}

	if (!username) return <LoginPage />;

	const visibleViews = views.filter(v => !v.permission || hasPermission(v.permission));
	const active = views.find(v => v.name === view);

	return (
		<div className="shell">
			<main>{active?.render({ invoice, onSold })}</main>
			<nav className={menuExtended ? "menu" : "menu collapsed"}>
				<button className="menu-item" aria-label="Menu" aria-expanded={menuExtended} onClick={() => setMenuExtended(!menuExtended)}>
					<Menu />
				</button>
				<div className="menu-bottom">
					{visibleViews.map(({ name, icon: Icon }) => (
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
