import { screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { describe, expect, it } from "vitest";
import { invoice, mockApi, renderEnglish } from "../../test-utils";
import { InvoiceView } from "./InvoiceView";
import type { Invoice } from "../../api";

describe("InvoiceView", () =>
{
	it("Render_WithInvoice_ShowsLinesAndTotal", () =>
	{
		// Arrange / Act
		renderEnglish(<InvoiceView invoice={invoice as Invoice} />);

		// Assert
		expect(screen.getByRole("cell", { name: "Screw" })).toBeInTheDocument();
		expect(screen.getByTestId("invoice-total")).toHaveTextContent("10,000");
		expect(screen.getByRole("heading", { name: "Invoice 7" })).toBeInTheDocument();
		expect(screen.getByText("Cash")).toBeInTheDocument();
	});

	it("Search_UnknownNumber_ShowsInvoiceNotFound", async () =>
	{
		// Arrange
		mockApi({ "GET /api/invoices/99": { status: 404 } });
		renderEnglish(<InvoiceView />);

		// Act
		await userEvent.type(screen.getByLabelText("Invoice ID"), "99");
		await userEvent.click(screen.getByRole("button", { name: "Show" }));

		// Assert
		expect(await screen.findByRole("alert")).toHaveTextContent("Invoice not found.");
	});

	it("Search_KnownNumber_ShowsInvoice", async () =>
	{
		// Arrange
		mockApi({ "GET /api/invoices/7": { body: invoice } });
		renderEnglish(<InvoiceView />);

		// Act
		await userEvent.type(screen.getByLabelText("Invoice ID"), "7");
		await userEvent.click(screen.getByRole("button", { name: "Show" }));

		// Assert
		expect(await screen.findByRole("article", { name: "Invoice 7" })).toBeInTheDocument();
	});
});
