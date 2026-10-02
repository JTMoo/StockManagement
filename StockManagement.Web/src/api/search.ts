import { send } from "./client";

export type SearchDomain = "StockItems" | "Customers" | "Invoices" | "Suppliers";

export type SearchHit = { id: string; title: string; subtitle: string };

export type SearchGroup = { domain: SearchDomain; items: SearchHit[]; totalCount: number };

export type SearchResponse = { groups: SearchGroup[] };

export const searchApi = {
	search: (q: string, includeInactive: boolean, signal?: AbortSignal) =>
		send<SearchResponse>(`/search?q=${encodeURIComponent(q)}&includeInactive=${includeInactive}`, { signal })
};
