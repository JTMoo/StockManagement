import { useRef, useState } from "react";
import { api, type ApiFailure, type DetectedColumns, type ImportBatch, type ImportColumnMapping, type ImportRowStatus, type ImportTarget } from "../../api";
import { FailureMessage } from "../../FailureMessage";
import { StatusBadge, type StatusTone } from "../../StatusBadge";
import { useI18n } from "../../i18n";

const rowStatusTone: Record<ImportRowStatus, StatusTone> = { Ready: "success", Duplicate: "warning", Error: "danger" };
const rowStatusLabelKey = { Ready: "statusImported", Duplicate: "statusDuplicate", Error: "statusError" } as const;

/** Upload → map columns → preview → commit/undo panel shared by every import target (ADR-0018, ADR-0028). */
export function ImportPanel({ target, onImported }: { target: ImportTarget; onImported?: () => void })
{
	const { t } = useI18n();
	const fileInput = useRef<HTMLInputElement>(null);
	const [file, setFile] = useState<File>();
	const [columns, setColumns] = useState<DetectedColumns>();
	const [mapping, setMapping] = useState<ImportColumnMapping>({});
	const [batch, setBatch] = useState<ImportBatch>();
	const [failure, setFailure] = useState<ApiFailure>();
	const [busy, setBusy] = useState(false);

	async function onDetectColumns()
	{
		if (!file) return;

		setBusy(true);
		const response = await api.detectImportColumns(target, file);
		setBusy(false);
		if (!response.ok) return setFailure(response.failure);

		setFailure(undefined);
		setColumns(response.value);
		setMapping(Object.fromEntries(response.value.columns.filter(column => column.matchedFieldName).map(column => [column.column, column.matchedFieldName!])));
	}

	async function onPreview()
	{
		if (!file) return;

		setBusy(true);
		const response = await api.previewImport(target, file, mapping);
		setBusy(false);
		if (!response.ok) return setFailure(response.failure);

		setFailure(undefined);
		setBatch(response.value);
		setFile(undefined);
		setColumns(undefined);
		setMapping({});
		if (fileInput.current) fileInput.current.value = "";
	}

	async function onCommit()
	{
		if (!batch) return;

		setBusy(true);
		const response = await api.commitImportBatch(batch.id);
		setBusy(false);
		if (!response.ok) return setFailure(response.failure);

		setFailure(undefined);
		setBatch(response.value);
		onImported?.();
	}

	async function onUndo()
	{
		if (!batch) return;

		setBusy(true);
		const response = await api.undoImportBatch(batch.id);
		setBusy(false);
		if (!response.ok) return setFailure(response.failure);

		setFailure(undefined);
		setBatch(response.value);
		onImported?.();
	}

	async function onDownloadReport()
	{
		if (!batch) return;

		const response = await api.downloadImportBatchReport(batch.id);
		if (!response.ok) return setFailure({ kind: "unexpected" });

		const url = URL.createObjectURL(await response.blob());
		const link = document.createElement("a");
		link.href = url;
		link.download = `${batch.fileName.replace(/\.[^.]+$/, "")}-report.csv`;
		link.click();
		URL.revokeObjectURL(url);
	}

	return (
		<div className="panel">
			<div className="form-actions">
				<input ref={fileInput} type="file" accept=".xlsx" aria-label={t("chooseFile")} onChange={event => { setFile(event.target.files?.[0]); setColumns(undefined); }} />
				{!columns && <button type="button" disabled={!file || busy} onClick={onDetectColumns}>{t("preview")}</button>}
			</div>
			<FailureMessage failure={failure} />
			{columns && !batch && (
				<>
					<table>
						<thead><tr><th>{t("column")}</th><th>{t("field")}</th></tr></thead>
						<tbody>
							{columns.columns.map(column => (
								<tr key={column.column}>
									<td>{column.header}</td>
									<td>
										<select aria-label={column.header} value={mapping[column.column] ?? ""} onChange={event => setMapping({ ...mapping, [column.column]: event.target.value })}>
											<option value="">{t("ignoreColumn")}</option>
											{columns.fields.map(field => <option key={field.name} value={field.name}>{field.displayName}</option>)}
										</select>
									</td>
								</tr>
							))}
						</tbody>
					</table>
					<button type="button" disabled={busy} onClick={onPreview}>{t("preview")}</button>
				</>
			)}
			{batch && (
				<>
					<div className="form-grid">
						<label>{t("imported")}<output>{batch.readyCount}</output></label>
						<label>{t("duplicatesSkipped")}<output>{batch.duplicateCount}</output></label>
					</div>
					{batch.status === "Previewed" && <button type="button" disabled={batch.readyCount === 0 || busy} onClick={onCommit}>{t("commit")}</button>}
					{batch.status === "Committed" && <button type="button" disabled={busy} onClick={onUndo}>{t("undo")}</button>}
					{batch.rows.some(row => row.status !== "Ready") && <button type="button" onClick={onDownloadReport}>{t("downloadReport")}</button>}
					<table>
						<thead><tr><th>{t("row")}</th><th>{t("status")}</th><th></th></tr></thead>
						<tbody>
							{batch.rows.filter(row => row.status !== "Ready").map(row => (
								<tr key={row.row}>
									<td>{row.row}</td>
									<td><StatusBadge tone={rowStatusTone[row.status]}>{t(rowStatusLabelKey[row.status])}</StatusBadge></td>
									<td>{row.message ?? rowSummary(row.fields)}</td>
								</tr>
							))}
						</tbody>
					</table>
				</>
			)}
		</div>
	);
}

function rowSummary(fields?: Record<string, unknown>): string
{
	return Object.values(fields ?? {}).filter(value => typeof value === "string" && value).join(" ");
}
