import { authHeaders, send, sendForm } from "./client";

export type ImportTarget = "StockItems" | "Customers" | "OpeningStock";

export type ImportRowStatus = "Ready" | "Duplicate" | "Error";

export type ImportBatchStatus = "Previewed" | "Committed" | "Undone";

export type ImportBatchRow = { row: number; status: ImportRowStatus; message?: string; fields?: Record<string, unknown> };

export type ImportBatch = {
	id: string;
	target: ImportTarget;
	fileName: string;
	sheetName: string;
	status: ImportBatchStatus;
	readyCount: number;
	duplicateCount: number;
	errorCount: number;
	rows: ImportBatchRow[];
};

export const importApi = {
	previewImport: (target: ImportTarget, file: File) => sendForm<ImportBatch>("/import/batches", file, { Target: target }),
	commitImportBatch: (id: string) => send<ImportBatch>(`/import/batches/${encodeURIComponent(id)}/commit`, { method: "POST" }),
	undoImportBatch: (id: string) => send<ImportBatch>(`/import/batches/${encodeURIComponent(id)}/undo`, { method: "POST" }),
	downloadImportBatchReport: (id: string) => fetch(`/api/import/batches/${encodeURIComponent(id)}/report`, { headers: authHeaders() })
};
