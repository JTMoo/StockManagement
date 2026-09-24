using FastEndpoints;
using StockManagement.Kernel.Database.Interfaces;

namespace StockManagement.Api.Features.StockItems;


public class ListStockItemsEndpoint(IStockItemServiceProvider stockItemServiceProvider) : EndpointWithoutRequest<IReadOnlyList<StockItemResponse>>
{
	private readonly IStockItemServiceProvider _stockItemServiceProvider = stockItemServiceProvider;


	public override void Configure()
	{
		this.Get("/stock-items");
		this.AllowAnonymous();
	}

	public override async Task<IReadOnlyList<StockItemResponse>> ExecuteAsync(CancellationToken cancellationToken)
	{
		var stockItems = await _stockItemServiceProvider.GetAllStockItemsAsync() ?? [];
		return stockItems.Select(StockItemResponse.From).ToList();
	}
}
