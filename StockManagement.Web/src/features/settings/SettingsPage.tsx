import { api, type Language } from "../../api";
import { FailureMessage } from "../../FailureMessage";
import { Page } from "../../Page";
import { cultures, useI18n, type Culture, type TextKey } from "../../i18n";
import { useLoad } from "../../useLoad";

const languageByCulture: Record<Culture, Language> = { "de-DE": "German", "en-US": "English", "es-PY": "Spanish" };
const cultureByLanguage: Record<Language, Culture> = { German: "de-DE", English: "en-US", Spanish: "es-PY" };
const cultureNames: Record<Culture, TextKey> = { "de-DE": "german", "en-US": "english", "es-PY": "spanish" };

export function SettingsPage()
{
	const { t, setCulture } = useI18n();
	const { data: settings, setData, failure } = useLoad(api.getSettings);

	async function onChange(culture: Culture)
	{
		const result = await api.updateSettings(languageByCulture[culture]);
		if (!result.ok) return;
		setData(result.value);
		setCulture(culture);
	}

	return (
		<Page title={t("settings")}>
			<FailureMessage failure={failure} />
			<div className="form-grid">
				<label>
					{t("selectLanguage")}
					<select value={settings ? cultureByLanguage[settings.language] : ""} onChange={event => onChange(event.target.value as Culture)}>
						{cultures.map(name => <option key={name} value={name}>{t(cultureNames[name])}</option>)}
					</select>
				</label>
			</div>
		</Page>
	);
}
