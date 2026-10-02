import type { ReactNode } from "react";

/** Word + dot, never color alone (DataTable/StatusBadge spec). "muted" (Anulada) drops the dot - not a live state to act on. */
export type StatusTone = "success" | "warning" | "danger" | "info" | "muted";

export function StatusBadge({ tone, children, testId }: { tone: StatusTone; children: ReactNode; testId?: string })
{
	return (
		<span className={`badge ${tone}`} data-testid={testId}>
			{tone !== "muted" && <span className="dot" />}
			{children}
		</span>
	);
}
