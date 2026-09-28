using FastEndpoints;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using StockManagement.Auth.Core.Contracts;
using StockManagement.Kernel.Database.Interfaces;
using StockManagement.Kernel.Exceptions;
using StockManagement.Kernel.Model;

namespace StockManagement.Api.Features.StockItems;


public sealed record UpdateStockItemRequest(string Id, string Code, string Name, string Description = "", string Location = "", int Amount = 0, decimal Price = 0, string Manufacturer = "", string? SupplierId = null, int MinimumStock = 0);


public class UpdateStockItemValidator : Validator<UpdateStockItemRequest>
{
	public UpdateStockItemValidator()
	{
		this.RuleFor(request => request.Code).NotEmpty();
		this.RuleFor(request => request.Name).NotEmpty();
		this.RuleFor(request => request.Amount).GreaterThanOrEqualTo(0);
		this.RuleFor(request => request.Price).GreaterThanOrEqualTo(0);
		this.RuleFor(request => request.MinimumStock).GreaterThanOrEqualTo(0);
	}
}


/// <remarks>Matches the stored item by <c>Id</c> (docs/decisions.md: update by Id, never by business key), so Code can be renamed.</remarks>
public class UpdateStockItemEndpoint(IStockItemServiceProvider stockItemServiceProvider) : Endpoint<UpdateStockItemRequest, Results<Ok<StockItemResponse>, NotFound, Conflict<DuplicateStockItemCodeResponse>>>
{
	private readonly IStockItemServiceProvider _stockItemServiceProvider = stockItemServiceProvider;


	public override void Configure()
	{
		this.Put("/stock-items/{Id}");
		this.Permissions(Permission.StockItemsWrite);
	}

	public override async Task<Results<Ok<StockItemResponse>, NotFound, Conflict<DuplicateStockItemCodeResponse>>> ExecuteAsync(UpdateStockItemRequest request, CancellationToken cancellationToken)
	{
		if (await _stockItemServiceProvider.GetStockItemByIdAsync(request.Id) is not StockItem stockItem) return TypedResults.NotFound();

		stockItem.Code = request.Code;
		stockItem.Name = request.Name;
		stockItem.Description = request.Description;
		stockItem.Location = request.Location;
		stockItem.Amount = request.Amount;
		stockItem.Price = request.Price;
		stockItem.Manufacturer = request.Manufacturer;
		stockItem.SupplierId = request.SupplierId;
		stockItem.MinimumStock = request.MinimumStock;

		try
		{
			await _stockItemServiceProvider.UpdateStockItemAsync(stockItem);
		}
		catch (StockItemCodeAlreadyExistsException)
		{
			return TypedResults.Conflict(new DuplicateStockItemCodeResponse(request.Code));
		}

		// Reload: stockItem.Supplier may still reference the old row after a SupplierId change
		var updated = await _stockItemServiceProvider.GetStockItemByIdAsync(stockItem.Id);
		return TypedResults.Ok(StockItemResponse.From(updated));
	}
}
