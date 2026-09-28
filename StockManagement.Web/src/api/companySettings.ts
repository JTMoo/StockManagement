import { send } from "./client";

export type CompanySettings = { companyName: string; taxId: string; currency: string; vatRatePercent: number; paymentTermInDays: number; firstInvoiceNumber: number; firstCustomerId: number; currencyDecimalDigits: number };

export const companySettingsApi = {
	getCompanySettings: (signal?: AbortSignal) => send<CompanySettings>("/company-settings", { signal }),
	updateCompanySettings: (settings: CompanySettings) => send<CompanySettings>("/company-settings", { method: "PUT", body: JSON.stringify(settings) })
};
