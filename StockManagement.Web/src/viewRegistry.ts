// Registry: each feature owns its own ViewDef file; this only orders them (see ADR-0026).

import type { LucideIcon } from "lucide-react";
import type { Permission } from "./api";
import type { ReactNode } from "react";
import { companySettingsView } from "./features/settings/companySettingsView";
import { customersView } from "./features/customers/view";
import { invoicesView } from "./features/invoices/view";
import { salesView } from "./features/sales/view";
import { settingsView } from "./features/settings/view";
import { stockItemsView } from "./features/stock-items/view";
import { usersView } from "./features/users/view";
import type { Invoice } from "./api";

export type ViewName = "stockItems" | "clients" | "newSale" | "invoices" | "companySettings" | "settings" | "users";

export type ViewContext = { invoice: Invoice | undefined; onSold: (invoice: Invoice) => void };

export type ViewDef = { name: ViewName; icon: LucideIcon; permission?: Permission; render: (ctx: ViewContext) => ReactNode };

// Same order and icons as the WPF menu (FontAwesome Wrench, AddressBook, Inbox)
export const views: ViewDef[] = [stockItemsView, customersView, salesView, invoicesView, companySettingsView, settingsView, usersView];
