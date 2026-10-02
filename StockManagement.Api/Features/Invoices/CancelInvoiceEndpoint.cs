using FastEndpoints;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using StockManagement.Api.Features.CreditNotes;
using StockManagement.Auth.Core.Contracts;
using StockManagement.Kernel.Database.Interfaces;
using StockManagement.Kernel.Model;
using StockManagement.Sales.Core.Contracts;

namespace StockManagement.Api.Features.Invoices;


public sealed record CancelInvoiceRequest(int Number, string Reason);


public sealed record InvoiceAlreadyCancelledResponse(string Reason);


public class CancelInvoiceValidator : Validator<CancelInvoiceRequest>
{
	public CancelInvoiceValidator()
	{
		this.RuleFor(request => request.Reason).NotEmpty().WithMessage("reasonRequired");
	}
}


/// <remarks>Restocks every line and stores a credit note referencing the invoice; the invoice itself is kept, only marked cancelled (#56)</remarks>
public class CancelInvoiceEndpoint(IInvoiceServiceProvider invoiceServiceProvider, ICreditNoteService creditNoteService)
	: Endpoint<CancelInvoiceRequest, Results<Ok<CreditNoteResponse>, NotFound, Conflict<InvoiceAlreadyCancelledResponse>>>
{
	private readonly IInvoiceServiceProvider _invoiceServiceProvider = invoiceServiceProvider;
	private readonly ICreditNoteService _creditNoteService = creditNoteService;


	public override void Configure()
	{
		this.Post("/invoices/{Number}/cancel");
		this.Permissions(Permission.SalesWrite);
	}

	public override async Task<Results<Ok<CreditNoteResponse>, NotFound, Conflict<InvoiceAlreadyCancelledResponse>>> ExecuteAsync(CancelInvoiceRequest request, CancellationToken cancellationToken)
	{
		if (await _invoiceServiceProvider.GetInvoiceAync(request.Number) is not Invoice invoice) return TypedResults.NotFound();

		var result = await _creditNoteService.CancelInvoiceAsync(invoice, request.Reason, DateTime.Now, cancellationToken);
		if (!result.Succeeded || result.CreditNote is not CreditNote creditNote) return TypedResults.Conflict(new InvoiceAlreadyCancelledResponse("Invoice is already cancelled."));

		return TypedResults.Ok(CreditNoteResponse.From(creditNote));
	}
}
