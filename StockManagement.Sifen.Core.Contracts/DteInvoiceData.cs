namespace StockManagement.Sifen.Core.Contracts;


/// <summary>
/// Everything <see cref="IDteXmlBuilder"/> needs to build a Factura Electrónica DE.
/// </summary>
public sealed record DteInvoiceData(
	string Cdc,
	DteEmisor Emisor,
	DteReceptor Receptor,
	DateTime IssueDate,
	long DocumentNumber,
	IReadOnlyList<DteItem> Items,
	int CurrencyDecimalDigits);
