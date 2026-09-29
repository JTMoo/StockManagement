import { send } from "./client";

export type Language = "German" | "English" | "Spanish";

export type Settings = { language: Language };

export const settingsApi = {
	getSettings: (signal?: AbortSignal) => send<Settings>("/settings", { signal }),
	updateSettings: (language: Language) => send<Settings>("/settings", { method: "PUT", body: JSON.stringify({ language }) })
};
