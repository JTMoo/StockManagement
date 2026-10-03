namespace StockManagement.Sales.Core.Contracts;


/// <summary>
/// One row of the Marangatu IVA sales book export (#123); layout is a best-effort approximation of DNIT's
/// "Especificaciones Técnicas para registro de comprobantes en Marangatu" spec, unverified (no network access
/// to the DNIT document from this environment) - diff against the real file before relying on it for filing.
/// </summary>
public sealed record IvaBookRow(string DocumentTypeCode, string Number, DateTime Date, string CustomerRuc, string CustomerName,
	decimal Taxed10, decimal Vat10, decimal Taxed5, decimal Vat5, decimal Exempt, decimal Total);
