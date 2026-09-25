import { act, renderHook, waitFor } from "@testing-library/react";
import { describe, expect, it } from "vitest";
import { api } from "./api";
import { AuthProvider, useAuth } from "./auth";
import { mockApi } from "./test-utils";

describe("AuthProvider", () =>
{
	it("Login_CorrectCredentials_SetsUsernameAndToken", async () =>
	{
		// Arrange
		mockApi({ "POST /api/auth/login": { body: { token: "the-token", username: "admin" } } });
		const { result } = renderHook(() => useAuth(), { wrapper: AuthProvider });

		// Act
		await act(() => result.current.login("admin", "ChangeMe123!"));

		// Assert
		expect(result.current.username).toBe("admin");
		expect(localStorage.getItem("auth.token")).toBe("the-token");
	});

	it("Logout_AfterLogin_ClearsUsernameAndToken", async () =>
	{
		// Arrange
		mockApi({ "POST /api/auth/login": { body: { token: "the-token", username: "admin" } } });
		const { result } = renderHook(() => useAuth(), { wrapper: AuthProvider });
		await act(() => result.current.login("admin", "ChangeMe123!"));

		// Act
		act(() => result.current.logout());

		// Assert
		expect(result.current.username).toBeNull();
		expect(localStorage.getItem("auth.token")).toBeNull();
	});

	it("ExistingSession_Unauthorized_LogsOut", async () =>
	{
		// Arrange
		localStorage.setItem("auth.token", "stale-token");
		localStorage.setItem("auth.username", "admin");
		mockApi({ "GET /api/stock-items": { status: 401 } });
		const { result } = renderHook(() => useAuth(), { wrapper: AuthProvider });
		expect(result.current.username).toBe("admin");

		// Act: any 401 response (not just from login) triggers the shared handler
		await act(() => api.listStockItems());

		// Assert
		await waitFor(() => expect(result.current.username).toBeNull());
	});
});
