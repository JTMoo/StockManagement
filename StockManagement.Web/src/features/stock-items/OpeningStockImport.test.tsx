import { screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { describe, expect, it } from "vitest";
import { mockApi, renderEnglish } from "../../test-utils";
import { OpeningStockImport } from "./OpeningStockImport";

const file = new File(["dummy"], "opening-stock.xlsx", { type: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" });

describe("OpeningStockImport", () =>
{
	it("Preview_Valid_SendsOpeningStockAsTheTarget", async () =>
	{
		// Arrange
		const previewed = { id: "batch-1", target: "OpeningStock", fileName: "opening-stock.xlsx", sheetName: "Sheet1", status: "Previewed", readyCount: 1, duplicateCount: 0, errorCount: 0, rows: [] };
		const fetchMock = mockApi({ "POST /api/import/batches": { body: previewed } });
		renderEnglish(<OpeningStockImport />);

		// Act
		await userEvent.upload(screen.getByLabelText("Choose file"), file);
		await userEvent.click(screen.getByRole("button", { name: "Preview" }));

		// Assert
		await screen.findByText("1");
		const [, init] = fetchMock.mock.calls.find(([url]) => url === "/api/import/batches")!;
		expect((init!.body as FormData).get("Target")).toBe("OpeningStock");
	});
});
