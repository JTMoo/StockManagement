using FastEndpoints;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using StockManagement.Kernel.Database.Interfaces;
using StockManagement.Kernel.Model;

namespace StockManagement.Api.Features.StockItems;


public sealed record CheckOutStockItemRequest(string Id, int Amount, string Reason);


public class CheckOutStockItemValidator : Validator<CheckOutStockItemRequest>
{
	public CheckOutStockItemValidator()
	{
		this.RuleFor(request => request.Amount).GreaterThan(0);
		this.RuleFor(request => request.Reason).NotEmpty();
	}
}


public sealed record InsufficientStockResponse(int InStock);


/// <remarks>Removes units from stock, recording the reason as a <see cref="Transaction"/> (#8)</remarks>
public class CheckOutStockItemEndpoint(IStockItemServiceProvider stockItemServiceProvider) : Endpoint<CheckOutStockItemRequest, Results<Ok<StockItemResponse>, NotFound, Conflict<InsufficientStockResponse>>>
{
	private readonly IStockItemServiceProvider _stockItemServiceProvider = stockItemServiceProvider;


	public override void Configure()
	{
		this.Post("/stock-items/{Id}/check-out");
	}

	public override async Task<Results<Ok<StockItemResponse>, NotFound, Conflict<InsufficientStockResponse>>> ExecuteAsync(CheckOutStockItemRequest request, CancellationToken cancellationToken)
	{
		if (await _stockItemServiceProvider.GetStockItemByIdAsync(request.Id) is not StockItem stockItem) return TypedResults.NotFound();

		if (!await _stockItemServiceProvider.TryCheckOutStockItemAsync(stockItem, request.Amount, request.Reason))
		{
			return TypedResults.Conflict(new InsufficientStockResponse(stockItem.Amount));
		}

		return TypedResults.Ok(StockItemResponse.From(stockItem));
	}
}
