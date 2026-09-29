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

export type ImportField = { name: string; displayName: string };

export type DetectedColumn = { column: number; header: string; matchedFieldName?: string };

export type DetectedColumns = { sheetName: string; columns: DetectedColumn[]; fields: ImportField[] };

/** Column number → target field name (ImportField.name); confirmed by the user before preview (ADR-0028) */
export type ImportColumnMapping = Record<number, string>;

export const importApi = {
	detectImportColumns: (target: ImportTarget, file: File) => sendForm<DetectedColumns>("/import/batches/columns", file, { Target: target }),
	previewImport: (target: ImportTarget, file: File, mapping?: ImportColumnMapping) =>
		sendForm<ImportBatch>("/import/batches", file, { Target: target, ...(mapping ? { Mapping: JSON.stringify(mapping) } : {}) }),
	commitImportBatch: (id: string) => send<ImportBatch>(`/import/batches/${encodeURIComponent(id)}/commit`, { method: "POST" }),
	undoImportBatch: (id: string) => send<ImportBatch>(`/import/batches/${encodeURIComponent(id)}/undo`, { method: "POST" }),
	downloadImportBatchReport: (id: string) => fetch(`/api/import/batches/${encodeURIComponent(id)}/report`, { headers: authHeaders() })
};
