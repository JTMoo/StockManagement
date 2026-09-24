import { useEffect, useState } from "react";
import type { Invoice } from "../../api";
import { Page } from "../../Page";
import { useI18n } from "../../i18n";
import { InvoiceList } from "./InvoiceList";
import { InvoiceView } from "./InvoiceView";

/** Invoices tab: browsable list, or the invoice a row (or a just-completed sale) selected. */
export function InvoiceBrowser({ invoice: initial }: { invoice?: Invoice })
{
	const { t } = useI18n();
	const [selected, setSelected] = useState(initial);

	useEffect(() => setSelected(initial), [initial]);

	if (selected) return <InvoiceView invoice={selected} onBack={() => setSelected(undefined)} />;
	return <Page title={t("invoices")}><InvoiceList onSelect={setSelected} /></Page>;
}
