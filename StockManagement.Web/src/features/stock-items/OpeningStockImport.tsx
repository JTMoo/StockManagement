import { ImportPanel } from "../import/ImportPanel";

/** Embedded in StockItemList (toggled by its "Opening Stock" toolbar button), not its own nav entry. */
export function OpeningStockImport({ onImported }: { onImported?: () => void })
{
	return <ImportPanel target="OpeningStock" onImported={onImported} />;
}
