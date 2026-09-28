import { Banknote, Settings } from "lucide-react";
import type { NavRoute } from "../../routes";
import { CompanySettingsPage } from "./CompanySettingsPage";
import { SettingsPage } from "./SettingsPage";

export const companySettingsRoute: NavRoute = {
	name: "companySettings",
	icon: Banknote,
	render: () => <CompanySettingsPage />
};

export const settingsRoute: NavRoute = {
	name: "settings",
	icon: Settings,
	render: () => <SettingsPage />
};
