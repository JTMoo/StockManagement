import { BookUser } from "lucide-react";
import type { NavRoute } from "../../routes";
import { CustomerList } from "./CustomerList";

export const customersRoute: NavRoute = {
	name: "clients",
	icon: BookUser,
	render: () => <CustomerList />
};
