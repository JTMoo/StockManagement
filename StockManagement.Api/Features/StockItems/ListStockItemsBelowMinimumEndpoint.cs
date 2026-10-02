using FastEndpoints;
using StockManagement.Auth.Core.Contracts;
using StockManagement.Kernel.Database.Interfaces;

namespace StockManagement.Api.Features.StockItems;


/// <remarks>Reorder candidates (#57): items with a minimum stock set and fewer units on hand</remarks>
public class ListStockItemsBelowMinimumEndpoint(IStockItemServiceProvider stockItemServiceProvider) : EndpointWithoutRequest<IReadOnlyList<StockItemResponse>>
{
	private readonly IStockItemServiceProvider _stockItemServiceProvider = stockItemServiceProvider;


	public override void Configure()
	{
		this.Get("/stock-items/below-minimum");
		this.Permissions(Permission.StockItemsRead);
	}

	public override async Task<IReadOnlyList<StockItemResponse>> ExecuteAsync(CancellationToken cancellationToken)
	{
		var stockItems = await _stockItemServiceProvider.GetStockItemsBelowMinimumAsync(cancellationToken) ?? [];
		return stockItems.Select(StockItemResponse.From).ToList();
	}
}
