import { screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { describe, expect, it } from "vitest";
import { mockApi, renderEnglish } from "../../test-utils";
import { OpenInvoiceImport } from "./OpenInvoiceImport";

const file = new File(["dummy"], "open-invoices.xlsx", { type: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" });

describe("OpenInvoiceImport", () =>
{
	it("Preview_Valid_SendsOpenInvoicesAsTheTarget", async () =>
	{
		// Arrange
		const detected = { sheetName: "Sheet1", columns: [], fields: [] };
		const previewed = { id: "batch-1", target: "OpenInvoices", fileName: "open-invoices.xlsx", sheetName: "Sheet1", status: "Previewed", readyCount: 1, duplicateCount: 0, errorCount: 0, rows: [] };
		const fetchMock = mockApi({ "POST /api/import/batches/columns": { body: detected }, "POST /api/import/batches": { body: previewed } });
		renderEnglish(<OpenInvoiceImport />);

		// Act
		await userEvent.upload(screen.getByLabelText("Choose file"), file);
		await userEvent.click(screen.getByRole("button", { name: "Preview" }));
		await userEvent.click(await screen.findByRole("button", { name: "Preview" }));

		// Assert
		await screen.findByText("1");
		const [, init] = fetchMock.mock.calls.find(([url]) => url === "/api/import/batches")!;
		expect((init!.body as FormData).get("Target")).toBe("OpenInvoices");
	});
});
