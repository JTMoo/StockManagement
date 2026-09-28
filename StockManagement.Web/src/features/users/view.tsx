import { Users } from "lucide-react";
import type { ViewDef } from "../../viewRegistry";
import { UserList } from "./UserList";

// #14: user management is only useful (and only allowed by the server) with Users.Manage
export const usersView: ViewDef = { name: "users", icon: Users, permission: "Users.Manage", render: () => <UserList /> };
