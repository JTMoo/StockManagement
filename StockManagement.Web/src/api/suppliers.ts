import { send } from "./client";

export type Supplier = { id: string; name: string; contactName: string; country: string; currency: string; leadTimeDays: number; miscellaneous: string };

export type NewSupplier = Partial<Omit<Supplier, "id">> & { name: string };

export const suppliersApi = {
	listSuppliers: (signal?: AbortSignal) => send<Supplier[]>("/suppliers", { signal }),
	createSupplier: (supplier: NewSupplier) => send<Supplier>("/suppliers", { method: "POST", body: JSON.stringify(supplier) }),
	updateSupplier: (supplier: Supplier) => send<Supplier>(`/suppliers/${encodeURIComponent(supplier.id)}`, { method: "PUT", body: JSON.stringify(supplier) }),
	deleteSupplier: (supplier: Supplier) => send<void>(`/suppliers/${encodeURIComponent(supplier.id)}`, { method: "DELETE" })
};
