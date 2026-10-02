import { send, sendAllPages } from "./client";

export type Customer = { customerId: number; name: string; lastname: string; address: string; phoneNumber: string; identificationNumber: string; postboxNumber: string; email: string; miscellaneous: string };

export type NewCustomer = Partial<Omit<Customer, "customerId">> & { name: string };

export const customersApi = {
	listCustomers: (signal?: AbortSignal) => sendAllPages<Customer>("/customers", signal),
	createCustomer: (customer: NewCustomer) => send<Customer>("/customers", { method: "POST", body: JSON.stringify(customer) }),
	updateCustomer: (customer: Customer) => send<Customer>(`/customers/${customer.customerId}`, { method: "PUT", body: JSON.stringify(customer) })
};
