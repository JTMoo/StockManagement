import { createContext, useContext, useMemo, useState, type ReactNode } from "react";
import deDE from "./i18n/de-DE.json";
import enUS from "./i18n/en-US.json";
import esPY from "./i18n/es-PY.json";

// Texts come from StockManagement.Language (scripts/resx-to-json.mjs); keys are the resx names.
const texts = { "de-DE": deDE, "en-US": enUS, "es-PY": esPY };

export type Culture = keyof typeof texts;
export type TextKey = keyof typeof enUS;
export const cultures = Object.keys(texts) as Culture[];

type I18n = {
	culture: Culture;
	setCulture: (culture: Culture) => void;
	t: (key: TextKey) => string;
	formatNumber: (value: number) => string;
	formatDate: (value: string) => string;
};

const I18nContext = createContext<I18n | null>(null);

export function isTextKey(key: string): key is TextKey
{
	return key in enUS;
}

function initialCulture(): Culture
{
	return cultures.find(culture => culture.startsWith(navigator.language.slice(0, 2))) ?? "en-US";
}

export function I18nProvider({ children, culture: fixedCulture }: { children: ReactNode; culture?: Culture })
{
	const [culture, setCulture] = useState<Culture>(fixedCulture ?? initialCulture);
	const value = useMemo<I18n>(() => ({
		culture,
		setCulture,
		t: key => texts[culture][key] ?? key,
		formatNumber: value => new Intl.NumberFormat(culture).format(value),
		formatDate: value => new Date(value).toLocaleDateString(culture)
	}), [culture]);

	return <I18nContext.Provider value={value}>{children}</I18nContext.Provider>;
}

export function useI18n(): I18n
{
	const i18n = useContext(I18nContext);
	if (!i18n) throw new Error("useI18n needs an I18nProvider.");
	return i18n;
}
