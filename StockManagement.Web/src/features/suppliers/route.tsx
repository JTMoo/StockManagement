import { Truck } from "lucide-react";
import type { NavRoute } from "../../routes";
import { SupplierList } from "./SupplierList";

export const suppliersRoute: NavRoute = {
	name: "suppliers",
	icon: Truck,
	permission: "Suppliers.Read",
	render: () => <SupplierList />
};
