import { StatusBadge, type StatusTone } from "../../StatusBadge";
import type { Invoice } from "../../api";
import type { TextKey } from "../../i18n";

const toneByStatus: Record<Invoice["status"], StatusTone> = { Open: "warning", PartiallyPaid: "warning", Paid: "success", Overdue: "danger", Cancelled: "muted" };
const labelKeyByStatus: Record<Invoice["status"], TextKey> = { Open: "statusPending", PartiallyPaid: "statusPending", Paid: "statusPaid", Overdue: "statusOverdue", Cancelled: "cancelled" };

/** Cancelled invoices keep the `invoice-cancelled` testid regardless of the computed status (ADR-0030 cancel flow). */
export function invoiceStatusBadge(invoice: Invoice, t: (key: TextKey) => string)
{
	if (invoice.isCancelled) return <StatusBadge tone="muted" testId="invoice-cancelled">{t("cancelled")}</StatusBadge>;
	return <StatusBadge tone={toneByStatus[invoice.status]}>{t(labelKeyByStatus[invoice.status])}</StatusBadge>;
}
