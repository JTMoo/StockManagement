using FastEndpoints;
using Microsoft.AspNetCore.Http.HttpResults;
using StockManagement.Kernel.Database.Interfaces;
using StockManagement.Kernel.Model;

namespace StockManagement.Api.Features.StockItems;


public sealed record GetStockItemRequest(string Code);


public class GetStockItemEndpoint(IStockItemServiceProvider stockItemServiceProvider) : Endpoint<GetStockItemRequest, Results<Ok<StockItemResponse>, NotFound>>
{
	private readonly IStockItemServiceProvider _stockItemServiceProvider = stockItemServiceProvider;


	public override void Configure()
	{
		this.Get("/stock-items/{Code}");
		this.AllowAnonymous();
	}

	public override async Task<Results<Ok<StockItemResponse>, NotFound>> ExecuteAsync(GetStockItemRequest request, CancellationToken cancellationToken)
	{
		if (await _stockItemServiceProvider.GetStockItemAsync(request.Code) is not StockItem stockItem) return TypedResults.NotFound();

		return TypedResults.Ok(StockItemResponse.From(stockItem));
	}
}
