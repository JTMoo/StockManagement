import { ShoppingCart } from "lucide-react";
import type { ViewDef } from "../../viewRegistry";
import { SaleForm } from "./SaleForm";

export const salesView: ViewDef = { name: "newSale", icon: ShoppingCart, render: ({ onSold }) => <SaleForm onSold={onSold} /> };
