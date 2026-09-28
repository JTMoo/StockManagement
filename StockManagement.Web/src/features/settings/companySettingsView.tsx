import { Banknote } from "lucide-react";
import type { ViewDef } from "../../viewRegistry";
import { CompanySettingsPage } from "./CompanySettingsPage";

export const companySettingsView: ViewDef = { name: "companySettings", icon: Banknote, render: () => <CompanySettingsPage /> };
