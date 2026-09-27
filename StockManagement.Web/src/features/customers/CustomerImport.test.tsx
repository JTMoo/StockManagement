import { screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { describe, expect, it } from "vitest";
import { mockApi, renderEnglish } from "../../test-utils";
import { CustomerImport } from "./CustomerImport";

const file = new File(["dummy"], "customers.xlsx", { type: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" });

describe("CustomerImport", () =>
{
	it("Preview_Valid_SendsCustomersAsTheTarget", async () =>
	{
		// Arrange
		const previewed = { id: "batch-1", target: "Customers", fileName: "customers.xlsx", sheetName: "Sheet1", status: "Previewed", readyCount: 1, duplicateCount: 0, errorCount: 0, rows: [] };
		const fetchMock = mockApi({ "POST /api/import/batches": { body: previewed } });
		renderEnglish(<CustomerImport />);

		// Act
		await userEvent.upload(screen.getByLabelText("Choose file"), file);
		await userEvent.click(screen.getByRole("button", { name: "Preview" }));

		// Assert
		await screen.findByText("1");
		const [, init] = fetchMock.mock.calls.find(([url]) => url === "/api/import/batches")!;
		expect((init!.body as FormData).get("Target")).toBe("Customers");
	});
});
