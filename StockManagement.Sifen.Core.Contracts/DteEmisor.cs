namespace StockManagement.Sifen.Core.Contracts;


/// <summary>
/// Issuer (<c>gEmis</c>) data for a DTE, sourced from <c>CompanySettings</c>.
/// </summary>
public sealed record DteEmisor(
	string RucBase,
	int RucCheckDigit,
	string RazonSocial,
	string EstablishmentCode,
	string PointOfSaleCode,
	string EstablishmentAddress,
	string TimbradoNumber,
	DateOnly TimbradoValidSince);
