// Typed client for StockManagement.Api (see ADR-0004). Expected failures are results, not exceptions.

export type SaleCondition = "Cash" | "Credit";

export type StockItem = { id: string; code: string; name: string; description: string; location: string; amount: number; price: number; manufacturer: string };

export type NewStockItem = Partial<Omit<StockItem, "id" | "code">> & { code: string; name: string };

export type Customer = { customerId: number; name: string; lastname: string; address: string; phoneNumber: string; identificationNumber: string; postboxNumber: string; email: string; miscellaneous: string };

export type NewCustomer = Partial<Omit<Customer, "customerId">> & { name: string };

export type InvoiceLine = { code: string; name: string; amount: number; unitPrice: number };

export type Invoice = { number: number; date: string; expirationDate: string; total: number; tax: number; saleCondition: SaleCondition; customerId: number; lines: InvoiceLine[] };

export type NewSale = { customerId: number; saleCondition: SaleCondition; items: { code: string; amount: number }[] };

export type ApiFailure =
	| { kind: "notFound" }
	| { kind: "invalid"; codes: string[] }
	| { kind: "conflict"; unavailableItems: string[] }
	| { kind: "duplicate"; code: string }
	| { kind: "unexpected" };

export type Result<T> = { ok: true; value: T } | { ok: false; failure: ApiFailure };

type ProblemDetails = { errors?: { reason: string }[] };

async function send<T>(path: string, init?: RequestInit): Promise<Result<T>>
{
	let response: Response;
	try
	{
		response = await fetch(`/api${path}`, { ...init, headers: { "Content-Type": "application/json" } });
	}
	catch
	{
		return { ok: false, failure: { kind: "unexpected" } };
	}

	if (response.ok) return { ok: true, value: (response.status === 204 ? undefined : await response.json()) as T };
	if (response.status === 404) return { ok: false, failure: { kind: "notFound" } };
	if (response.status === 409)
	{
		const body = (await response.json()) as { unavailableItems?: string[]; code?: string };
		if (body.unavailableItems) return { ok: false, failure: { kind: "conflict", unavailableItems: body.unavailableItems } };
		return { ok: false, failure: { kind: "duplicate", code: body.code ?? "" } };
	}
	if (response.status === 400) return { ok: false, failure: { kind: "invalid", codes: ((await response.json()) as ProblemDetails).errors?.map(error => error.reason) ?? [] } };
	return { ok: false, failure: { kind: "unexpected" } };
}

export const api = {
	listStockItems: (signal?: AbortSignal) => send<StockItem[]>("/stock-items", { signal }),
	createStockItem: (stockItem: NewStockItem) => send<StockItem>("/stock-items", { method: "POST", body: JSON.stringify(stockItem) }),
	updateStockItem: (stockItem: StockItem) => send<StockItem>(`/stock-items/${encodeURIComponent(stockItem.id)}`, { method: "PUT", body: JSON.stringify(stockItem) }),
	deleteStockItem: (stockItem: StockItem) => send<void>(`/stock-items/${encodeURIComponent(stockItem.id)}`, { method: "DELETE" }),
	listCustomers: (signal?: AbortSignal) => send<Customer[]>("/customers", { signal }),
	createCustomer: (customer: NewCustomer) => send<Customer>("/customers", { method: "POST", body: JSON.stringify(customer) }),
	updateCustomer: (customer: Customer) => send<Customer>(`/customers/${customer.customerId}`, { method: "PUT", body: JSON.stringify(customer) }),
	createSale: (sale: NewSale) => send<Invoice>("/sales", { method: "POST", body: JSON.stringify(sale) }),
	getInvoice: (number: number, signal?: AbortSignal) => send<Invoice>(`/invoices/${number}`, { signal })
};
