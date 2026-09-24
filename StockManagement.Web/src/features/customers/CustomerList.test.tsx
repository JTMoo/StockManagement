import { screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { describe, expect, it } from "vitest";
import { ana, mockApi, renderEnglish, sentBody } from "../../test-utils";
import { CustomerList } from "./CustomerList";

describe("CustomerList", () =>
{
	it("Create_Valid_PostsAndAddsRow", async () =>
	{
		// Arrange
		const fetchMock = mockApi({ "GET /api/customers": { body: [] }, "POST /api/customers": { status: 201, body: ana } });
		renderEnglish(<CustomerList />);

		// Act
		await userEvent.type(screen.getByLabelText("Name"), "Ana");
		await userEvent.type(screen.getByLabelText("Lastname"), "Gómez");
		await userEvent.click(screen.getByRole("button", { name: "Create Customer" }));

		// Assert
		expect(await screen.findByRole("cell", { name: "Gómez" })).toBeInTheDocument();
		expect(sentBody(fetchMock, "POST /api/customers")).toEqual({ name: "Ana", lastname: "Gómez" });
		expect(screen.getByLabelText("Name")).toHaveValue("");
	});

	it("Create_Rejected_ShowsErrorAndKeepsInput", async () =>
	{
		// Arrange
		mockApi({ "GET /api/customers": { body: [] }, "POST /api/customers": { status: 400, body: { errors: [{ name: "name", reason: "'name' must not be empty." }] } } });
		renderEnglish(<CustomerList />);

		// Act
		await userEvent.type(screen.getByLabelText("Name"), " ");
		await userEvent.click(screen.getByRole("button", { name: "Create Customer" }));

		// Assert
		expect(await screen.findByRole("alert")).toHaveTextContent("Please check your input.");
		expect(screen.getByLabelText("Name")).toHaveValue(" ");
	});

	it("Edit_Valid_PutsAndUpdatesRow", async () =>
	{
		// Arrange
		const updated = { ...ana, lastname: "Silva" };
		const fetchMock = mockApi({ "GET /api/customers": { body: [ana] }, "PUT /api/customers/1001": { body: updated } });
		renderEnglish(<CustomerList />);
		await userEvent.click(await screen.findByRole("button", { name: "Edit" }));

		// Act
		await userEvent.clear(screen.getByLabelText("Lastname"));
		await userEvent.type(screen.getByLabelText("Lastname"), "Silva");
		await userEvent.click(screen.getByRole("button", { name: "Save Customer" }));

		// Assert
		expect(await screen.findByRole("cell", { name: "Silva" })).toBeInTheDocument();
		expect(sentBody(fetchMock, "PUT /api/customers/1001")).toEqual(updated);
		expect(screen.queryByRole("button", { name: "Save Customer" })).not.toBeInTheDocument();
	});

	it("Edit_Cancelled_ShowsCreateFormAgainWithoutSaving", async () =>
	{
		// Arrange
		mockApi({ "GET /api/customers": { body: [ana] } });
		renderEnglish(<CustomerList />);
		await userEvent.click(await screen.findByRole("button", { name: "Edit" }));

		// Act
		await userEvent.click(screen.getByRole("button", { name: "Cancel" }));

		// Assert
		expect(screen.getByRole("button", { name: "Create Customer" })).toBeInTheDocument();
	});

	it("Edit_Rejected_ShowsError", async () =>
	{
		// Arrange
		mockApi({ "GET /api/customers": { body: [ana] }, "PUT /api/customers/1001": { status: 400, body: { errors: [{ name: "name", reason: "'name' must not be empty." }] } } });
		renderEnglish(<CustomerList />);
		await userEvent.click(await screen.findByRole("button", { name: "Edit" }));

		// Act
		await userEvent.click(screen.getByRole("button", { name: "Save Customer" }));

		// Assert
		expect(await screen.findByRole("alert")).toHaveTextContent("Please check your input.");
	});
});
