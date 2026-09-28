import { send } from "./client";

export type Language = "German" | "English" | "Spanish";

export type Settings = { language: Language };

export type CompanySettings = { companyName: string; taxId: string; currency: string; vatRatePercent: number; paymentTermInDays: number; firstInvoiceNumber: number; firstCustomerId: number; currencyDecimalDigits: number };

export const settingsApi = {
	getSettings: (signal?: AbortSignal) => send<Settings>("/settings", { signal }),
	updateSettings: (language: Language) => send<Settings>("/settings", { method: "PUT", body: JSON.stringify({ language }) }),
	getCompanySettings: (signal?: AbortSignal) => send<CompanySettings>("/company-settings", { signal }),
	updateCompanySettings: (settings: CompanySettings) => send<CompanySettings>("/company-settings", { method: "PUT", body: JSON.stringify(settings) })
};
