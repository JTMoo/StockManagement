import { send } from "./client";
import type { SaleCondition } from "./sales";

export type InvoiceLine = { code: string; name: string; amount: number; unitPrice: number };

export type Invoice = { number: number; date: string; expirationDate: string; total: number; tax: number; saleCondition: SaleCondition; customerId: number; customerName: string; isCancelled: boolean; lines: InvoiceLine[] };

export type CreditNote = { number: number; date: string; reason: string; total: number; tax: number; invoiceNumber: number };

export type InvoiceListResult = { items: Invoice[]; nextCursor: string | null };

export type InvoiceFilter = { customerId?: number; from?: string; to?: string; cursor?: string; pageSize: number };

export const invoicesApi = {
	getInvoice: (number: number, signal?: AbortSignal) => send<Invoice>(`/invoices/${number}`, { signal }),
	listInvoices: (filter: InvoiceFilter, signal?: AbortSignal) => send<InvoiceListResult>(`/invoices?${invoiceFilterQuery(filter)}`, { signal }),
	cancelInvoice: (number: number, reason: string) => send<CreditNote>(`/invoices/${number}/cancel`, { method: "POST", body: JSON.stringify({ number, reason }) })
};

function invoiceFilterQuery(filter: InvoiceFilter): string
{
	const params = new URLSearchParams({ pageSize: String(filter.pageSize) });
	if (filter.customerId !== undefined) params.set("customerId", String(filter.customerId));
	if (filter.from) params.set("from", filter.from);
	if (filter.to) params.set("to", filter.to);
	if (filter.cursor) params.set("cursor", filter.cursor);
	return params.toString();
}
