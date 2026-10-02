const { app, BrowserWindow, dialog } = require("electron");
const { spawn } = require("node:child_process");
const path = require("node:path");
const http = require("node:http");

const url = process.env.STOCKMANAGEMENT_URL || "http://localhost:5080";
const isPackaged = app.isPackaged;

let apiProcess = null;
let apiSpawnError = null;

function apiBinaryPath() {
	const exe = process.platform === "win32" ? "StockManagement.Api.exe" : "StockManagement.Api";
	return path.join(process.resourcesPath, "api", exe);
}

function startApi() {
	apiProcess = spawn(apiBinaryPath(), [], {
		cwd: path.dirname(apiBinaryPath()),
		stdio: "ignore",
	});
	apiProcess.on("error", (error) => {
		apiSpawnError = error;
		console.error("Failed to start bundled API:", error);
	});
}

function waitForApi(timeoutMs = 30000, intervalMs = 300) {
	const deadline = Date.now() + timeoutMs;
	return new Promise((resolve, reject) => {
		const attempt = () => {
			http
				.get(`${url}/health`, (response) => {
					response.resume();
					if (response.statusCode >= 200 && response.statusCode < 300) {
						resolve();
						return;
					}
					if (Date.now() > deadline) {
						reject(new Error(`Bundled API returned status ${response.statusCode}`));
						return;
					}
					setTimeout(attempt, intervalMs);
				})
				.on("error", () => {
					if (Date.now() > deadline) {
						reject(new Error("Timed out waiting for the bundled API to become ready"));
						return;
					}
					setTimeout(attempt, intervalMs);
				});
		};
		attempt();
	});
}

function createWindow() {
	const window = new BrowserWindow({ width: 1280, height: 800 });
	window.loadURL(url);
}

app.whenReady().then(async () => {
	if (isPackaged) {
		startApi();
		try {
			await waitForApi();
		} catch (error) {
			console.error(error);
			dialog.showErrorBox(
				"Kora failed to start",
				apiSpawnError
					? `The bundled API could not be started: ${apiSpawnError.message}`
					: `The bundled API did not become ready in time: ${error.message}`,
			);
			app.quit();
			return;
		}
	}
	createWindow();
});

app.on("before-quit", () => {
	apiProcess?.kill();
});

app.on("window-all-closed", () => {
	if (process.platform !== "darwin") app.quit();
});

app.on("activate", () => {
	if (BrowserWindow.getAllWindows().length === 0) createWindow();
});
