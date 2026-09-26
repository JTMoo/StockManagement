import { screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { describe, expect, it } from "vitest";
import { App } from "./App";
import { mockApi, renderEnglish } from "./test-utils";

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
		renderEnglish(<App />);
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
		renderEnglish(<App />);

		// Act
		await userEvent.click(screen.getByRole("button", { name: "Menu" }));
		await userEvent.click(screen.getByRole("button", { name: "Clients" }));

		// Assert
		expect(screen.queryByText("Clients", { selector: "span" })).not.toBeInTheDocument();
		expect(screen.getByRole("heading", { name: "Clients" })).toBeInTheDocument();
	});

	it("NotLoggedIn_ShowsLoginScreenInsteadOfShell", () =>
	{
		// Arrange + Act
		renderEnglish(<App />, { authenticated: false });

		// Assert
		expect(screen.getByRole("heading", { name: "User login" })).toBeInTheDocument();
		expect(screen.queryByRole("button", { name: "Settings" })).not.toBeInTheDocument();
	});

	it("WithoutUsersManage_HidesUsersNavItem", () =>
	{
		// Arrange + Act
		mockApi({ "GET /api/stock-items": { body: [] } });
		renderEnglish(<App />, { permissions: ["StockItems.Read"] });

		// Assert
		expect(screen.queryByRole("button", { name: "Users" })).not.toBeInTheDocument();
	});
});
