import { useState, type FormEvent } from "react";
import type { ApiFailure } from "../../api";
import { useAuth } from "../../auth";
import { FailureMessage } from "../../FailureMessage";
import { useI18n } from "../../i18n";

export function LoginPage()
{
	const { t } = useI18n();
	const { login } = useAuth();
	const [username, setUsername] = useState("");
	const [password, setPassword] = useState("");
	const [failure, setFailure] = useState<ApiFailure>();
	const [busy, setBusy] = useState(false);

	async function onSubmit(event: FormEvent)
	{
		event.preventDefault();
		setBusy(true);
		const result = await login(username, password);
		setBusy(false);
		if (!result.ok) setFailure(result.failure);
	}

	return (
		<div className="login">
			<form onSubmit={onSubmit} className="panel form-grid">
				<h2>{t("userLogin")}</h2>
				<FailureMessage failure={failure} unauthorized="invalidCredentials" />
				<label>
					{t("username")}
					<input value={username} autoFocus required onChange={event => setUsername(event.target.value)} />
				</label>
				<label>
					{t("password")}
					<input type="password" value={password} required onChange={event => setPassword(event.target.value)} />
				</label>
				<div className="form-actions">
					<button type="submit" disabled={busy}>{t("userLogin")}</button>
				</div>
			</form>
		</div>
	);
}
