using FastEndpoints;
using Microsoft.AspNetCore.Http.HttpResults;
using StockManagement.Kernel.Database.Interfaces;
using StockManagement.Kernel.Model;

namespace StockManagement.Api.Features.Invoices;


public sealed record GetInvoiceRequest(int Number);


public class GetInvoiceEndpoint(IInvoiceServiceProvider invoiceServiceProvider) : Endpoint<GetInvoiceRequest, Results<Ok<InvoiceResponse>, NotFound>>
{
	private readonly IInvoiceServiceProvider _invoiceServiceProvider = invoiceServiceProvider;


	public override void Configure()
	{
		this.Get("/invoices/{Number}");
		this.AllowAnonymous();
	}

	public override async Task<Results<Ok<InvoiceResponse>, NotFound>> ExecuteAsync(GetInvoiceRequest request, CancellationToken cancellationToken)
	{
		if (await _invoiceServiceProvider.GetInvoiceAync(request.Number) is not Invoice invoice) return TypedResults.NotFound();

		return TypedResults.Ok(InvoiceResponse.From(invoice));
	}
}
