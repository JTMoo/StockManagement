import { useState, type FormEvent } from "react";
import { api, type ApiFailure, type EditUser, type Permission, type User, type UserRole } from "../../api";
import { FailureMessage } from "../../FailureMessage";
import { useI18n, type TextKey } from "../../i18n";
import { PermissionCheckboxes } from "./PermissionCheckboxes";

const fields: (keyof EditUser & TextKey)[] = ["username", "fullName", "email", "phone", "position"];

export function EditUserForm({ user, onSaved, onCancel }: { user: User; onSaved: (user: User) => void; onCancel: () => void })
{
	const { t } = useI18n();
	const [draft, setDraft] = useState<EditUser>({ ...user, password: "" });
	const [failure, setFailure] = useState<ApiFailure>();
	const [busy, setBusy] = useState(false);

	async function onSubmit(event: FormEvent)
	{
		event.preventDefault();
		setBusy(true);
		const result = await api.updateUser({ ...draft, password: draft.password || undefined });
		setBusy(false);
		if (!result.ok) return setFailure(result.failure);

		setFailure(undefined);
		onSaved(result.value);
	}

	return (
		<form onSubmit={onSubmit} className="panel">
			<div className="form-grid">
				{fields.map(field => (
					<label key={field}>
						{t(field)}
						<input value={draft[field] ?? ""} required={field === "username"} onChange={event => setDraft({ ...draft, [field]: event.target.value })} />
					</label>
				))}
				<label>
					{t("password")}
					<input type="password" value={draft.password} minLength={8} placeholder={t("password")} onChange={event => setDraft({ ...draft, password: event.target.value })} />
				</label>
				<label>
					{t("role")}
					<select value={draft.role} onChange={event => setDraft({ ...draft, role: event.target.value as UserRole })}>
						<option value="Standard">{t("standard")}</option>
						<option value="Admin">{t("admin")}</option>
					</select>
				</label>
				<PermissionCheckboxes role={draft.role} permissions={draft.permissions} onChange={(permissions: Permission[]) => setDraft({ ...draft, permissions })} />
			</div>
			<FailureMessage failure={failure} duplicate="usernameAlreadyExists" />
			<div className="form-actions">
				<button type="submit" disabled={busy}>{t("saveUser")}</button>
				<button type="button" onClick={onCancel} disabled={busy}>{t("cancel")}</button>
			</div>
		</form>
	);
}
