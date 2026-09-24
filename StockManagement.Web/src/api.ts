// Typed client for StockManagement.Api (see ADR-0004). Expected failures are results, not exceptions.

export type SaleCondition = "Cash" | "Credit";

export type StockItem = { code: string; name: string; description: string; location: string; amount: number; price: number; manufacturer: string };

export type Customer = { customerId: number; name: string; lastname: string; address: string; phoneNumber: string; identificationNumber: string; postboxNumber: string; email: string; miscellaneous: string };

export type NewCustomer = Partial<Omit<Customer, "customerId">> & { name: string };

export type InvoiceLine = { code: string; name: string; amount: number; unitPrice: number };

export type Invoice = { number: number; date: string; expirationDate: string; total: number; tax: number; saleCondition: SaleCondition; customerId: number; lines: InvoiceLine[] };

export type NewSale = { customerId: number; saleCondition: SaleCondition; items: { code: string; amount: number }[] };

export type ApiFailure =
	| { kind: "notFound" }
	| { kind: "invalid"; codes: string[] }
	| { kind: "conflict"; unavailableItems: string[] }
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

	if (response.ok) return { ok: true, value: (await response.json()) as T };
	if (response.status === 404) return { ok: false, failure: { kind: "notFound" } };
	if (response.status === 409) return { ok: false, failure: { kind: "conflict", unavailableItems: ((await response.json()) as { unavailableItems: string[] }).unavailableItems } };
	if (response.status === 400) return { ok: false, failure: { kind: "invalid", codes: ((await response.json()) as ProblemDetails).errors?.map(error => error.reason) ?? [] } };
	return { ok: false, failure: { kind: "unexpected" } };
}

export const api = {
	listStockItems: (signal?: AbortSignal) => send<StockItem[]>("/stock-items", { signal }),
	listCustomers: (signal?: AbortSignal) => send<Customer[]>("/customers", { signal }),
	createCustomer: (customer: NewCustomer) => send<Customer>("/customers", { method: "POST", body: JSON.stringify(customer) }),
	createSale: (sale: NewSale) => send<Invoice>("/sales", { method: "POST", body: JSON.stringify(sale) }),
	getInvoice: (number: number, signal?: AbortSignal) => send<Invoice>(`/invoices/${number}`, { signal })
};
