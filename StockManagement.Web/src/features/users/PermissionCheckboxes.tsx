import type { Permission, UserRole } from "../../api";
import { useI18n } from "../../i18n";
import { permissionOptions } from "./permissionOptions";

/** Role Admin implies every permission (ADR-0017): the checkboxes only matter, and only show, for Standard. */
export function PermissionCheckboxes({ role, permissions, onChange }: { role: UserRole; permissions: Permission[]; onChange: (permissions: Permission[]) => void })
{
	const { t } = useI18n();

	if (role === "Admin") return null;

	function toggle(permission: Permission, checked: boolean)
	{
		onChange(checked ? [...permissions, permission] : permissions.filter(existing => existing !== permission));
	}

	return (
		<fieldset className="checkbox-group">
			<legend>{t("rights")}</legend>
			{permissionOptions.map(({ value, label }) => (
				<label key={value}>
					<input type="checkbox" checked={permissions.includes(value)} onChange={event => toggle(value, event.target.checked)} />
					{t(label)}
				</label>
			))}
		</fieldset>
	);
}
