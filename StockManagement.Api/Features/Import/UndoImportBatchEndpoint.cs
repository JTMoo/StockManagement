using FastEndpoints;
using Microsoft.AspNetCore.Http.HttpResults;
using StockManagement.Auth.Core.Contracts;
using StockManagement.Import.Core.Contracts;
using StockManagement.Kernel.Exceptions;

namespace StockManagement.Api.Features.Import;


public sealed record UndoImportBatchRequest(string Id);


/// <remarks>Removes the entities a commit created; a batch that is not Committed (still Previewed, or already undone) is a conflict.</remarks>
public class UndoImportBatchEndpoint(IImportBatchService importBatchService) : Endpoint<UndoImportBatchRequest, Results<Ok<ImportBatchResponse>, NotFound, Conflict<ImportBatchStatusConflictResponse>>>
{
	private readonly IImportBatchService _importBatchService = importBatchService;


	public override void Configure()
	{
		this.Post("/import/batches/{Id}/undo");
		this.Permissions(Permission.StockItemsWrite, Permission.CustomersWrite);
	}

	public override async Task<Results<Ok<ImportBatchResponse>, NotFound, Conflict<ImportBatchStatusConflictResponse>>> ExecuteAsync(UndoImportBatchRequest request, CancellationToken cancellationToken)
	{
		try
		{
			var batch = await _importBatchService.UndoAsync(request.Id, cancellationToken);
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
