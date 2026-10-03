namespace StockManagement.Sifen.Core.Contracts;


/// <summary>
/// SIFEN <c>iTiDE</c> document type code. Only the types Kora currently issues are mapped.
/// </summary>
/// <remarks>
/// <see cref="NotaDeRemisionElectronica"/> = 7 per DNIT's published code list (#162); not cross-checked against
/// an official DNIT example in this session (no network access to dnit.gov.py) - same flag as every other
/// unverified SIFEN constant in this project.
/// </remarks>
public enum SifenDocumentType
{
	FacturaElectronica = 1,
	NotaDeRemisionElectronica = 7,
}
