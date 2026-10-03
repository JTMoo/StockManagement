namespace StockManagement.Sales.Core.Contracts;


/// <summary>
/// Outcome of <see cref="IPaymentLinkGateway.CreateAsync"/>.
/// </summary>
public sealed record PaymentLinkGatewayResult(bool Succeeded, string? ExternalId = null, string? QrUrl = null, string? Error = null)
{
	public static PaymentLinkGatewayResult Success(string externalId, string qrUrl)
	{
		return new(true, externalId, qrUrl);
	}

	public static PaymentLinkGatewayResult Failure(string error)
	{
		return new(false, Error: error);
	}
}
