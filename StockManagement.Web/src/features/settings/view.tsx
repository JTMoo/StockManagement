import { Settings } from "lucide-react";
import type { ViewDef } from "../../viewRegistry";
import { SettingsPage } from "./SettingsPage";

export const settingsView: ViewDef = { name: "settings", icon: Settings, render: () => <SettingsPage /> };
