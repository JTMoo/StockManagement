// Registry: combines the per-domain clients below into one `api` object. Each domain owns its own
// file to keep unrelated changes from touching the same lines (see ADR-0026).

export * from "./client";
export * from "./stockItems";
export * from "./customers";
export * from "./invoices";
export * from "./sales";
export * from "./settings";
export * from "./import";
export * from "./users";
export * from "./auth";

import { authApi } from "./auth";
import { customersApi } from "./customers";
import { importApi } from "./import";
import { invoicesApi } from "./invoices";
import { salesApi } from "./sales";
import { settingsApi } from "./settings";
import { stockItemsApi } from "./stockItems";
import { usersApi } from "./users";

export const api = {
	...stockItemsApi,
	...customersApi,
	...invoicesApi,
	...salesApi,
	...settingsApi,
	...importApi,
	...usersApi,
	...authApi
};
