import { send } from "./client";
import type { Invoice } from "./invoices";

export type SaleCondition = "Cash" | "Credit";

export type NewSale = { customerId: number; saleCondition: SaleCondition; items: { code: string; amount: number }[] };

export const salesApi = {
	createSale: (sale: NewSale) => send<Invoice>("/sales", { method: "POST", body: JSON.stringify(sale) })
};
