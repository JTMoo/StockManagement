import { screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { describe, expect, it, vi } from "vitest";
import { invoice, mockApi, renderEnglish } from "../../test-utils";
import { InvoiceList } from "./InvoiceList";

const listBody = { items: [invoice], totalCount: 1 };

describe("InvoiceList", () =>
{
	it("Render_Loaded_ShowsRowWithCustomerAndTotal", async () =>
	{
		// Arrange
		mockApi({ "GET /api/invoices?page=1&pageSize=20": { body: listBody } });
		renderEnglish(<InvoiceList onSelect={vi.fn()} />);

		// Assert
		expect(await screen.findByRole("cell", { name: "Ana Gómez" })).toBeInTheDocument();
		expect(screen.getByRole("button", { name: "7" })).toBeInTheDocument();
	});

	it("ClickInvoiceNumber_CallsOnSelectWithThatInvoice", async () =>
	{
		// Arrange
		mockApi({ "GET /api/invoices?page=1&pageSize=20": { body: listBody } });
		const onSelect = vi.fn();
		renderEnglish(<InvoiceList onSelect={onSelect} />);
		await screen.findByRole("button", { name: "7" });

		// Act
		await userEvent.click(screen.getByRole("button", { name: "7" }));

		// Assert
		expect(onSelect).toHaveBeenCalledWith(invoice);
	});

	it("SetCustomerId_RequestsFilteredPageOne_AndResetsPage", async () =>
	{
		// Arrange
		const fetchMock = mockApi({
			"GET /api/invoices?page=1&pageSize=20": { body: listBody },
			"GET /api/invoices?page=1&pageSize=20&customerId=1001": { body: listBody }
		});
		renderEnglish(<InvoiceList onSelect={vi.fn()} />);
		await screen.findByRole("button", { name: "7" });

		// Act
		await userEvent.type(screen.getByLabelText("Customer ID"), "1001");

		// Assert
		expect(fetchMock).toHaveBeenCalledWith("/api/invoices?page=1&pageSize=20&customerId=1001", expect.anything());
	});

	it("NextPage_Disabled_WhenNoFurtherResults", async () =>
	{
		// Arrange
		mockApi({ "GET /api/invoices?page=1&pageSize=20": { body: listBody } });
		renderEnglish(<InvoiceList onSelect={vi.fn()} />);

		// Assert
		expect(await screen.findByRole("button", { name: "Next" })).toBeDisabled();
		expect(screen.getByRole("button", { name: "Previous" })).toBeDisabled();
	});
});
