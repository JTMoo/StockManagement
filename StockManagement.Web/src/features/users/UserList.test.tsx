import { screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { describe, expect, it } from "vitest";
import { mockApi, renderEnglish, sentBody } from "../../test-utils";
import { UserList } from "./UserList";

const ana = { id: "u1", username: "ana", fullName: "Ana Gómez", email: "", phone: "", position: "", role: "Standard" as const, permissions: [] };

describe("UserList", () =>
{
	it("Create_Valid_PostsAndAddsRow", async () =>
	{
		// Arrange
		const fetchMock = mockApi({ "GET /api/users": { body: [] }, "POST /api/users": { status: 201, body: ana } });
		renderEnglish(<UserList />);

		// Act
		await userEvent.type(screen.getByLabelText("Username"), "ana");
		await userEvent.type(screen.getByLabelText("Password"), "s3cret!23");
		await userEvent.click(screen.getByRole("button", { name: "Create User" }));

		// Assert
		expect(await screen.findByRole("cell", { name: "ana" })).toBeInTheDocument();
		expect(sentBody(fetchMock, "POST /api/users")).toMatchObject({ username: "ana", password: "s3cret!23", role: "Standard" });
	});

	it("Create_DuplicateUsername_ShowsError", async () =>
	{
		// Arrange
		mockApi({ "GET /api/users": { body: [] }, "POST /api/users": { status: 409, body: { code: "ana" } } });
		renderEnglish(<UserList />);

		// Act
		await userEvent.type(screen.getByLabelText("Username"), "ana");
		await userEvent.type(screen.getByLabelText("Password"), "s3cret!23");
		await userEvent.click(screen.getByRole("button", { name: "Create User" }));

		// Assert
		expect(await screen.findByRole("alert")).toHaveTextContent("A user with this username already exists: ana");
	});

	it("SelectAdminRole_HidesPermissionCheckboxes", async () =>
	{
		// Arrange
		mockApi({ "GET /api/users": { body: [] } });
		renderEnglish(<UserList />);

		// Act
		await userEvent.selectOptions(screen.getByLabelText("Role"), "Admin");

		// Assert
		expect(screen.queryByText("View clients")).not.toBeInTheDocument();
	});

	it("DeleteUser_NotSelf_RemovesRow", async () =>
	{
		// Arrange
		mockApi({ "GET /api/users": { body: [ana] }, "DELETE /api/users/u1": { status: 204 } });
		renderEnglish(<UserList />);
		await screen.findByRole("cell", { name: "ana" });

		// Act
		await userEvent.click(screen.getByRole("button", { name: "Delete User" }));

		// Assert
		expect(screen.queryByRole("cell", { name: "ana" })).not.toBeInTheDocument();
	});

	it("OwnRow_HasNoDeleteButton", async () =>
	{
		// Arrange: renderEnglish logs in as "admin"
		const self = { ...ana, id: "u0", username: "admin" };
		mockApi({ "GET /api/users": { body: [self] } });
		renderEnglish(<UserList />);

		// Act
		await screen.findByRole("cell", { name: "admin" });

		// Assert
		expect(screen.queryByRole("button", { name: "Delete User" })).not.toBeInTheDocument();
	});
});
