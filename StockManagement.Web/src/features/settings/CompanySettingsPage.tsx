import { useState, type FormEvent } from "react";
import { api, type ApiFailure, type CompanySettings } from "../../api";
import { FailureMessage } from "../../FailureMessage";
import { Page } from "../../Page";
import { useI18n } from "../../i18n";
import { useLoad } from "../../useLoad";

export function CompanySettingsPage()
{
	const { t } = useI18n();
	const { data: settings, setData, failure: loadFailure } = useLoad(api.getCompanySettings);
	const [draft, setDraft] = useState<CompanySettings>();
	const [failure, setFailure] = useState<ApiFailure>();
	const [busy, setBusy] = useState(false);

	const current = draft ?? settings;

	async function onSubmit(event: FormEvent)
	{
		event.preventDefault();
		if (!current) return;

		setBusy(true);
		const result = await api.updateCompanySettings(current);
		setBusy(false);
		if (!result.ok) return setFailure(result.failure);

		setFailure(undefined);
		setData(result.value);
		setDraft(undefined);
	}

	if (!current) return <Page title={t("companySettings")}><FailureMessage failure={loadFailure} /></Page>;

	return (
		<Page title={t("companySettings")}>
			<form onSubmit={onSubmit} className="panel">
				<div className="form-grid">
					<label>
						{t("companyName")}
						<input value={current.companyName} onChange={event => setDraft({ ...current, companyName: event.target.value })} />
					</label>
					<label>
						{t("taxId")}
						<input value={current.taxId} onChange={event => setDraft({ ...current, taxId: event.target.value })} />
					</label>
					<label>
						{t("currency")}
						<input value={current.currency} onChange={event => setDraft({ ...current, currency: event.target.value })} />
					</label>
					<label>
						{t("vatRate")}
						<input type="number" min={0} max={100} step="0.01" value={current.vatRatePercent} onChange={event => setDraft({ ...current, vatRatePercent: Number(event.target.value) })} />
					</label>
					<label>
						{t("paymentTermInDays")}
						<input type="number" min={0} step="1" value={current.paymentTermInDays} onChange={event => setDraft({ ...current, paymentTermInDays: Number(event.target.value) })} />
					</label>
					<label>
						{t("firstInvoiceNumber")}
						<input type="number" min={1} step="1" value={current.firstInvoiceNumber} onChange={event => setDraft({ ...current, firstInvoiceNumber: Number(event.target.value) })} />
					</label>
					<label>
						{t("firstCustomerId")}
						<input type="number" min={1} step="1" value={current.firstCustomerId} onChange={event => setDraft({ ...current, firstCustomerId: Number(event.target.value) })} />
					</label>
					<label>
						{t("currencyDecimalDigits")}
						<input type="number" min={0} max={4} step="1" value={current.currencyDecimalDigits} onChange={event => setDraft({ ...current, currencyDecimalDigits: Number(event.target.value) })} />
					</label>
					<label>
						{t("ruc")}
						<input value={current.ruc} onChange={event => setDraft({ ...current, ruc: event.target.value })} />
					</label>
					<label>
						{t("timbradoNumber")}
						<input value={current.timbradoNumber} onChange={event => setDraft({ ...current, timbradoNumber: event.target.value })} />
					</label>
					<label>
						{t("timbradoValidFrom")}
						<input type="date" value={current.timbradoValidFrom?.slice(0, 10) ?? ""} onChange={event => setDraft({ ...current, timbradoValidFrom: event.target.value || null })} />
					</label>
					<label>
						{t("timbradoValidTo")}
						<input type="date" value={current.timbradoValidTo?.slice(0, 10) ?? ""} onChange={event => setDraft({ ...current, timbradoValidTo: event.target.value || null })} />
					</label>
					<label>
						{t("establishmentCode")}
						<input value={current.establishmentCode} pattern="\d{3}" onChange={event => setDraft({ ...current, establishmentCode: event.target.value })} />
					</label>
					<label>
						{t("pointOfSaleCode")}
						<input value={current.pointOfSaleCode} pattern="\d{3}" onChange={event => setDraft({ ...current, pointOfSaleCode: event.target.value })} />
					</label>
				</div>
				<FailureMessage failure={failure} />
				<div className="form-actions">
					<button type="submit" disabled={busy}>{t("save")}</button>
				</div>
			</form>
		</Page>
	);
}
