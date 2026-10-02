import { screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { describe, expect, it } from "vitest";
import type { Invoice } from "../../api";
import { invoice, mockApi, renderEnglish } from "../../test-utils";
import { InvoiceBrowser } from "./InvoiceBrowser";

describe("InvoiceBrowser", () =>
{
	it("NoInvoice_ShowsList", async () =>
	{
		// Arrange
		mockApi({ "GET /api/invoices?pageSize=20": { body: { items: [invoice], nextCursor: null } } });
		renderEnglish(<InvoiceBrowser />);

		// Assert
		expect(await screen.findByRole("cell", { name: "Ana Gómez" })).toBeInTheDocument();
	});

	it("SelectRowThenBack_ShowsDetailThenListAgain", async () =>
	{
		// Arrange
		mockApi({ "GET /api/invoices?pageSize=20": { body: { items: [invoice], nextCursor: null } } });
		renderEnglish(<InvoiceBrowser />);
		await userEvent.click(await screen.findByRole("button", { name: "001-001-0000007" }));

		// Assert: detail
		expect(await screen.findByRole("article", { name: "Invoice 001-001-0000007" })).toBeInTheDocument();

		// Act: back
		await userEvent.click(screen.getByRole("button", { name: "Back" }));

		// Assert: list again
		expect(await screen.findByRole("cell", { name: "Ana Gómez" })).toBeInTheDocument();
	});

	it("InitialInvoiceProp_ShowsThatInvoiceDirectly", () =>
	{
		// Arrange / Act
		renderEnglish(<InvoiceBrowser invoice={invoice as Invoice} />);

		// Assert
		expect(screen.getByRole("article", { name: "Invoice 001-001-0000007" })).toBeInTheDocument();
	});
});
