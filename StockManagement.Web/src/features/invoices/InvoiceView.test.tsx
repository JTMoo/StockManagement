import { screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { describe, expect, it } from "vitest";
import { invoice, mockApi, renderEnglish, sentBody } from "../../test-utils";
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
		expect(screen.getByRole("heading", { name: "Invoice 001-001-0000007" })).toBeInTheDocument();
		expect(screen.getByText("Cash")).toBeInTheDocument();
		expect(screen.getByText("Paid")).toBeInTheDocument();
	});

	it("Search_UnknownNumber_ShowsInvoiceNotFound", async () =>
	{
		// Arrange
		mockApi({ "GET /api/invoices/001-001-0000099": { status: 404 } });
		renderEnglish(<InvoiceView />);

		// Act
		await userEvent.type(screen.getByLabelText("Invoice ID"), "001-001-0000099");
		await userEvent.click(screen.getByRole("button", { name: "Show" }));

		// Assert
		expect(await screen.findByRole("alert")).toHaveTextContent("Invoice not found.");
	});

	it("Search_KnownNumber_ShowsInvoice", async () =>
	{
		// Arrange
		mockApi({ "GET /api/invoices/001-001-0000007": { body: invoice } });
		renderEnglish(<InvoiceView />);

		// Act
		await userEvent.type(screen.getByLabelText("Invoice ID"), "001-001-0000007");
		await userEvent.click(screen.getByRole("button", { name: "Show" }));

		// Assert
		expect(await screen.findByRole("article", { name: "Invoice 001-001-0000007" })).toBeInTheDocument();
	});

	it("CancelInvoice_ReasonGiven_ShowsCancelledAndHidesForm", async () =>
	{
		// Arrange
		const fetchMock = mockApi({ "POST /api/invoices/001-001-0000007/cancel": { body: { number: 1, date: "2026-09-27T10:00:00", reason: "Customer returned the goods", total: 10000, tax: 909, invoiceNumber: "001-001-0000007" } } });
		renderEnglish(<InvoiceView invoice={invoice as Invoice} />);
		await userEvent.click(screen.getByRole("button", { name: "Cancel invoice" }));
		await userEvent.type(screen.getByLabelText("Reason"), "Customer returned the goods");

		// Act
		await userEvent.click(screen.getByRole("button", { name: "Cancel invoice" }));

		// Assert
		expect(await screen.findByTestId("invoice-cancelled")).toHaveTextContent("Cancelled");
		expect(screen.queryByLabelText("Reason")).not.toBeInTheDocument();
		expect(sentBody(fetchMock, "POST /api/invoices/001-001-0000007/cancel")).toEqual({ number: "001-001-0000007", reason: "Customer returned the goods" });
	});

	it("Render_AlreadyCancelledInvoice_HidesCancelButton", () =>
	{
		// Arrange / Act
		renderEnglish(<InvoiceView invoice={{ ...invoice, isCancelled: true } as Invoice} />);

		// Assert
		expect(screen.getByTestId("invoice-cancelled")).toHaveTextContent("Cancelled");
		expect(screen.queryByRole("button", { name: "Cancel invoice" })).not.toBeInTheDocument();
	});
});
