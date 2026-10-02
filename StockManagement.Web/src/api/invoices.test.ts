import { describe, expect, it } from "vitest";
import { invoice, mockApi } from "../test-utils";
import { invoicesApi } from "./invoices";

describe("invoicesApi", () =>
{
	it("getInvoice_Ok_ReturnsValue", async () =>
	{
		// Arrange
		mockApi({ "GET /api/invoices/001-001-0000007": { body: invoice } });

		// Act
		const result = await invoicesApi.getInvoice("001-001-0000007");

		// Assert
		expect(result).toEqual({ ok: true, value: invoice });
	});

	it("getInvoice_404_ReturnsNotFound", async () =>
	{
		// Arrange
		mockApi({ "GET /api/invoices/001-001-0000007": { status: 404 } });

		// Act
		const result = await invoicesApi.getInvoice("001-001-0000007");

		// Assert
		expect(result).toEqual({ ok: false, failure: { kind: "notFound" } });
	});
});
