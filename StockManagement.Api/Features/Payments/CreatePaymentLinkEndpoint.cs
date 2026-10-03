using FastEndpoints;
using Microsoft.AspNetCore.Http.HttpResults;
using StockManagement.Auth.Core.Contracts;
using StockManagement.Sales.Core.Contracts;

namespace StockManagement.Api.Features.Payments;


public sealed record CreatePaymentLinkRequest(string Number);


/// <param name="Reason">A resource key, e.g. <c>invoiceAlreadyPaid</c> or <c>paymentLinkGatewayError</c></param>
public sealed record PaymentLinkRejectedResponse(string Reason);


/// <remarks>Rejects with 409 when the invoice has no amount due, or Bancard rejected the request.</remarks>
public class CreatePaymentLinkEndpoint(IPaymentLinkService paymentLinkService) : Endpoint<CreatePaymentLinkRequest, Results<Created<PaymentLinkResponse>, NotFound, Conflict<PaymentLinkRejectedResponse>>>
{
	private readonly IPaymentLinkService _paymentLinkService = paymentLinkService;


	public override void Configure()
	{
		this.Post("/invoices/{Number}/payment-link");
		this.Permissions(Permission.SalesWrite);
	}

	public override async Task<Results<Created<PaymentLinkResponse>, NotFound, Conflict<PaymentLinkRejectedResponse>>> ExecuteAsync(CreatePaymentLinkRequest request, CancellationToken cancellationToken)
	{
		var result = await _paymentLinkService.CreateForInvoiceAsync(request.Number, cancellationToken);
		if (!result.Succeeded)
		{
			return result.Error switch
			{
				CreatePaymentLinkError.InvoiceNotFound => TypedResults.NotFound(),
				CreatePaymentLinkError.AlreadyPaid => TypedResults.Conflict(new PaymentLinkRejectedResponse("invoiceAlreadyPaid")),
				_ => TypedResults.Conflict(new PaymentLinkRejectedResponse("paymentLinkGatewayError"))
			};
		}

		return TypedResults.Created($"/api/invoices/{request.Number}/payment-link", PaymentLinkResponse.From(result.Link!));
	}
}
