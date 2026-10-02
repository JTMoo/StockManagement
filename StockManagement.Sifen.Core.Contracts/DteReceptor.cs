namespace StockManagement.Sifen.Core.Contracts;


/// <summary>
/// Buyer (<c>gDatRec</c>) data for a DTE. <paramref name="RucBase"/> is null for a buyer identified by CI instead of RUC.
/// </summary>
public sealed record DteReceptor(
	string Name,
	string? RucBase,
	int? RucCheckDigit,
	string? DocumentNumber);
