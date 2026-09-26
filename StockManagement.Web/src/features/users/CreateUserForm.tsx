import { useState, type FormEvent } from "react";
import { api, type ApiFailure, type NewUser, type Permission, type User, type UserRole } from "../../api";
import { FailureMessage } from "../../FailureMessage";
import { useI18n, type TextKey } from "../../i18n";
import { PermissionCheckboxes } from "./PermissionCheckboxes";

const fields: (keyof NewUser & TextKey)[] = ["username", "fullName", "email", "phone", "position"];
const empty: NewUser = { username: "", password: "", role: "Standard", permissions: [] };

export function CreateUserForm({ onCreated }: { onCreated: (user: User) => void })
{
	const { t } = useI18n();
	const [user, setUser] = useState<NewUser>(empty);
	const [failure, setFailure] = useState<ApiFailure>();
	const [busy, setBusy] = useState(false);

	async function onSubmit(event: FormEvent)
	{
		event.preventDefault();
		setBusy(true);
		const result = await api.createUser(user);
		setBusy(false);
		if (!result.ok) return setFailure(result.failure);

		setFailure(undefined);
		setUser(empty);
		onCreated(result.value);
	}

	return (
		<form onSubmit={onSubmit} className="panel">
			<div className="form-grid">
				{fields.map(field => (
					<label key={field}>
						{t(field)}
						<input value={user[field] ?? ""} required={field === "username"} onChange={event => setUser({ ...user, [field]: event.target.value })} />
					</label>
				))}
				<label>
					{t("password")}
					<input type="password" value={user.password} required minLength={8} onChange={event => setUser({ ...user, password: event.target.value })} />
				</label>
				<label>
					{t("role")}
					<select value={user.role} onChange={event => setUser({ ...user, role: event.target.value as UserRole })}>
						<option value="Standard">{t("standard")}</option>
						<option value="Admin">{t("admin")}</option>
					</select>
				</label>
				<PermissionCheckboxes role={user.role ?? "Standard"} permissions={user.permissions ?? []} onChange={(permissions: Permission[]) => setUser({ ...user, permissions })} />
			</div>
			<FailureMessage failure={failure} duplicate="usernameAlreadyExists" />
			<div className="form-actions">
				<button type="submit" disabled={busy}>{t("createUser")}</button>
			</div>
		</form>
	);
}
