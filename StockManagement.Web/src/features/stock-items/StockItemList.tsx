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
		<section>
			<h2>{t("stockItems")}</h2>
			<input type="search" placeholder={t("searchBoxDefault")} aria-label={t("searchBoxDefault")} value={search} onChange={event => setSearch(event.target.value)} />
			<FailureMessage failure={failure} />
			<table>
				<thead>
					<tr><th>{t("code")}</th><th>{t("name")}</th><th>{t("description")}</th><th>{t("location")}</th><th>{t("amount")}</th><th>{t("price")}</th></tr>
				</thead>
				<tbody>
					{visible.map(item => (
						<tr key={item.code}>
							<td>{item.code}</td><td>{item.name}</td><td>{item.description}</td><td>{item.location}</td>
							<td className="number">{formatNumber(item.amount)}</td><td className="number">{formatNumber(item.price)}</td>
						</tr>
					))}
				</tbody>
			</table>
		</section>
	);
}
