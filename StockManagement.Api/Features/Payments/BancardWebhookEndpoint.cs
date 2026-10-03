using FastEndpoints;
using Microsoft.AspNetCore.Http.HttpResults;
using StockManagement.Sales.Core.Contracts;

namespace StockManagement.Api.Features.Payments;


/// <summary>Bancard's <c>shop_process_id</c> is the invoice number (<see cref="PaymentLinkService.CreateForInvoiceAsync"/>)</summary>
public sealed record BancardWebhookRequest(string ShopProcessId);


/// <remarks>
/// Unauthenticated per Bancard's webhook contract - re-verifies the status against Bancard itself
/// (<see cref="IPaymentLinkGateway.GetStatusAsync"/>) rather than trusting this payload, so a spoofed call can only
/// trigger a redundant, harmless status re-check.
/// </remarks>
public class BancardWebhookEndpoint(IPaymentLinkService paymentLinkService) : Endpoint<BancardWebhookRequest, Results<Ok, NotFound>>
{
	private readonly IPaymentLinkService _paymentLinkService = paymentLinkService;


	public override void Configure()
	{
		this.Post("/webhooks/bancard");
		this.AllowAnonymous();
	}

	public override async Task<Results<Ok, NotFound>> ExecuteAsync(BancardWebhookRequest request, CancellationToken cancellationToken)
	{
		var confirmed = await _paymentLinkService.ConfirmAsync(request.ShopProcessId, cancellationToken);
		return confirmed ? TypedResults.Ok() : TypedResults.NotFound();
	}
}
