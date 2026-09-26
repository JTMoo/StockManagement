// Typed client for StockManagement.Api (see ADR-0004). Expected failures are results, not exceptions.

export type SaleCondition = "Cash" | "Credit";

export type StockItem = { id: string; code: string; name: string; description: string; location: string; amount: number; price: number; manufacturer: string };

export type NewStockItem = Partial<Omit<StockItem, "id" | "code">> & { code: string; name: string };

export type Customer = { customerId: number; name: string; lastname: string; address: string; phoneNumber: string; identificationNumber: string; postboxNumber: string; email: string; miscellaneous: string };

export type NewCustomer = Partial<Omit<Customer, "customerId">> & { name: string };

export type InvoiceLine = { code: string; name: string; amount: number; unitPrice: number };

export type Invoice = { number: number; date: string; expirationDate: string; total: number; tax: number; saleCondition: SaleCondition; customerId: number; customerName: string; lines: InvoiceLine[] };

export type InvoiceListResult = { items: Invoice[]; totalCount: number };

export type InvoiceFilter = { customerId?: number; from?: string; to?: string; page: number; pageSize: number };

export type NewSale = { customerId: number; saleCondition: SaleCondition; items: { code: string; amount: number }[] };

export type Language = "German" | "English" | "Spanish";

export type Settings = { language: Language };

export type CompanySettings = { companyName: string; taxId: string; currency: string; vatRatePercent: number; paymentTermInDays: number; firstInvoiceNumber: number; firstCustomerId: number };

export type StockItemImportRowError = { row: number; message: string };

export type ImportTarget = "StockItems" | "Customers";

export type ImportRowStatus = "Ready" | "Duplicate" | "Error";

export type ImportBatchStatus = "Previewed" | "Committed" | "Undone";

export type ImportBatchRow = { row: number; status: ImportRowStatus; message?: string; fields?: Record<string, unknown> };

export type ImportBatch = {
	id: string;
	target: ImportTarget;
	fileName: string;
	sheetName: string;
	status: ImportBatchStatus;
	readyCount: number;
	duplicateCount: number;
	errorCount: number;
	rows: ImportBatchRow[];
};

export type StockItemImportResult = { sheetName: string; imported: number; duplicates: number; errors: StockItemImportRowError[] };

export type LoginResult = { token: string; username: string };

export type ApiFailure =
	| { kind: "notFound" }
	| { kind: "invalid"; codes: string[] }
	| { kind: "conflict"; unavailableItems: string[] }
	| { kind: "duplicate"; code: string }
	| { kind: "insufficientStock"; inStock: number }
	| { kind: "invalidState"; reason: string }
	| { kind: "unauthorized" }
	| { kind: "unexpected" };

export type Result<T> = { ok: true; value: T } | { ok: false; failure: ApiFailure };

type ProblemDetails = { errors?: { reason: string }[] };

// Set by AuthProvider; kept out of React so api.ts has no framework dependency
let authToken: string | null = null;
let onUnauthorized: (() => void) | null = null;

export function setAuthToken(token: string | null)
{
	authToken = token;
}

export function setUnauthorizedHandler(handler: (() => void) | null)
{
	onUnauthorized = handler;
}

function authHeaders(): Record<string, string>
{
	return authToken ? { "Authorization": `Bearer ${authToken}` } : {};
}

async function send<T>(path: string, init?: RequestInit): Promise<Result<T>>
{
	// Content-Type only with a body: FastEndpoints otherwise tries to parse the (empty) GET body as JSON and rejects it
	const headers = { ...authHeaders(), ...(init?.body ? { "Content-Type": "application/json" } : {}) };
	return handleResponse(() => fetch(`/api${path}`, { ...init, headers }));
}

async function sendForm<T>(path: string, file: File, fields?: Record<string, string>): Promise<Result<T>>
{
	const body = new FormData();
	for (const [key, value] of Object.entries(fields ?? {})) body.append(key, value);
	body.append("File", file);
	return handleResponse(() => fetch(`/api${path}`, { method: "POST", body, headers: authHeaders() }));
}

