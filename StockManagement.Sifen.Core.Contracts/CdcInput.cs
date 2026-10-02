namespace StockManagement.Sifen.Core.Contracts;


/// <summary>
/// Fields that make up a 44-digit CDC (control code), per DNIT's SIFEN manual.
/// </summary>
/// <param name="RucBase">Issuer RUC without its check digit, digits only.</param>
/// <param name="RucCheckDigit">Issuer RUC check digit (see <c>RucValidator</c>).</param>
/// <param name="EstablishmentCode">3-digit establishment code (e.g. <c>"001"</c>).</param>
/// <param name="PointOfSaleCode">3-digit point-of-sale code (e.g. <c>"001"</c>).</param>
/// <param name="DocumentNumber">Document sequence number, 1-9999999.</param>
/// <param name="SecurityCode">9-digit contributor security code, digits only.</param>
public sealed record CdcInput(
	SifenDocumentType DocumentType,
	string RucBase,
	int RucCheckDigit,
	string EstablishmentCode,
	string PointOfSaleCode,
	long DocumentNumber,
	TaxpayerType TaxpayerType,
	DateOnly IssueDate,
	EmissionType EmissionType,
	string SecurityCode);
