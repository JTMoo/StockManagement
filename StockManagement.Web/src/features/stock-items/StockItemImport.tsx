import { ImportPanel } from "../import/ImportPanel";

/** Embedded in StockItemList (toggled by its "Excel Import" toolbar button), not its own nav entry. */
export function StockItemImport({ onImported }: { onImported?: () => void })
{
	return <ImportPanel target="StockItems" onImported={onImported} />;
}
