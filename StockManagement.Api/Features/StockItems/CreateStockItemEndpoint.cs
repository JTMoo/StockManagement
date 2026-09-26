using FastEndpoints;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using StockManagement.Auth.Core.Contracts;
using StockManagement.Kernel.Database.Interfaces;
using StockManagement.Kernel.Exceptions;
using StockManagement.Kernel.Model;

namespace StockManagement.Api.Features.StockItems;


public sealed record CreateStockItemRequest(string Code, string Name, string Description = "", string Location = "", int Amount = 0, decimal Price = 0, string Manufacturer = "");


public class CreateStockItemValidator : Validator<CreateStockItemRequest>
{
	public CreateStockItemValidator()
	{
		this.RuleFor(request => request.Code).NotEmpty();
		this.RuleFor(request => request.Name).NotEmpty();
		this.RuleFor(request => request.Amount).GreaterThanOrEqualTo(0);
		this.RuleFor(request => request.Price).GreaterThanOrEqualTo(0);
	}
}


public sealed record DuplicateStockItemCodeResponse(string Code);


public class CreateStockItemEndpoint(IStockItemServiceProvider stockItemServiceProvider) : Endpoint<CreateStockItemRequest, Results<Created<StockItemResponse>, Conflict<DuplicateStockItemCodeResponse>>>
{
	private readonly IStockItemServiceProvider _stockItemServiceProvider = stockItemServiceProvider;


	public override void Configure()
	{
		this.Post("/stock-items");
		this.Permissions(Permission.StockItemsWrite);
	}

	public override async Task<Results<Created<StockItemResponse>, Conflict<DuplicateStockItemCodeResponse>>> ExecuteAsync(CreateStockItemRequest request, CancellationToken cancellationToken)
	{
		var stockItem = new StockItem(request.Name, code: request.Code, description: request.Description, amount: request.Amount, manufacturer: request.Manufacturer)
		{
			Location = request.Location,
			Price = (double)request.Price
		};

		try
		{
			await _stockItemServiceProvider.AddStockItemAsync(stockItem);
		}
		catch (StockItemCodeAlreadyExistsException)
		{
			return TypedResults.Conflict(new DuplicateStockItemCodeResponse(request.Code));
		}

		return TypedResults.Created($"/api/stock-items/{stockItem.Code}", StockItemResponse.From(stockItem));
	}
}
