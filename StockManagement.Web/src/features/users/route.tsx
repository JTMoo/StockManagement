import { Users } from "lucide-react";
import type { NavRoute } from "../../routes";
import { UserList } from "./UserList";

// #14: user management is only useful (and only allowed by the server) with Users.Manage
export const usersRoute: NavRoute = {
	name: "users",
	icon: Users,
	permission: "Users.Manage",
	render: () => <UserList />
};
