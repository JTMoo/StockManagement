import { screen, within } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { describe, expect, it, vi } from "vitest";
import { ana, invoice, mockApi, nut, renderEnglish, screw, sentBody, type Route } from "../../test-utils";
import { SaleForm } from "./SaleForm";

async function renderSaleForm(sale: Route = { status: 201, body: invoice })
{
	const fetchMock = mockApi({ "GET /api/customers": { body: [ana] }, "GET /api/stock-items": { body: [screw, nut] }, "POST /api/sales": sale });
	const onSold = vi.fn();
	renderEnglish(<SaleForm onSold={onSold} />);
	await screen.findByRole("option", { name: "1001 Ana Gómez" });
	return { fetchMock, onSold };
}

async function addToCart(code: string, amount: number)
{
	await userEvent.selectOptions(screen.getByLabelText("Stock item"), code);
	await userEvent.clear(screen.getByLabelText("Quantity"));
	await userEvent.type(screen.getByLabelText("Quantity"), String(amount));
	await userEvent.click(screen.getByRole("button", { name: "Add to Shopping Cart" }));
}

describe("SaleForm", () =>
{
	it("Load_SoldOutItem_NotOffered", async () =>
	{
		// Arrange / Act
		await renderSaleForm();

		// Assert
		expect(screen.getByRole("option", { name: "A1 Screw (10)" })).toBeInTheDocument();
		expect(screen.queryByRole("option", { name: /Nut/ })).not.toBeInTheDocument();
	});

	it("AddToCart_SameItemTwice_MergesLineAndTotals", async () =>
	{
		// Arrange
		await renderSaleForm();

		// Act
		await addToCart("A1", 2);
		await addToCart("A1", 1);

		// Assert
		const cart = screen.getByRole("table", { name: "Shopping Cart" });
		expect(within(cart).getAllByRole("row")).toHaveLength(3);
		expect(within(cart).getByRole("row", { name: /Total/ })).toHaveTextContent("15,000");
	});

	it("Sell_Accepted_PostsSaleAndReportsInvoice", async () =>
	{
		// Arrange
		const { fetchMock, onSold } = await renderSaleForm();
		await userEvent.selectOptions(screen.getByLabelText("Customer"), "1001");
		await userEvent.selectOptions(screen.getByLabelText("Sale condition"), "Credit");
		await addToCart("A1", 2);

		// Act
		await userEvent.click(screen.getByRole("button", { name: "Sell" }));

		// Assert
		expect(sentBody(fetchMock, "POST /api/sales")).toEqual({ customerId: 1001, saleCondition: "Credit", items: [{ code: "A1", amount: 2 }] });
		expect(onSold).toHaveBeenCalledWith(invoice);
	});

	it("Sell_NotEnoughStock_ShowsUnavailableItems", async () =>
	{
		// Arrange
		const { onSold } = await renderSaleForm({ status: 409, body: { unavailableItems: ["Screw"] } });
		await userEvent.selectOptions(screen.getByLabelText("Customer"), "1001");
		await addToCart("A1", 11);

		// Act
		await userEvent.click(screen.getByRole("button", { name: "Sell" }));

		// Assert
		expect(await screen.findByRole("alert")).toHaveTextContent("Not enough stock: Screw");
		expect(onSold).not.toHaveBeenCalled();
	});

	it("Sell_EmptyCartOrNoCustomer_Disabled", async () =>
	{
		// Arrange / Act
		await renderSaleForm();

		// Assert
		expect(screen.getByRole("button", { name: "Sell" })).toBeDisabled();
	});

	it("RemoveLine_OnlyLine_EmptiesCart", async () =>
	{
		// Arrange
		await renderSaleForm();
		await addToCart("A1", 2);

		// Act
		await userEvent.click(screen.getByRole("button", { name: "Remove" }));

		// Assert
		expect(within(screen.getByRole("table", { name: "Shopping Cart" })).queryByRole("cell", { name: "Screw" })).not.toBeInTheDocument();
	});
});
