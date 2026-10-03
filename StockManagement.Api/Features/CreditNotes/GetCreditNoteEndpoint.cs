using FastEndpoints;
using Microsoft.AspNetCore.Http.HttpResults;
using StockManagement.Auth.Core.Contracts;
using StockManagement.Kernel.Database.Interfaces;
using StockManagement.Kernel.Model;

namespace StockManagement.Api.Features.CreditNotes;


public sealed record GetCreditNoteRequest(int Number);


public class GetCreditNoteEndpoint(ICreditNoteServiceProvider creditNoteServiceProvider) : Endpoint<GetCreditNoteRequest, Results<Ok<CreditNoteResponse>, NotFound>>
{
	private readonly ICreditNoteServiceProvider _creditNoteServiceProvider = creditNoteServiceProvider;


	public override void Configure()
	{
		this.Get("/credit-notes/{Number}");
		this.Permissions(Permission.SalesRead);
	}

	public override async Task<Results<Ok<CreditNoteResponse>, NotFound>> ExecuteAsync(GetCreditNoteRequest request, CancellationToken cancellationToken)
	{
		if (await _creditNoteServiceProvider.GetCreditNoteAsync(request.Number, cancellationToken) is not CreditNote creditNote) return TypedResults.NotFound();

		return TypedResults.Ok(CreditNoteResponse.From(creditNote));
	}
}
