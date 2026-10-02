import { screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { describe, expect, it } from "vitest";
import { acme, mockApi, renderEnglish, sentBody } from "../../test-utils";
import { SupplierList } from "./SupplierList";

describe("SupplierList", () =>
{
	it("Load_OneSupplier_ShowsIt", async () =>
	{
		// Arrange
		mockApi({ "GET /api/suppliers?pageSize=100": { body: { items: [acme], nextCursor: null } } });

		// Act
		renderEnglish(<SupplierList />);

		// Assert
		expect(await screen.findByRole("cell", { name: "Acme" })).toBeInTheDocument();
	});

	it("Create_Valid_PostsAndAddsRow", async () =>
	{
		// Arrange
		const fetchMock = mockApi({ "GET /api/suppliers?pageSize=100": { body: { items: [], nextCursor: null } }, "POST /api/suppliers": { status: 201, body: acme } });
		renderEnglish(<SupplierList />);

		// Act
		await userEvent.type(screen.getByLabelText("Name"), "Acme");
		await userEvent.click(screen.getByRole("button", { name: "Create Supplier" }));

		// Assert
		expect(await screen.findByRole("cell", { name: "Acme" })).toBeInTheDocument();
		expect(sentBody(fetchMock, "POST /api/suppliers")).toEqual({ id: "", name: "Acme", contactName: "", country: "", currency: "", leadTimeDays: 0, miscellaneous: "" });
	});

	it("Create_DuplicateName_ShowsError", async () =>
	{
		// Arrange
		mockApi({ "GET /api/suppliers?pageSize=100": { body: { items: [], nextCursor: null } }, "POST /api/suppliers": { status: 409, body: { name: "Acme" } } });
		renderEnglish(<SupplierList />);

		// Act
		await userEvent.type(screen.getByLabelText("Name"), "Acme");
		await userEvent.click(screen.getByRole("button", { name: "Create Supplier" }));

		// Assert
		expect(await screen.findByRole("alert")).toHaveTextContent("A supplier with this name already exists: Acme");
	});

	it("Delete_Confirmed_RemovesRow", async () =>
	{
		// Arrange
		const fetchMock = mockApi({ "GET /api/suppliers?pageSize=100": { body: { items: [acme], nextCursor: null } }, "DELETE /api/suppliers/1": { status: 204 } });
		renderEnglish(<SupplierList />);
		await screen.findByRole("cell", { name: "Acme" });

		// Act
		await userEvent.click(screen.getByRole("button", { name: "Delete Supplier" }));
		await userEvent.click(screen.getByRole("button", { name: "Confirm" }));

		// Assert
		expect(screen.queryByRole("cell", { name: "Acme" })).not.toBeInTheDocument();
		expect(fetchMock).toHaveBeenCalledWith("/api/suppliers/1", expect.objectContaining({ method: "DELETE" }));
	});

	it("Delete_InUse_ShowsError", async () =>
	{
		// Arrange
		mockApi({ "GET /api/suppliers?pageSize=100": { body: { items: [acme], nextCursor: null } }, "DELETE /api/suppliers/1": { status: 409, body: { reason: "Supplier is assigned to stock items and can't be deleted." } } });
		renderEnglish(<SupplierList />);
		await screen.findByRole("cell", { name: "Acme" });

		// Act
		await userEvent.click(screen.getByRole("button", { name: "Delete Supplier" }));
		await userEvent.click(screen.getByRole("button", { name: "Confirm" }));

		// Assert
		expect(await screen.findByRole("alert")).toHaveTextContent("Supplier is assigned to stock items and can't be deleted.");
		expect(screen.getByRole("cell", { name: "Acme" })).toBeInTheDocument();
	});
});
