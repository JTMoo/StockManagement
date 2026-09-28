import { ShoppingCart } from "lucide-react";
import type { NavRoute } from "../../routes";
import { SaleForm } from "./SaleForm";

export const salesRoute: NavRoute = {
	name: "newSale",
	icon: ShoppingCart,
	render: ({ onSold }) => <SaleForm onSold={onSold} />
};
