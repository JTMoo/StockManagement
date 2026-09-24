import { useState } from "react";
import { api } from "../../api";
import { FailureMessage } from "../../FailureMessage";
import { useI18n } from "../../i18n";
import { useLoad } from "../../useLoad";

export function StockItemList()
{
	const { t, formatNumber } = useI18n();
	const { data: stockItems = [], failure } = useLoad(api.listStockItems);
	const [search, setSearch] = useState("");

	const term = search.trim().toLowerCase();
	const visible = stockItems.filter(item => [item.code, item.name, item.description, item.location].some(value => value.toLowerCase().includes(term)));

	return (
		<section className="with-sidebar">
			<div>
				<h2>{t("stockItems")}</h2>
				<FailureMessage failure={failure} />
				<table>
					<thead>
						<tr><th>{t("name")}</th><th>{t("code")}</th><th className="number">{t("quantity")}</th><th className="number">{t("price")}</th><th>{t("manufacturer")}</th><th>{t("location")}</th></tr>
					</thead>
					<tbody>
						{visible.map(item => (
							<tr key={item.code}>
								<td>{item.name}</td><td>{item.code}</td>
								<td className="number">{formatNumber(item.amount)}</td><td className="number">{formatNumber(item.price)}</td>
								<td>{item.manufacturer}</td><td>{item.location}</td>
							</tr>
						))}
					</tbody>
				</table>
			</div>
			<aside>
				<label>
					{t("search")}
					<input type="search" placeholder={t("searchBoxDefault")} value={search} onChange={event => setSearch(event.target.value)} />
				</label>
			</aside>
		</section>
	);
}
