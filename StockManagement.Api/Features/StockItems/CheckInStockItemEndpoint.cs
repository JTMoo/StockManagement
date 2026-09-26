using FastEndpoints;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using StockManagement.Kernel.Database.Interfaces;
using StockManagement.Kernel.Model;

namespace StockManagement.Api.Features.StockItems;


public sealed record CheckInStockItemRequest(string Id, int Amount, string Reason);


public class CheckInStockItemValidator : Validator<CheckInStockItemRequest>
{
	public CheckInStockItemValidator()
	{
		this.RuleFor(request => request.Amount).GreaterThan(0);
		this.RuleFor(request => request.Reason).NotEmpty();
	}
}


/// <remarks>Adds units to stock, recording the reason as a <see cref="Transaction"/> (#8)</remarks>
public class CheckInStockItemEndpoint(IStockItemServiceProvider stockItemServiceProvider) : Endpoint<CheckInStockItemRequest, Results<Ok<StockItemResponse>, NotFound>>
{
	private readonly IStockItemServiceProvider _stockItemServiceProvider = stockItemServiceProvider;


	public override void Configure()
	{
		this.Post("/stock-items/{Id}/check-in");
	}

	public override async Task<Results<Ok<StockItemResponse>, NotFound>> ExecuteAsync(CheckInStockItemRequest request, CancellationToken cancellationToken)
	{
		if (await _stockItemServiceProvider.GetStockItemByIdAsync(request.Id) is not StockItem stockItem) return TypedResults.NotFound();

		await _stockItemServiceProvider.CheckInStockItemAsync(stockItem, request.Amount, request.Reason);

		return TypedResults.Ok(StockItemResponse.From(stockItem));
	}
}
