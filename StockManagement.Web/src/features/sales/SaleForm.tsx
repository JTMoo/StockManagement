import { useState, type FormEvent } from "react";
import { api, type ApiFailure, type Invoice, type SaleCondition, type StockItem } from "../../api";
import { FailureMessage } from "../../FailureMessage";
import { useI18n } from "../../i18n";
import { useLoad } from "../../useLoad";

type CartLine = { item: StockItem; amount: number };

export function SaleForm({ onSold }: { onSold: (invoice: Invoice) => void })
{
	const { t, formatNumber } = useI18n();
	const customers = useLoad(api.listCustomers);
	const stockItems = useLoad(api.listStockItems);
	const [customerId, setCustomerId] = useState("");
	const [code, setCode] = useState("");
	const [amount, setAmount] = useState(1);
	const [saleCondition, setSaleCondition] = useState<SaleCondition>("Cash");
	const [cart, setCart] = useState<CartLine[]>([]);
	const [failure, setFailure] = useState<ApiFailure>();
	const [busy, setBusy] = useState(false);

	const available = (stockItems.data ?? []).filter(item => item.amount > 0);
	const total = cart.reduce((sum, line) => sum + line.item.price * line.amount, 0);

	function onAdd()
	{
		const item = available.find(candidate => candidate.code === code);
		if (!item || amount < 1) return;

		const existing = cart.find(line => line.item.code === code);
		setCart(existing
			? cart.map(line => line === existing ? { ...line, amount: line.amount + amount } : line)
			: [...cart, { item, amount }]);
		setAmount(1);
	}

	async function onSubmit(event: FormEvent)
	{
		event.preventDefault();
		setBusy(true);
		const result = await api.createSale({ customerId: Number(customerId), saleCondition, items: cart.map(line => ({ code: line.item.code, amount: line.amount })) });
		setBusy(false);
		if (!result.ok) return setFailure(result.failure);

		onSold(result.value);
	}

	return (
		<form onSubmit={onSubmit}>
			<h2>{t("newSale")}</h2>
			<FailureMessage failure={customers.failure ?? stockItems.failure} />
			<div className="inline-form">
				<label>
					{t("customer")}
					<select value={customerId} required onChange={event => setCustomerId(event.target.value)}>
						<option value="">{t("selectCustomer")}</option>
						{customers.data?.map(customer => <option key={customer.customerId} value={customer.customerId}>{`${customer.customerId} ${customer.name} ${customer.lastname}`.trim()}</option>)}
					</select>
				</label>
				<label>
					{t("saleCondition")}
					<select value={saleCondition} onChange={event => setSaleCondition(event.target.value as SaleCondition)}>
						<option value="Cash">{t("cash")}</option>
						<option value="Credit">{t("credit")}</option>
					</select>
				</label>
			</div>
			<div className="inline-form">
				<label>
					{t("stockItem")}
					<select value={code} onChange={event => setCode(event.target.value)}>
						<option value="" />
						{available.map(item => <option key={item.code} value={item.code}>{`${item.code} ${item.name} (${item.amount})`}</option>)}
					</select>
				</label>
				<label>
					{t("quantity")}
					<input type="number" min={1} value={amount} onChange={event => setAmount(Number(event.target.value))} />
				</label>
				<button type="button" onClick={onAdd} disabled={!code}>{t("addToShoppingCart")}</button>
			</div>
			<table aria-label={t("shoppingCart")}>
				<thead>
					<tr><th>{t("code")}</th><th>{t("name")}</th><th>{t("quantity")}</th><th>{t("price")}</th><th /></tr>
				</thead>
				<tbody>
					{cart.map(line => (
						<tr key={line.item.code}>
							<td>{line.item.code}</td><td>{line.item.name}</td>
							<td className="number">{formatNumber(line.amount)}</td><td className="number">{formatNumber(line.item.price * line.amount)}</td>
							<td><button type="button" onClick={() => setCart(cart.filter(other => other !== line))}>{t("remove")}</button></td>
						</tr>
					))}
				</tbody>
				<tfoot>
					<tr><th colSpan={3}>{t("total")}</th><td className="number">{formatNumber(total)}</td><td /></tr>
				</tfoot>
			</table>
			<FailureMessage failure={failure} notFound="customerNotFound" />
			<button type="submit" disabled={busy || cart.length === 0 || !customerId}>{t("sell")}</button>
		</form>
	);
}
