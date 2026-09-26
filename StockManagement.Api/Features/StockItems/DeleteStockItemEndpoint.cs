using FastEndpoints;
using Microsoft.AspNetCore.Http.HttpResults;
using StockManagement.Auth.Core.Contracts;
using StockManagement.Kernel.Database.Interfaces;
using StockManagement.Kernel.Model;

namespace StockManagement.Api.Features.StockItems;


public sealed record DeleteStockItemRequest(string Id);


/// <remarks>Matches the stored item by <c>Id</c> (docs/decisions.md: update by Id, never by business key).</remarks>
public class DeleteStockItemEndpoint(IStockItemServiceProvider stockItemServiceProvider) : Endpoint<DeleteStockItemRequest, Results<NoContent, NotFound>>
{
	private readonly IStockItemServiceProvider _stockItemServiceProvider = stockItemServiceProvider;


	public override void Configure()
	{
		this.Delete("/stock-items/{Id}");
		this.Permissions(Permission.StockItemsWrite);
	}

	public override async Task<Results<NoContent, NotFound>> ExecuteAsync(DeleteStockItemRequest request, CancellationToken cancellationToken)
	{
		if (await _stockItemServiceProvider.GetStockItemByIdAsync(request.Id) is not StockItem stockItem) return TypedResults.NotFound();

		await _stockItemServiceProvider.DeleteStockItemAsync(stockItem);
		return TypedResults.NoContent();
	}
}
