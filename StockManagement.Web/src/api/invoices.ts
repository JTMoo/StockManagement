import { send } from "./client";

export type SaleCondition = "Cash" | "Credit";

export type InvoiceLine = { code: string; name: string; amount: number; unitPrice: number };

export type Invoice = { number: number; date: string; expirationDate: string; total: number; tax: number; saleCondition: SaleCondition; customerId: number; customerName: string; lines: InvoiceLine[] };

export type InvoiceListResult = { items: Invoice[]; totalCount: number };

export type InvoiceFilter = { customerId?: number; from?: string; to?: string; page: number; pageSize: number };

export const invoicesApi = {
	getInvoice: (number: number, signal?: AbortSignal) => send<Invoice>(`/invoices/${number}`, { signal }),
	listInvoices: (filter: InvoiceFilter, signal?: AbortSignal) => send<InvoiceListResult>(`/invoices?${invoiceFilterQuery(filter)}`, { signal })
};

function invoiceFilterQuery(filter: InvoiceFilter): string
{
	const params = new URLSearchParams({ page: String(filter.page), pageSize: String(filter.pageSize) });
	if (filter.customerId !== undefined) params.set("customerId", String(filter.customerId));
	if (filter.from) params.set("from", filter.from);
	if (filter.to) params.set("to", filter.to);
	return params.toString();
}
