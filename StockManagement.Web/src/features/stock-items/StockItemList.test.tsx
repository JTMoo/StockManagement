import { screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { describe, expect, it, vi } from "vitest";
import { mockApi, nut, renderEnglish, screw, sentBody } from "../../test-utils";
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

	it("Create_Valid_PostsAndAddsRow", async () =>
	{
		// Arrange
		const fetchMock = mockApi({ "GET /api/stock-items": { body: [] }, "POST /api/stock-items": { status: 201, body: screw } });
		renderEnglish(<StockItemList />);

		// Act
		await userEvent.type(screen.getByLabelText("Code"), "A1");
		await userEvent.type(screen.getByLabelText("Name"), "Screw");
		await userEvent.click(screen.getByRole("button", { name: "Create Stock item" }));

		// Assert
		expect(await screen.findByRole("cell", { name: "Screw" })).toBeInTheDocument();
		expect(sentBody(fetchMock, "POST /api/stock-items")).toEqual({ id: "", code: "A1", name: "Screw", description: "", location: "", amount: 0, price: 0, manufacturer: "" });
	});

	it("Create_DuplicateCode_ShowsError", async () =>
	{
		// Arrange
		mockApi({ "GET /api/stock-items": { body: [] }, "POST /api/stock-items": { status: 409, body: { code: "A1" } } });
		renderEnglish(<StockItemList />);

		// Act
		await userEvent.type(screen.getByLabelText("Code"), "A1");
		await userEvent.type(screen.getByLabelText("Name"), "Screw");
		await userEvent.click(screen.getByRole("button", { name: "Create Stock item" }));

		// Assert
		expect(await screen.findByRole("alert")).toHaveTextContent("A stock item with this code already exists: A1");
	});

	it("Edit_Valid_UpdatesRow", async () =>
	{
		// Arrange
		const fetchMock = mockApi({ "GET /api/stock-items": { body: [screw] }, "PUT /api/stock-items/1": { body: { ...screw, amount: 25 } } });
		renderEnglish(<StockItemList />);
		await screen.findByRole("cell", { name: "Screw" });

		// Act
		await userEvent.click(screen.getAllByRole("button", { name: "Edit" })[0]);
		await userEvent.clear(screen.getByLabelText("Amount"));
		await userEvent.type(screen.getByLabelText("Amount"), "25");
		await userEvent.click(screen.getByRole("button", { name: "Save" }));

		// Assert
		expect(await screen.findByRole("cell", { name: "25" })).toBeInTheDocument();
		expect(sentBody(fetchMock, "PUT /api/stock-items/1")).toMatchObject({ id: "1", code: "A1", amount: 25 });
		expect(screen.getByLabelText("Code")).toHaveValue("");
	});

	it("Delete_Confirmed_RemovesRow", async () =>
	{
		// Arrange
		const fetchMock = mockApi({ "GET /api/stock-items": { body: [screw, nut] }, "DELETE /api/stock-items/1": { status: 204 } });
		vi.spyOn(window, "confirm").mockReturnValue(true);
		renderEnglish(<StockItemList />);
		await screen.findByRole("cell", { name: "Screw" });

		// Act
		await userEvent.click(screen.getAllByRole("button", { name: "Delete Item" })[0]);

		// Assert
		expect(screen.queryByRole("cell", { name: "Screw" })).not.toBeInTheDocument();
		expect(screen.getByRole("cell", { name: "Nut" })).toBeInTheDocument();
		expect(fetchMock).toHaveBeenCalledWith("/api/stock-items/1", expect.objectContaining({ method: "DELETE" }));
	});

	it("Delete_Cancelled_KeepsRow", async () =>
	{
		// Arrange
		mockApi({ "GET /api/stock-items": { body: [screw] } });
		vi.spyOn(window, "confirm").mockReturnValue(false);
		renderEnglish(<StockItemList />);
		await screen.findByRole("cell", { name: "Screw" });

		// Act
		await userEvent.click(screen.getByRole("button", { name: "Delete Item" }));

		// Assert
		expect(screen.getByRole("cell", { name: "Screw" })).toBeInTheDocument();
	});
});
