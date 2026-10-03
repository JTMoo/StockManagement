using FastEndpoints;
using StockManagement.Auth.Core.Contracts;
using StockManagement.Kernel.Database.Interfaces;

namespace StockManagement.Api.Features.RemissionNotes;


public sealed record RemissionNoteListResponse(IReadOnlyList<RemissionNoteResponse> Items);


public class ListRemissionNotesEndpoint(IRemissionNoteServiceProvider remissionNoteServiceProvider) : EndpointWithoutRequest<RemissionNoteListResponse>
{
	private readonly IRemissionNoteServiceProvider _remissionNoteServiceProvider = remissionNoteServiceProvider;


	public override void Configure()
	{
		this.Get("/remission-notes");
		this.Permissions(Permission.SalesRead);
	}

	public override async Task<RemissionNoteListResponse> ExecuteAsync(CancellationToken cancellationToken)
	{
		var remissionNotes = await _remissionNoteServiceProvider.GetRemissionNotesAsync(cancellationToken);
		return new(remissionNotes.Select(RemissionNoteResponse.From).ToList());
	}
}
