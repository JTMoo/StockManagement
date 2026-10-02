using FastEndpoints;
using Microsoft.AspNetCore.Http.HttpResults;
using StockManagement.Auth.Core.Contracts;
using StockManagement.Kernel.Database.Interfaces;
using StockManagement.Kernel.Model;
using StockManagement.Sales.Core.Contracts;

namespace StockManagement.Api.Features.Invoices;


public sealed record GetInvoiceRequest(string Number);


public class GetInvoiceEndpoint(IInvoiceServiceProvider invoiceServiceProvider, IPaymentService paymentService) : Endpoint<GetInvoiceRequest, Results<Ok<InvoiceResponse>, NotFound>>
{
	private readonly IInvoiceServiceProvider _invoiceServiceProvider = invoiceServiceProvider;
	private readonly IPaymentService _paymentService = paymentService;


	public override void Configure()
	{
		this.Get("/invoices/{Number}");
		this.Permissions(Permission.SalesRead);
	}

	public override async Task<Results<Ok<InvoiceResponse>, NotFound>> ExecuteAsync(GetInvoiceRequest request, CancellationToken cancellationToken)
	{
		if (await _invoiceServiceProvider.GetInvoiceAync(request.Number) is not Invoice invoice) return TypedResults.NotFound();

		return TypedResults.Ok(InvoiceResponse.From(invoice, _paymentService));
	}
}
