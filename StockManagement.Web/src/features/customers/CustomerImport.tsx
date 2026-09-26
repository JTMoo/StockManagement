import { ImportPanel } from "../import/ImportPanel";

/** Embedded in CustomerList (toggled by its "Excel Import" toolbar button), not its own nav entry. */
export function CustomerImport({ onImported }: { onImported?: () => void })
{
	return <ImportPanel target="Customers" onImported={onImported} />;
}
