using FastEndpoints;
using Microsoft.AspNetCore.Http.HttpResults;
using StockManagement.Auth.Core.Contracts;
using StockManagement.Sales.Core.Contracts;

namespace StockManagement.Api.Features.Payments;


public sealed record GetPaymentLinkRequest(string Number);


/// <summary>Latest Bancard payment link created for the invoice, if any</summary>
public class GetPaymentLinkEndpoint(IPaymentLinkService paymentLinkService) : Endpoint<GetPaymentLinkRequest, Results<Ok<PaymentLinkResponse>, NotFound>>
{
	private readonly IPaymentLinkService _paymentLinkService = paymentLinkService;


	public override void Configure()
	{
		this.Get("/invoices/{Number}/payment-link");
		this.Permissions(Permission.SalesRead);
	}

	public override async Task<Results<Ok<PaymentLinkResponse>, NotFound>> ExecuteAsync(GetPaymentLinkRequest request, CancellationToken cancellationToken)
	{
		var link = await _paymentLinkService.GetLatestForInvoiceAsync(request.Number, cancellationToken);
		return link is null ? TypedResults.NotFound() : TypedResults.Ok(PaymentLinkResponse.From(link));
	}
}
