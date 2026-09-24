import { describe, expect, it } from "vitest";
import { api } from "./api";
import { invoice, mockApi, screw } from "./test-utils";

describe("api", () =>
{
	it("getInvoice_Ok_ReturnsValue", async () =>
	{
		// Arrange
		mockApi({ "GET /api/invoices/7": { body: invoice } });

		// Act
		const result = await api.getInvoice(7);

		// Assert
		expect(result).toEqual({ ok: true, value: invoice });
	});

	it("getInvoice_404_ReturnsNotFound", async () =>
	{
		// Arrange
		mockApi({ "GET /api/invoices/7": { status: 404 } });

		// Act
		const result = await api.getInvoice(7);

		// Assert
		expect(result).toEqual({ ok: false, failure: { kind: "notFound" } });
	});

	it("createSale_409_ReturnsUnavailableItems", async () =>
	{
		// Arrange
		mockApi({ "POST /api/sales": { status: 409, body: { unavailableItems: ["Screw"] } } });

		// Act
		const result = await api.createSale({ customerId: 1001, saleCondition: "Cash", items: [{ code: "A1", amount: 99 }] });

		// Assert
		expect(result).toEqual({ ok: false, failure: { kind: "conflict", unavailableItems: ["Screw"] } });
	});

	it("createSale_400_ReturnsErrorReasons", async () =>
	{
		// Arrange
		mockApi({ "POST /api/sales": { status: 400, body: { errors: [{ name: "customerId", reason: "customerNotFound" }] } } });

		// Act
		const result = await api.createSale({ customerId: 1, saleCondition: "Cash", items: [] });

		// Assert
		expect(result).toEqual({ ok: false, failure: { kind: "invalid", codes: ["customerNotFound"] } });
	});

	it("listStockItems_NetworkDown_ReturnsUnexpected", async () =>
	{
		// Arrange
		mockApi({});

		// Act
		const result = await api.listStockItems();

		// Assert
		expect(result).toEqual({ ok: false, failure: { kind: "unexpected" } });
	});

	it("createStockItem_409_ReturnsDuplicate", async () =>
	{
		// Arrange
		mockApi({ "POST /api/stock-items": { status: 409, body: { code: "A1" } } });

		// Act
		const result = await api.createStockItem({ code: "A1", name: "Screw" });

		// Assert
		expect(result).toEqual({ ok: false, failure: { kind: "duplicate", code: "A1" } });
	});

	it("deleteStockItem_204_ReturnsOk", async () =>
	{
		// Arrange
		mockApi({ "DELETE /api/stock-items/1": { status: 204 } });

		// Act
		const result = await api.deleteStockItem(screw);

		// Assert
		expect(result).toEqual({ ok: true, value: undefined });
	});

	it("deleteStockItem_404_ReturnsNotFound", async () =>
	{
		// Arrange
		mockApi({ "DELETE /api/stock-items/1": { status: 404 } });

		// Act
		const result = await api.deleteStockItem(screw);

		// Assert
		expect(result).toEqual({ ok: false, failure: { kind: "notFound" } });
	});
});
