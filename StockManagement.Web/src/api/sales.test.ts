import { describe, expect, it } from "vitest";
import { mockApi } from "../test-utils";
import { salesApi } from "./sales";

describe("salesApi", () =>
{
	it("createSale_409_ReturnsUnavailableItems", async () =>
	{
		// Arrange
		mockApi({ "POST /api/sales": { status: 409, body: { unavailableItems: ["Screw"] } } });

		// Act
		const result = await salesApi.createSale({ customerId: 1001, saleCondition: "Cash", items: [{ code: "A1", amount: 99 }] });

		// Assert
		expect(result).toEqual({ ok: false, failure: { kind: "conflict", unavailableItems: ["Screw"] } });
	});

	it("createSale_400_ReturnsErrorReasons", async () =>
	{
		// Arrange
		mockApi({ "POST /api/sales": { status: 400, body: { errors: [{ name: "customerId", reason: "customerNotFound" }] } } });

		// Act
		const result = await salesApi.createSale({ customerId: 1, saleCondition: "Cash", items: [] });

		// Assert
		expect(result).toEqual({ ok: false, failure: { kind: "invalid", codes: ["customerNotFound"] } });
	});
});
