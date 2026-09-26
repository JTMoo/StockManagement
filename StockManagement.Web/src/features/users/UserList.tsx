import { useState } from "react";
import { api, type User } from "../../api";
import { useAuth } from "../../auth";
import { FailureMessage } from "../../FailureMessage";
import { Page } from "../../Page";
import { useI18n } from "../../i18n";
import { useLoad } from "../../useLoad";
import { CreateUserForm } from "./CreateUserForm";
import { EditUserForm } from "./EditUserForm";

export function UserList()
{
	const { t } = useI18n();
	const { username: ownUsername } = useAuth();
	const { data: users = [], setData, failure: loadFailure } = useLoad(api.listUsers);
	const [editing, setEditing] = useState<User>();
	const [deleteFailure, setDeleteFailure] = useState(loadFailure);

	const onCreated = (user: User) => setData([...users, user]);
	const onSaved = (user: User) =>
	{
		setData(users.map(existing => existing.id === user.id ? user : existing));
		setEditing(undefined);
	};

	async function onDelete(user: User)
	{
		const result = await api.deleteUser(user);
		if (!result.ok) return setDeleteFailure(result.failure);

		setDeleteFailure(undefined);
		setData(users.filter(existing => existing.id !== user.id));
	}

	return (
		<Page title={t("users")}>
			{editing
				? <EditUserForm user={editing} onSaved={onSaved} onCancel={() => setEditing(undefined)} />
				: <CreateUserForm onCreated={onCreated} />}
			<FailureMessage failure={deleteFailure ?? loadFailure} />
			<table>
				<thead>
					<tr><th>{t("username")}</th><th>{t("fullName")}</th><th>{t("email")}</th><th>{t("role")}</th><th>{t("position")}</th><th /></tr>
				</thead>
				<tbody>
					{users.map(user => (
						<tr key={user.id}>
							<td>{user.username}</td><td>{user.fullName}</td><td>{user.email}</td>
							<td>{t(user.role === "Admin" ? "admin" : "standard")}</td><td>{user.position}</td>
							<td className="row-actions">
								<button type="button" onClick={() => setEditing(user)}>{t("edit")}</button>
								{user.username !== ownUsername && <button type="button" onClick={() => onDelete(user)}>{t("deleteUser")}</button>}
							</td>
						</tr>
					))}
				</tbody>
			</table>
		</Page>
	);
}
