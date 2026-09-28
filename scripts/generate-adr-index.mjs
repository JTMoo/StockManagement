#!/usr/bin/env node
// Regenerates the ADR table in docs/decisions.md from docs/adr/*.md (source of truth = the files).
// Run without args to write; `--check` exits 1 if the file would change (used in CI).

import { readFileSync, readdirSync, writeFileSync } from "node:fs";
import { dirname, join } from "node:path";
import { fileURLToPath } from "node:url";

const root = join(dirname(fileURLToPath(import.meta.url)), "..");
const adrDir = join(root, "docs", "adr");
const decisionsPath = join(root, "docs", "decisions.md");

const START = "<!-- adr-table:start -->";
const END = "<!-- adr-table:end -->";

function parseAdr(fileName)
{
	const number = fileName.match(/^(\d{4})-/)?.[1];
	if (!number) return null;

	const text = readFileSync(join(adrDir, fileName), "utf8");
	const title = text.match(/^# ADR-\d{4}: (.+)$/m)?.[1] ?? fileName;
	const status = text.match(/^- Status: (.+)$/m)?.[1] ?? "Unknown";

	return { number, title, status, fileName };
}

const adrs = readdirSync(adrDir)
	.filter(name => /^\d{4}-.*\.md$/.test(name))
	.map(parseAdr)
	.filter(Boolean)
	.sort((a, b) => a.number.localeCompare(b.number));

const rows = adrs.map(a => `| [${a.number}](adr/${a.fileName}) | ${a.title} | ${a.status} |`).join("\n");
const table = `${START}\n| ADR | Decision | Status |\n|---|---|---|\n${rows}\n${END}`;

const current = readFileSync(decisionsPath, "utf8");
const next = current.replace(new RegExp(`${START}[\\s\\S]*?${END}`), table);

if (process.argv.includes("--check"))
{
	if (current !== next)
	{
		console.error("docs/decisions.md ADR table is stale. Run: node scripts/generate-adr-index.mjs");
		process.exit(1);
	}
	console.log("ADR table is up to date.");
}
else
{
	writeFileSync(decisionsPath, next);
	console.log(`Wrote ${adrs.length} ADRs to docs/decisions.md`);
}
