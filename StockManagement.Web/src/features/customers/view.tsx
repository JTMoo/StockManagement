import { BookUser } from "lucide-react";
import type { ViewDef } from "../../viewRegistry";
import { CustomerList } from "./CustomerList";

export const customersView: ViewDef = { name: "clients", icon: BookUser, render: () => <CustomerList /> };
