const { app, BrowserWindow } = require("electron");

const url = process.env.STOCKMANAGEMENT_URL || "http://localhost:5080";

function createWindow() {
	const window = new BrowserWindow({ width: 1280, height: 800 });
	window.loadURL(url);
}

app.whenReady().then(createWindow);

app.on("window-all-closed", () => {
	if (process.platform !== "darwin") app.quit();
});

app.on("activate", () => {
	if (BrowserWindow.getAllWindows().length === 0) createWindow();
});
