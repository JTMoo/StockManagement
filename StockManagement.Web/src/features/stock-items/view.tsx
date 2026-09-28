import { Wrench } from "lucide-react";
import type { ViewDef } from "../../viewRegistry";
import { StockItemList } from "./StockItemList";

export const stockItemsView: ViewDef = { name: "stockItems", icon: Wrench, render: () => <StockItemList /> };
