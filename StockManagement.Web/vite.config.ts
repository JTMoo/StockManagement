import react from "@vitejs/plugin-react";
import { defineConfig } from "vitest/config";

// Dev: proxy /api to the local API. Build: the API serves the app from wwwroot (same origin, no CORS).
export default defineConfig({
	plugins: [react()],
	server: { proxy: { "/api": "http://localhost:5080" } },
	build: { outDir: "../StockManagement.Api/wwwroot", emptyOutDir: true },
	test: { environment: "jsdom", include: ["src/**/*.test.{ts,tsx}"], setupFiles: ["src/test-setup.ts"] }
});
