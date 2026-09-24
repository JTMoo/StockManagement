import { render } from "@testing-library/react";
import type { ReactElement } from "react";
import { vi } from "vitest";
import { I18nProvider } from "./i18n";

export type Route = { status?: number; body?: unknown };

/** Stubs fetch: key is "METHOD /api/path"; returns the fetch mock to inspect calls. */
export function mockApi(routes: Record<string, Route>)
{
	const fetchMock = vi.fn(async (url: string, init?: RequestInit) =>
	{
		const route = routes[`${init?.method ?? "GET"} ${url}`];
		if (!route) throw new TypeError("Failed to fetch");
		return new Response(JSON.stringify(route.body ?? null), { status: route.status ?? 200 });
	});
	vi.stubGlobal("fetch", fetchMock);
	return fetchMock;
}

export function sentBody(fetchMock: ReturnType<typeof mockApi>, key: string): unknown
{
	const call = fetchMock.mock.calls.find(([url, init]) => `${init?.method ?? "GET"} ${url}` === key);
	return call ? JSON.parse(call[1]!.body as string) : undefined;
}

export function renderEnglish(ui: ReactElement)
{
	return render(<I18nProvider culture="en-US">{ui}</I18nProvider>);
}

export const screw = { code: "A1", name: "Screw", description: "M6", location: "A-1", amount: 10, price: 5000, manufacturer: "None" };
export const nut = { code: "B2", name: "Nut", description: "M6", location: "B-2", amount: 0, price: 1000, manufacturer: "None" };
export const ana = { customerId: 1001, name: "Ana", lastname: "Gómez", address: "", phoneNumber: "", identificationNumber: "", postboxNumber: "", email: "", miscellaneous: "" };
export const invoice = { number: 7, date: "2026-09-24T10:00:00", expirationDate: "2026-10-24T10:00:00", total: 10000, tax: 909, saleCondition: "Cash", customerId: 1001, customerName: "Ana Gómez", lines: [{ code: "A1", name: "Screw", amount: 2, unitPrice: 5000 }] };
