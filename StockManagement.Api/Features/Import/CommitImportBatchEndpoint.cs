using FastEndpoints;
using Microsoft.AspNetCore.Http.HttpResults;
using StockManagement.Import.Core.Contracts;
using StockManagement.Kernel.Exceptions;

namespace StockManagement.Api.Features.Import;


public sealed record CommitImportBatchRequest(string Id);


public sealed record ImportBatchStatusConflictResponse(string Reason);


/// <remarks>Writes a previewed batch's ready rows to its target; a batch that is not Previewed (already committed, or undone) is a conflict.</remarks>
public class CommitImportBatchEndpoint(IImportBatchService importBatchService) : Endpoint<CommitImportBatchRequest, Results<Ok<ImportBatchResponse>, NotFound, Conflict<ImportBatchStatusConflictResponse>>>
{
	private readonly IImportBatchService _importBatchService = importBatchService;


	public override void Configure()
	{
		this.Post("/import/batches/{Id}/commit");
	}

	public override async Task<Results<Ok<ImportBatchResponse>, NotFound, Conflict<ImportBatchStatusConflictResponse>>> ExecuteAsync(CommitImportBatchRequest request, CancellationToken cancellationToken)
	{
		try
		{
			var batch = await _importBatchService.CommitAsync(request.Id, cancellationToken);
			return TypedResults.Ok(ImportBatchResponse.From(batch));
		}
		catch (ImportBatchNotFoundException)
		{
			return TypedResults.NotFound();
		}
		catch (InvalidImportBatchStatusException ex)
		{
			return TypedResults.Conflict(new ImportBatchStatusConflictResponse(ex.Message));
		}
	}
}
