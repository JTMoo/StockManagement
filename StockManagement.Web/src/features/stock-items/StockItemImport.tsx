import { useRef, useState } from "react";
import { api, type ApiFailure, type StockItemImportResult } from "../../api";
import { FailureMessage } from "../../FailureMessage";
import { Page } from "../../Page";
import { useI18n } from "../../i18n";

export function StockItemImport()
{
	const { t } = useI18n();
	const fileInput = useRef<HTMLInputElement>(null);
	const [file, setFile] = useState<File>();
	const [result, setResult] = useState<StockItemImportResult>();
	const [failure, setFailure] = useState<ApiFailure>();
	const [busy, setBusy] = useState(false);

	async function onImport()
	{
		if (!file) return;

		setBusy(true);
		const response = await api.importStockItems(file);
		setBusy(false);
		if (!response.ok) return setFailure(response.failure);

		setFailure(undefined);
		setResult(response.value);
		setFile(undefined);
		if (fileInput.current) fileInput.current.value = "";
	}

	return (
		<Page title={t("excelImport")}>
			<div className="panel form-actions">
				<input ref={fileInput} type="file" accept=".xlsx" aria-label={t("chooseFile")} onChange={event => setFile(event.target.files?.[0])} />
				<button type="button" disabled={!file || busy} onClick={onImport}>{t("import")}</button>
			</div>
			<FailureMessage failure={failure} />
			{result && (
				<div className="panel">
					<div className="form-grid">
						<label>{t("imported")}<output>{result.imported}</output></label>
						<label>{t("duplicatesSkipped")}<output>{result.duplicates}</output></label>
					</div>
					{result.errors.length > 0 && (
						<table>
							<thead><tr><th>{t("row")}</th><th></th></tr></thead>
							<tbody>
								{result.errors.map(error => <tr key={error.row}><td>{error.row}</td><td>{error.message}</td></tr>)}
							</tbody>
						</table>
					)}
				</div>
			)}
		</Page>
	);
}
