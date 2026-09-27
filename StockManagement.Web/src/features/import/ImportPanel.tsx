import { useRef, useState } from "react";
import { api, type ApiFailure, type ImportBatch, type ImportTarget } from "../../api";
import { FailureMessage } from "../../FailureMessage";
import { useI18n } from "../../i18n";

/** Upload → preview → commit/undo panel shared by every import target (ADR-0018). */
export function ImportPanel({ target, onImported }: { target: ImportTarget; onImported?: () => void })
{
	const { t } = useI18n();
	const fileInput = useRef<HTMLInputElement>(null);
	const [file, setFile] = useState<File>();
	const [batch, setBatch] = useState<ImportBatch>();
	const [failure, setFailure] = useState<ApiFailure>();
	const [busy, setBusy] = useState(false);

	async function onPreview()
	{
		if (!file) return;

		setBusy(true);
		const response = await api.previewImport(target, file);
		setBusy(false);
		if (!response.ok) return setFailure(response.failure);

		setFailure(undefined);
		setBatch(response.value);
		setFile(undefined);
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

	return (
		<div className="panel">
			<div className="form-actions">
				<input ref={fileInput} type="file" accept=".xlsx" aria-label={t("chooseFile")} onChange={event => setFile(event.target.files?.[0])} />
				<button type="button" disabled={!file || busy} onClick={onPreview}>{t("preview")}</button>
			</div>
			<FailureMessage failure={failure} />
			{batch && (
				<>
					<div className="form-grid">
						<label>{t("imported")}<output>{batch.readyCount}</output></label>
						<label>{t("duplicatesSkipped")}<output>{batch.duplicateCount}</output></label>
					</div>
					{batch.status === "Previewed" && <button type="button" disabled={batch.readyCount === 0 || busy} onClick={onCommit}>{t("commit")}</button>}
					{batch.status === "Committed" && <button type="button" disabled={busy} onClick={onUndo}>{t("undo")}</button>}
					<table>
						<thead><tr><th>{t("row")}</th><th></th></tr></thead>
						<tbody>
							{batch.rows.filter(row => row.status !== "Ready").map(row => (
								<tr key={row.row}><td>{row.row}</td><td>{row.message ?? rowSummary(row.fields)}</td></tr>
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
