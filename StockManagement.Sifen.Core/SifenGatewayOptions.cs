namespace StockManagement.Sifen.Core;


/// <summary>
/// Direct-DNIT gateway config (ADR-0031, #134: no PSE). The signing certificate stays on the machine running the
/// API - only a local file path/password are configured, never the certificate itself or its PFX bytes in the DB.
/// </summary>
public sealed class SifenGatewayOptions
{
	public const string SectionName = "Sifen";

	/// <summary>
	/// Path to the taxpayer's PKCS#12 (.p12/.pfx) signing certificate, local to this machine
	/// </summary>
	public string CertificatePath { get; set; } = "";

	public string CertificatePassword { get; set; } = "";

	/// <summary>
	/// DNIT SOAP endpoint (sandbox vs. production per environment config, not hardcoded)
	/// </summary>
	public string ServiceUrl { get; set; } = "";
}
