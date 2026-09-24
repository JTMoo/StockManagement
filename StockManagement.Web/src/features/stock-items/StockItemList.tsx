import { useState } from "react";
import { api, type StockItem } from "../../api";
import { FailureMessage } from "../../FailureMessage";
import { Page } from "../../Page";
import { useI18n } from "../../i18n";
import { useLoad } from "../../useLoad";
import { StockItemForm } from "./StockItemForm";

export function StockItemList()
{
	const { t, formatNumber } = useI18n();
	const { data: stockItems = [], setData, failure } = useLoad(api.listStockItems);
	const [search, setSearch] = useState("");
	const [editing, setEditing] = useState<StockItem>();

	const term = search.trim().toLowerCase();
	const visible = stockItems.filter(item => [item.code, item.name, item.description, item.location].some(value => value.toLowerCase().includes(term)));

	function onSaved(stockItem: StockItem)
	{
		setData(editing ? stockItems.map(item => item.id === editing.id ? stockItem : item) : [...stockItems, stockItem]);
		setEditing(undefined);
	}

	async function onDelete(stockItem: StockItem)
	{
		if (!window.confirm(t("itemDeletionPrompt").replace("{0}", stockItem.name))) return;
		const result = await api.deleteStockItem(stockItem);
		if (!result.ok) return;

		setData(stockItems.filter(item => item.id !== stockItem.id));
		if (editing?.id === stockItem.id) setEditing(undefined);
	}

	return (
		<Page title={t("stockItems")} toolbar={<input type="search" aria-label={t("search")} placeholder={t("searchBoxDefault")} value={search} onChange={event => setSearch(event.target.value)} />}>
			<StockItemForm editing={editing} onSaved={onSaved} onCancel={() => setEditing(undefined)} />
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
								<button type="button" className="quiet" onClick={() => onDelete(item)}>{t("deleteItem")}</button>
							</td>
						</tr>
					))}
				</tbody>
			</table>
		</Page>
	);
}
