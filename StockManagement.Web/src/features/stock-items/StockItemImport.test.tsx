import { screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { describe, expect, it } from "vitest";
import { mockApi, renderEnglish } from "../../test-utils";
import { StockItemImport } from "./StockItemImport";

const file = new File(["dummy"], "stock.xlsx", { type: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" });

describe("StockItemImport", () =>
{
	it("Import_Valid_ShowsCounts", async () =>
	{
		// Arrange
		mockApi({ "POST /api/stock-items/import": { body: { sheetName: "Stock", imported: 2, duplicates: 1, errors: [] } } });
		renderEnglish(<StockItemImport />);

		// Act
		await userEvent.upload(screen.getByLabelText("Choose file"), file);
		await userEvent.click(screen.getByRole("button", { name: "Import" }));

		// Assert
		expect(await screen.findByText("2")).toBeInTheDocument();
		expect(screen.getByText("1")).toBeInTheDocument();
	});

	it("Import_RowErrors_ShowsErrorTable", async () =>
	{
		// Arrange
		mockApi({
			"POST /api/stock-items/import": {
				body: { sheetName: "Stock", imported: 1, duplicates: 0, errors: [{ row: 3, message: "Bad amount" }] }
			}
		});
		renderEnglish(<StockItemImport />);

		// Act
		await userEvent.upload(screen.getByLabelText("Choose file"), file);
		await userEvent.click(screen.getByRole("button", { name: "Import" }));

		// Assert
		expect(await screen.findByRole("cell", { name: "3" })).toBeInTheDocument();
		expect(screen.getByRole("cell", { name: "Bad amount" })).toBeInTheDocument();
	});

	it("NoFileChosen_ImportButtonDisabled", () =>
	{
		// Arrange
		mockApi({});

		// Act
		renderEnglish(<StockItemImport />);

		// Assert
		expect(screen.getByRole("button", { name: "Import" })).toBeDisabled();
	});

	it("Import_ApiFails_ShowsError", async () =>
	{
		// Arrange
		mockApi({});
		renderEnglish(<StockItemImport />);

		// Act
		await userEvent.upload(screen.getByLabelText("Choose file"), file);
		await userEvent.click(screen.getByRole("button", { name: "Import" }));

		// Assert
		expect(await screen.findByRole("alert")).toHaveTextContent("An unexpected error occured");
	});
});
