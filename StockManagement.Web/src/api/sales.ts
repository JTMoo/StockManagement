import { send } from "./client";
import type { Invoice, SaleCondition } from "./invoices";

export type NewSale = { customerId: number; saleCondition: SaleCondition; items: { code: string; amount: number }[] };

export const salesApi = {
	createSale: (sale: NewSale) => send<Invoice>("/sales", { method: "POST", body: JSON.stringify(sale) })
};
