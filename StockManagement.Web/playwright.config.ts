import { defineConfig } from "@playwright/test";

// Runs the built app through the real API on its own database. Needs MongoDB on 127.0.0.1:27017.
export const mongoUrl = "mongodb://127.0.0.1:27017";
export const databaseName = "StockManagementE2E";
const port = 5090;

export default defineConfig({
	testDir: "e2e",
	globalSetup: "./e2e/global-setup.ts",
	use: { baseURL: `http://localhost:${port}` },
	webServer: {
		command: "dotnet run --project ../StockManagement.Api --no-launch-profile",
		url: `http://localhost:${port}/api/stock-items`,
		timeout: 180_000,
		env: { ASPNETCORE_URLS: `http://localhost:${port}`, ConnectionStrings__Mongo: mongoUrl, Mongo__DatabaseName: databaseName }
	}
});
