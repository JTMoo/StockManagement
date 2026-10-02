namespace StockManagement.Sifen.Core.Contracts;


/// <summary>
/// One invoice line (<c>gCamItem</c>) in a DTE. <paramref name="UnitPrice"/> is VAT-inclusive, same convention as <c>SaleLine</c>.
/// </summary>
public sealed record DteItem(
	string Code,
	string Description,
	decimal Quantity,
	decimal UnitPrice,
	decimal VatRatePercent);
