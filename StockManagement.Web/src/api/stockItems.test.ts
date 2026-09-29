import { describe, expect, it } from "vitest";
import { mockApi, screw } from "../test-utils";
import { stockItemsApi } from "./stockItems";

describe("stockItemsApi", () =>
{
	it("listStockItems_NetworkDown_ReturnsUnexpected", async () =>
	{
		// Arrange
		mockApi({});

		// Act
		const result = await stockItemsApi.listStockItems();

		// Assert
		expect(result).toEqual({ ok: false, failure: { kind: "unexpected" } });
	});

	it("createStockItem_409_ReturnsDuplicate", async () =>
	{
		// Arrange
		mockApi({ "POST /api/stock-items": { status: 409, body: { code: "A1" } } });

		// Act
		const result = await stockItemsApi.createStockItem({ code: "A1", name: "Screw" });

		// Assert
		expect(result).toEqual({ ok: false, failure: { kind: "duplicate", code: "A1" } });
	});

	it("deleteStockItem_204_ReturnsOk", async () =>
	{
		// Arrange
		mockApi({ "DELETE /api/stock-items/1": { status: 204 } });

		// Act
		const result = await stockItemsApi.deleteStockItem(screw);

		// Assert
		expect(result).toEqual({ ok: true, value: undefined });
	});

	it("deleteStockItem_404_ReturnsNotFound", async () =>
	{
		// Arrange
		mockApi({ "DELETE /api/stock-items/1": { status: 404 } });

		// Act
		const result = await stockItemsApi.deleteStockItem(screw);

		// Assert
		expect(result).toEqual({ ok: false, failure: { kind: "notFound" } });
	});
});
