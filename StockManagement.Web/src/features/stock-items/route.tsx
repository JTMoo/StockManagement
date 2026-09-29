import { Wrench } from "lucide-react";
import type { NavRoute } from "../../routes";
import { StockItemList } from "./StockItemList";

export const stockItemsRoute: NavRoute = {
	name: "stockItems",
	icon: Wrench,
	render: () => <StockItemList />
};
