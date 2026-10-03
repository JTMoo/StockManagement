using FastEndpoints;
using Microsoft.AspNetCore.Http.HttpResults;
using StockManagement.Auth.Core.Contracts;
using StockManagement.Kernel.Database.Interfaces;
using StockManagement.Kernel.Model;

namespace StockManagement.Api.Features.RemissionNotes;


public sealed record GetRemissionNoteRequest(string Number);


public class GetRemissionNoteEndpoint(IRemissionNoteServiceProvider remissionNoteServiceProvider) : Endpoint<GetRemissionNoteRequest, Results<Ok<RemissionNoteResponse>, NotFound>>
{
	private readonly IRemissionNoteServiceProvider _remissionNoteServiceProvider = remissionNoteServiceProvider;


	public override void Configure()
	{
		this.Get("/remission-notes/{Number}");
		this.Permissions(Permission.SalesRead);
	}

	public override async Task<Results<Ok<RemissionNoteResponse>, NotFound>> ExecuteAsync(GetRemissionNoteRequest request, CancellationToken cancellationToken)
	{
		if (await _remissionNoteServiceProvider.GetRemissionNoteAsync(request.Number, cancellationToken) is not RemissionNote remissionNote) return TypedResults.NotFound();

		return TypedResults.Ok(RemissionNoteResponse.From(remissionNote));
	}
}
