namespace StockManagement.Sales.Core;


/// <summary>
/// Bancard vPOS/QR config (#150). Sandbox vs. production is picked by <see cref="BaseUrl"/> per environment config,
/// never hardcoded; keys never go in source control.
/// </summary>
public sealed class BancardGatewayOptions
{
	public const string SectionName = "Bancard";

	public string PublicKey { get; set; } = "";

	public string PrivateKey { get; set; } = "";

	/// <summary>Bancard vPOS API base URL (sandbox or production)</summary>
	public string BaseUrl { get; set; } = "";

	public string Currency { get; set; } = "PYG";
}
