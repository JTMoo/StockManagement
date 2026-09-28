using FastEndpoints;
using Microsoft.AspNetCore.Http.HttpResults;
using StockManagement.Auth.Core.Contracts;
using StockManagement.Import.Core.Contracts;
using StockManagement.Kernel.Model.Types;

namespace StockManagement.Api.Features.Import;


public sealed record GetImportFieldsRequest(ImportTarget Target);


public class GetImportFieldsEndpoint(IImportBatchService importBatchService) : Endpoint<GetImportFieldsRequest, Ok<IReadOnlyList<ImportField>>>
{
	private readonly IImportBatchService _importBatchService = importBatchService;


	public override void Configure()
	{
		this.Get("/import/fields");
		this.Permissions(Permission.StockItemsWrite, Permission.CustomersWrite);
	}

	public override Task<Ok<IReadOnlyList<ImportField>>> ExecuteAsync(GetImportFieldsRequest request, CancellationToken cancellationToken)
	{
		return Task.FromResult(TypedResults.Ok(_importBatchService.GetFields(request.Target)));
	}
}
