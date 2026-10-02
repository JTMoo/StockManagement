import { ImportPanel } from "../import/ImportPanel";

/** Embedded in InvoiceBrowser (toggled by its "Open Invoices" toolbar button), not its own nav entry. */
export function OpenInvoiceImport({ onImported }: { onImported?: () => void })
{
	return <ImportPanel target="OpenInvoices" onImported={onImported} />;
}
