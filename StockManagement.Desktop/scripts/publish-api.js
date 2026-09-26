// Self-contained publish of StockManagement.Api for the host OS/arch, into
// StockManagement.Desktop/resources/api. Run before electron-builder (ADR-0014).
const { spawnSync } = require("node:child_process");
const path = require("node:path");

const RIDS = {
	"win32-x64": "win-x64",
	"darwin-x64": "osx-x64",
	"darwin-arm64": "osx-arm64",
	"linux-x64": "linux-x64",
};

const rid = process.env.STOCKMANAGEMENT_RID || RIDS[`${process.platform}-${process.arch}`];
if (!rid) {
	console.error(`No RID mapping for ${process.platform}-${process.arch}; set STOCKMANAGEMENT_RID.`);
	process.exit(1);
}

const apiProject = path.join(__dirname, "..", "..", "StockManagement.Api");
const outDir = path.join(__dirname, "..", "resources", "api");

const result = spawnSync(
	"dotnet",
	["publish", apiProject, "-c", "Release", "-r", rid, "--self-contained", "-p:PublishSingleFile=true", "-o", outDir],
	{ stdio: "inherit" },
);

process.exit(result.status ?? 1);
