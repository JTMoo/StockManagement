import { screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { describe, expect, it } from "vitest";
import { mockApi, renderEnglish } from "../../test-utils";
import { SettingsPage } from "./SettingsPage";

describe("SettingsPage", () =>
{
	it("Load_German_ShowsGermanSelected", async () =>
	{
		// Arrange
		mockApi({ "GET /api/settings": { body: { language: "German" } } });
		renderEnglish(<SettingsPage />);

		// Assert
		expect(await screen.findByRole("combobox")).toHaveValue("de-DE");
	});

	it("SelectLanguage_Spanish_PutsAndSwitchesTexts", async () =>
	{
		// Arrange
		mockApi({
			"GET /api/settings": { body: { language: "English" } },
			"PUT /api/settings": { body: { language: "Spanish" } }
		});
		renderEnglish(<SettingsPage />);
		await screen.findByRole("combobox");

		// Act
		await userEvent.selectOptions(screen.getByRole("combobox"), "es-PY");

		// Assert
		expect(await screen.findByRole("heading", { name: "Configuracion" })).toBeInTheDocument();
	});
});
