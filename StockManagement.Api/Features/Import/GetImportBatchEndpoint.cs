using FastEndpoints;
using Microsoft.AspNetCore.Http.HttpResults;
using StockManagement.Auth.Core.Contracts;
using StockManagement.Import.Core.Contracts;
using StockManagement.Kernel.Model;

namespace StockManagement.Api.Features.Import;


public sealed record GetImportBatchRequest(string Id);


public class GetImportBatchEndpoint(IImportBatchService importBatchService) : Endpoint<GetImportBatchRequest, Results<Ok<ImportBatchResponse>, NotFound>>
{
	private readonly IImportBatchService _importBatchService = importBatchService;


	public override void Configure()
	{
		this.Get("/import/batches/{Id}");
		this.Permissions(Permission.StockItemsRead, Permission.CustomersRead);
	}

	public override async Task<Results<Ok<ImportBatchResponse>, NotFound>> ExecuteAsync(GetImportBatchRequest request, CancellationToken cancellationToken)
	{
		if (await _importBatchService.GetAsync(request.Id, cancellationToken) is not ImportBatch batch) return TypedResults.NotFound();

		return TypedResults.Ok(ImportBatchResponse.From(batch));
	}
}
