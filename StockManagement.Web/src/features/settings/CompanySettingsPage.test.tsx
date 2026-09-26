import { screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { describe, expect, it } from "vitest";
import { mockApi, renderEnglish, sentBody } from "../../test-utils";
import { CompanySettingsPage } from "./CompanySettingsPage";

const companySettings = { companyName: "Acme", taxId: "123456", currency: "PYG", vatRatePercent: 10, paymentTermInDays: 30, firstInvoiceNumber: 1, firstCustomerId: 1001 };

describe("CompanySettingsPage", () =>
{
	it("Load_Stored_ShowsFields", async () =>
	{
		// Arrange
		mockApi({ "GET /api/company-settings": { body: companySettings } });
		renderEnglish(<CompanySettingsPage />);

		// Assert
		expect(await screen.findByDisplayValue("Acme")).toBeInTheDocument();
		expect(screen.getByDisplayValue("123456")).toBeInTheDocument();
	});

	it("Submit_ChangedVatRate_Puts", async () =>
	{
		// Arrange
		const fetchMock = mockApi({
			"GET /api/company-settings": { body: companySettings },
			"PUT /api/company-settings": { body: { ...companySettings, vatRatePercent: 5 } }
		});
		renderEnglish(<CompanySettingsPage />);
		await screen.findByDisplayValue("Acme");

		// Act
		await userEvent.clear(screen.getByLabelText("VAT rate (%)"));
		await userEvent.type(screen.getByLabelText("VAT rate (%)"), "5");
		await userEvent.click(screen.getByRole("button", { name: "Save" }));

		// Assert
		expect(sentBody(fetchMock, "PUT /api/company-settings")).toMatchObject({ vatRatePercent: 5 });
	});
});
