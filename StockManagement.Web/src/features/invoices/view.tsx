import { Inbox } from "lucide-react";
import type { ViewDef } from "../../viewRegistry";
import { InvoiceBrowser } from "./InvoiceBrowser";

export const invoicesView: ViewDef = { name: "invoices", icon: Inbox, render: ({ invoice }) => <InvoiceBrowser invoice={invoice} /> };
