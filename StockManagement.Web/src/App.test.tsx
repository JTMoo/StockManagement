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
		mockApi({ "GET /api/stock-items": { body: [] } });
		render(<I18nProvider culture="en-US"><App /></I18nProvider>);

		// Act
		await userEvent.selectOptions(screen.getByRole("combobox"), "de-DE");

		// Assert
		expect(screen.getByRole("button", { name: "Neuer Verkauf" })).toBeInTheDocument();
	});
});
