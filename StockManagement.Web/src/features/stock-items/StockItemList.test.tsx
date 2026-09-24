import { screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { describe, expect, it } from "vitest";
import { mockApi, nut, renderEnglish, screw } from "../../test-utils";
import { StockItemList } from "./StockItemList";

describe("StockItemList", () =>
{
	it("Load_TwoItems_ShowsBoth", async () =>
	{
		// Arrange
		mockApi({ "GET /api/stock-items": { body: [screw, nut] } });

		// Act
		renderEnglish(<StockItemList />);

		// Assert
		expect(await screen.findByRole("cell", { name: "Screw" })).toBeInTheDocument();
		expect(screen.getByRole("cell", { name: "Nut" })).toBeInTheDocument();
	});

	it("Search_ByCode_ShowsOnlyMatches", async () =>
	{
		// Arrange
		mockApi({ "GET /api/stock-items": { body: [screw, nut] } });
		renderEnglish(<StockItemList />);
		await screen.findByRole("cell", { name: "Screw" });

		// Act
		await userEvent.type(screen.getByRole("searchbox"), "b2");

		// Assert
		expect(screen.queryByRole("cell", { name: "Screw" })).not.toBeInTheDocument();
		expect(screen.getByRole("cell", { name: "Nut" })).toBeInTheDocument();
	});

	it("Search_RegexCharacters_TreatedAsText", async () =>
	{
		// Arrange
		mockApi({ "GET /api/stock-items": { body: [screw] } });
		renderEnglish(<StockItemList />);
		await screen.findByRole("cell", { name: "Screw" });

		// Act
		await userEvent.type(screen.getByRole("searchbox"), "(");

		// Assert
		expect(screen.queryByRole("cell", { name: "Screw" })).not.toBeInTheDocument();
	});

	it("Load_ApiDown_ShowsError", async () =>
	{
		// Arrange
		mockApi({});

		// Act
		renderEnglish(<StockItemList />);

		// Assert
		expect(await screen.findByRole("alert")).toHaveTextContent("An unexpected error occured");
	});
});