async function handleResponse<T>(fetchCall: () => Promise<Response>): Promise<Result<T>>
{
	let response: Response;
	try
	{
		response = await fetchCall();
	}
	catch
	{
		return { ok: false, failure: { kind: "unexpected" } };
	}

	if (response.ok) return { ok: true, value: (response.status === 204 ? undefined : await response.json()) as T };
	if (response.status === 404) return { ok: false, failure: { kind: "notFound" } };
	if (response.status === 401)
	{
		onUnauthorized?.();
		return { ok: false, failure: { kind: "unauthorized" } };
	}
	if (response.status === 409)
	{
		const body = (await response.json()) as { unavailableItems?: string[]; code?: string; inStock?: number; reason?: string };
		if (body.unavailableItems) return { ok: false, failure: { kind: "conflict", unavailableItems: body.unavailableItems } };
		if (body.inStock !== undefined) return { ok: false, failure: { kind: "insufficientStock", inStock: body.inStock } };
		if (body.reason !== undefined) return { ok: false, failure: { kind: "invalidState", reason: body.reason } };
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
	checkInStockItem: (id: string, amount: number, reason: string) => send<StockItem>(`/stock-items/${encodeURIComponent(id)}/check-in`, { method: "POST", body: JSON.stringify({ amount, reason }) }),
	checkOutStockItem: (id: string, amount: number, reason: string) => send<StockItem>(`/stock-items/${encodeURIComponent(id)}/check-out`, { method: "POST", body: JSON.stringify({ amount, reason }) }),
	importStockItems: (file: File) => sendForm<StockItemImportResult>("/stock-items/import", file),
	previewImport: (target: ImportTarget, file: File) => sendForm<ImportBatch>("/import/batches", file, { Target: target }),
	commitImportBatch: (id: string) => send<ImportBatch>(`/import/batches/${encodeURIComponent(id)}/commit`, { method: "POST" }),
	undoImportBatch: (id: string) => send<ImportBatch>(`/import/batches/${encodeURIComponent(id)}/undo`, { method: "POST" }),
	listCustomers: (signal?: AbortSignal) => send<Customer[]>("/customers", { signal }),
	createCustomer: (customer: NewCustomer) => send<Customer>("/customers", { method: "POST", body: JSON.stringify(customer) }),
	updateCustomer: (customer: Customer) => send<Customer>(`/customers/${customer.customerId}`, { method: "PUT", body: JSON.stringify(customer) }),
	createSale: (sale: NewSale) => send<Invoice>("/sales", { method: "POST", body: JSON.stringify(sale) }),
	getInvoice: (number: number, signal?: AbortSignal) => send<Invoice>(`/invoices/${number}`, { signal }),
	listInvoices: (filter: InvoiceFilter, signal?: AbortSignal) => send<InvoiceListResult>(`/invoices?${invoiceFilterQuery(filter)}`, { signal }),
	getSettings: (signal?: AbortSignal) => send<Settings>("/settings", { signal }),
	updateSettings: (language: Language) => send<Settings>("/settings", { method: "PUT", body: JSON.stringify({ language }) }),
	getCompanySettings: (signal?: AbortSignal) => send<CompanySettings>("/company-settings", { signal }),
	updateCompanySettings: (settings: CompanySettings) => send<CompanySettings>("/company-settings", { method: "PUT", body: JSON.stringify(settings) }),
	login: (username: string, password: string) => send<LoginResult>("/auth/login", { method: "POST", body: JSON.stringify({ username, password }) })
};

function invoiceFilterQuery(filter: InvoiceFilter): string
{
	const params = new URLSearchParams({ page: String(filter.page), pageSize: String(filter.pageSize) });
	if (filter.customerId !== undefined) params.set("customerId", String(filter.customerId));
	if (filter.from) params.set("from", filter.from);
	if (filter.to) params.set("to", filter.to);
	return params.toString();
}
