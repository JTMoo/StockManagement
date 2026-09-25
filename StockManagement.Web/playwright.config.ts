import { defineConfig } from "@playwright/test";

// Runs the built app through the real API on its own database. Needs PostgreSQL on 127.0.0.1:5432 (ADR-0008).
export const postgres = { host: "127.0.0.1", port: 5432, database: "StockManagementE2E", user: "postgres", password: "postgres" };
const port = 5090;

export default defineConfig({
	testDir: "e2e",
	globalSetup: "./e2e/global-setup.ts",
	use: { baseURL: `http://localhost:${port}` },
	webServer: {
		command: "dotnet run --project ../StockManagement.Api --no-launch-profile",
		// Not an API route: every API endpoint now requires auth (ADR-0010), so this would never see a 2xx
		url: `http://localhost:${port}/`,
		timeout: 180_000,
		env: {
			ASPNETCORE_URLS: `http://localhost:${port}`,
			ConnectionStrings__Postgres: `Host=${postgres.host};Port=${postgres.port};Database=${postgres.database};Username=${postgres.user};Password=${postgres.password}`
		}
	}
});
