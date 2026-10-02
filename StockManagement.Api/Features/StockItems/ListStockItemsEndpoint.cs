using FastEndpoints;
using StockManagement.Auth.Core.Contracts;
using StockManagement.Kernel.Database.Interfaces;

namespace StockManagement.Api.Features.StockItems;


public sealed record ListStockItemsRequest(string? Cursor, int? PageSize);


/// <param name="NextCursor">Opaque cursor for the next page; <see langword="null"/> on the last page</param>
public sealed record StockItemListResponse(IReadOnlyList<StockItemResponse> Items, string? NextCursor);


public class ListStockItemsEndpoint(IStockItemServiceProvider stockItemServiceProvider) : Endpoint<ListStockItemsRequest, StockItemListResponse>
{
	private readonly IStockItemServiceProvider _stockItemServiceProvider = stockItemServiceProvider;


	public override void Configure()
	{
		this.Get("/stock-items");
		this.Permissions(Permission.StockItemsRead);
	}

	public override async Task<StockItemListResponse> ExecuteAsync(ListStockItemsRequest request, CancellationToken cancellationToken)
	{
		var pageSize = Math.Clamp(request.PageSize ?? 20, 1, 100);
		var result = await _stockItemServiceProvider.GetStockItemsAsync(request.Cursor, pageSize, cancellationToken);
		return new(result.Items.Select(StockItemResponse.From).ToList(), result.NextCursor);
	}
}
