using FastEndpoints;
using StockManagement.Auth.Core.Contracts;
using StockManagement.Kernel.Database.Interfaces;

namespace StockManagement.Api.Features.StockItems;


public sealed record ListStockItemsBelowMinimumRequest(string? Cursor, int? PageSize);


/// <remarks>Reorder candidates (#57): items with a minimum stock set and fewer units on hand</remarks>
public class ListStockItemsBelowMinimumEndpoint(IStockItemServiceProvider stockItemServiceProvider) : Endpoint<ListStockItemsBelowMinimumRequest, StockItemListResponse>
{
	private readonly IStockItemServiceProvider _stockItemServiceProvider = stockItemServiceProvider;


	public override void Configure()
	{
		this.Get("/stock-items/below-minimum");
		this.Permissions(Permission.StockItemsRead);
	}

	public override async Task<StockItemListResponse> ExecuteAsync(ListStockItemsBelowMinimumRequest request, CancellationToken cancellationToken)
	{
		var pageSize = Math.Clamp(request.PageSize ?? 20, 1, 100);
		var result = await _stockItemServiceProvider.GetStockItemsBelowMinimumAsync(request.Cursor, pageSize, cancellationToken);
		return new(result.Items.Select(StockItemResponse.From).ToList(), result.NextCursor);
	}
}
