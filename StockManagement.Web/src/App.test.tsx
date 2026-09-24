import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { describe, expect, it } from "vitest";
import { App } from "./App";
import { I18nProvider } from "./i18n";
import { mockApi } from "./test-utils";

describe("App", () =>
{
	it("SelectLanguage_German_ShowsGermanTexts", async () =>
	{
		// Arrange
		mockApi({
			"GET /api/stock-items": { body: [] },
			"GET /api/settings": { body: { language: "English" } },
			"PUT /api/settings": { body: { language: "German" } }
		});
		render(<I18nProvider culture="en-US"><App /></I18nProvider>);
		await userEvent.click(screen.getByRole("button", { name: "Settings" }));

		// Act
		await userEvent.selectOptions(await screen.findByRole("combobox"), "de-DE");

		// Assert
		expect(screen.getByRole("button", { name: "Neuer Verkauf" })).toBeInTheDocument();
	});

	it("ToggleMenu_Collapsed_KeepsMenuUsable", async () =>
	{
		// Arrange
		mockApi({ "GET /api/stock-items": { body: [] }, "GET /api/customers": { body: [] } });
		render(<I18nProvider culture="en-US"><App /></I18nProvider>);

		// Act
		await userEvent.click(screen.getByRole("button", { name: "Menu" }));
		await userEvent.click(screen.getByRole("button", { name: "Clients" }));

		// Assert
		expect(screen.queryByText("Clients", { selector: "span" })).not.toBeInTheDocument();
		expect(screen.getByRole("heading", { name: "Clients" })).toBeInTheDocument();
	});
});
