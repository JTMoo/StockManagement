import { useState } from "react";
import { api, type StockItem } from "../../api";
import { FailureMessage } from "../../FailureMessage";
import { Page } from "../../Page";
import { useI18n } from "../../i18n";
import { useLoad } from "../../useLoad";
import { OpeningStockImport } from "./OpeningStockImport";
import { StockCheckForm } from "./StockCheckForm";
import { StockItemForm } from "./StockItemForm";
import { StockItemImport } from "./StockItemImport";

export function StockItemList()
{
	const { t, formatNumber } = useI18n();
	const { data: stockItems = [], setData, failure } = useLoad(api.listStockItems);
	const [search, setSearch] = useState("");
	const [editing, setEditing] = useState<StockItem>();
	const [checking, setChecking] = useState<StockItem>();
	const [showImport, setShowImport] = useState(false);
	const [showOpeningStock, setShowOpeningStock] = useState(false);

	async function onImported()
	{
		const result = await api.listStockItems();
		if (result.ok) setData(result.value);
	}

	const term = search.trim().toLowerCase();
	const visible = stockItems.filter(item => [item.code, item.name, item.description, item.location].some(value => value.toLowerCase().includes(term)));

	function onSaved(stockItem: StockItem)
	{
		setData(editing ? stockItems.map(item => item.id === editing.id ? stockItem : item) : [...stockItems, stockItem]);
		setEditing(undefined);
	}

	function onChecked(stockItem: StockItem)
	{
		setData(stockItems.map(item => item.id === stockItem.id ? stockItem : item));
		setChecking(undefined);
	}

	async function onDelete(stockItem: StockItem)
	{
		if (!window.confirm(t("itemDeletionPrompt").replace("{0}", stockItem.name))) return;
		const result = await api.deleteStockItem(stockItem);
		if (!result.ok) return;

		setData(stockItems.filter(item => item.id !== stockItem.id));
		if (editing?.id === stockItem.id) setEditing(undefined);
		if (checking?.id === stockItem.id) setChecking(undefined);
	}

	return (
		<Page title={t("stockItems")} toolbar={<>
			<input type="search" aria-label={t("search")} placeholder={t("searchBoxDefault")} value={search} onChange={event => setSearch(event.target.value)} />
			<button type="button" className="quiet" aria-pressed={showImport} onClick={() => setShowImport(!showImport)}>{t("excelImport")}</button>
			<button type="button" className="quiet" aria-pressed={showOpeningStock} onClick={() => setShowOpeningStock(!showOpeningStock)}>{t("openingStock")}</button>
		</>}>
			<StockItemForm editing={editing} onSaved={onSaved} onCancel={() => setEditing(undefined)} />
			{checking && <StockCheckForm stockItem={checking} onChecked={onChecked} onCancel={() => setChecking(undefined)} />}
			{showImport && <StockItemImport onImported={onImported} />}
			{showOpeningStock && <OpeningStockImport onImported={onImported} />}
			<FailureMessage failure={failure} />
			<table>
				<thead>
					<tr><th>{t("name")}</th><th>{t("code")}</th><th className="number">{t("quantity")}</th><th className="number">{t("price")}</th><th>{t("manufacturer")}</th><th>{t("location")}</th><th></th></tr>
				</thead>
				<tbody>
					{visible.map(item => (
						<tr key={item.code} className={item.amount === 0 ? "sold-out" : undefined}>
							<td>{item.name}</td><td>{item.code}</td>
							<td className="number">{formatNumber(item.amount)}</td><td className="number">{formatNumber(item.price)}</td>
							<td>{item.manufacturer}</td><td>{item.location}</td>
							<td className="row-actions">
								<button type="button" className="quiet" onClick={() => setEditing(item)}>{t("edit")}</button>
								<button type="button" className="quiet" onClick={() => setChecking(item)}>{t("checkStock")}</button>
								<button type="button" className="quiet" onClick={() => onDelete(item)}>{t("deleteItem")}</button>
							</td>
						</tr>
					))}
				</tbody>
			</table>
		</Page>
	);
}
