import { Inbox } from "lucide-react";
import type { NavRoute } from "../../routes";
import { InvoiceBrowser } from "./InvoiceBrowser";

export const invoicesRoute: NavRoute = {
	name: "invoices",
	icon: Inbox,
	render: ({ invoice }) => <InvoiceBrowser invoice={invoice} />
};
