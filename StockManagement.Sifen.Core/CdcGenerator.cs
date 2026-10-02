using StockManagement.Kernel.Util;
using StockManagement.Sifen.Core.Contracts;

namespace StockManagement.Sifen.Core;


/// <summary>
/// Builds the 44-digit CDC: 2-digit doc type, 8-digit RUC + 1-digit RUC check digit, 3-digit establishment,
/// 3-digit point of sale, 7-digit document number, 1-digit taxpayer type, 8-digit issue date (yyyyMMdd),
/// 1-digit emission type, 9-digit security code, 1-digit check digit (43 digits, módulo 11).
/// </summary>
/// <remarks>
/// Field layout matches DNIT's published SIFEN CDC structure. The módulo-11 check digit algorithm mirrors
/// <see cref="RucValidator"/> (same SET check-digit family used throughout Paraguayan tax documents); it has
/// not been cross-checked against an official DNIT worked example in this session (no network access to
/// dnit.gov.py) — verify against a real DNIT-issued CDC before relying on this for production transmission.
/// </remarks>
public sealed class CdcGenerator : ICdcGenerator
{
	public string Generate(CdcInput input)
	{
		if (input.RucBase.Length == 0 || input.RucBase.Length > 8 || !input.RucBase.All(char.IsDigit))
			throw new ArgumentException("RucBase must be 1-8 digits.", nameof(input));
		if (input.EstablishmentCode.Length != 3 || !input.EstablishmentCode.All(char.IsDigit))
			throw new ArgumentException("EstablishmentCode must be 3 digits.", nameof(input));
		if (input.PointOfSaleCode.Length != 3 || !input.PointOfSaleCode.All(char.IsDigit))
			throw new ArgumentException("PointOfSaleCode must be 3 digits.", nameof(input));
		if (input.DocumentNumber is < 1 or > 9999999)
			throw new ArgumentException("DocumentNumber must be 1-9999999.", nameof(input));
		if (input.SecurityCode.Length != 9 || !input.SecurityCode.All(char.IsDigit))
			throw new ArgumentException("SecurityCode must be 9 digits.", nameof(input));

		var body =
			$"{(int)input.DocumentType:D2}" +
			$"{input.RucBase.PadLeft(8, '0')}" +
			$"{input.RucCheckDigit:D1}" +
			$"{input.EstablishmentCode}" +
			$"{input.PointOfSaleCode}" +
			$"{input.DocumentNumber:D7}" +
			$"{(int)input.TaxpayerType:D1}" +
			$"{input.IssueDate:yyyyMMdd}" +
			$"{(int)input.EmissionType:D1}" +
			$"{input.SecurityCode}";

		return body + Mod11.ComputeCheckDigit(body);
	}
}
