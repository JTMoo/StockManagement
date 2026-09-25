import { screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { describe, expect, it } from "vitest";
import { mockApi, renderEnglish } from "../../test-utils";
import { LoginPage } from "./LoginPage";

describe("LoginPage", () =>
{
	it("Submit_CorrectCredentials_StoresToken", async () =>
	{
		// Arrange
		mockApi({ "POST /api/auth/login": { body: { token: "the-token", username: "admin" } } });
		renderEnglish(<LoginPage />, { authenticated: false });

		// Act
		await userEvent.type(screen.getByLabelText("Username"), "admin");
		await userEvent.type(screen.getByLabelText("Password"), "ChangeMe123!");
		await userEvent.click(screen.getByRole("button", { name: "User login" }));

		// Assert
		await waitFor(() => expect(localStorage.getItem("auth.token")).toBe("the-token"));
		expect(localStorage.getItem("auth.username")).toBe("admin");
	});

	it("Submit_WrongCredentials_ShowsFailureAndNoToken", async () =>
	{
		// Arrange
		mockApi({ "POST /api/auth/login": { status: 401 } });
		renderEnglish(<LoginPage />, { authenticated: false });

		// Act
		await userEvent.type(screen.getByLabelText("Username"), "admin");
		await userEvent.type(screen.getByLabelText("Password"), "wrong");
		await userEvent.click(screen.getByRole("button", { name: "User login" }));

		// Assert
		expect(await screen.findByRole("alert")).toHaveTextContent("Incorrect username or password.");
		expect(localStorage.getItem("auth.token")).toBeNull();
	});
});
