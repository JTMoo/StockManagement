using FastEndpoints;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using StockManagement.Api.Features.Invoices;
using StockManagement.Auth.Core.Contracts;
using StockManagement.Kernel.Database.Interfaces;
using StockManagement.Kernel.Model;
using StockManagement.Kernel.Model.Types;
using StockManagement.Sales.Core.Contracts;

namespace StockManagement.Api.Features.Sales;


public sealed record CreateSaleRequest(int CustomerId, SaleCondition SaleCondition, IReadOnlyList<SaleItemRequest> Items);


public sealed record SaleItemRequest(string Code, int Amount);


/// <summary>
/// Articles that blocked the sale
/// </summary>
/// <param name="UnavailableItems">Names of articles with too little stock, or codes that do not exist</param>
public sealed record SaleConflictResponse(IReadOnlyList<string> UnavailableItems);


public class CreateSaleValidator : Validator<CreateSaleRequest>
{
	public CreateSaleValidator()
	{
		this.RuleFor(request => request.SaleCondition).IsInEnum().NotEqual(SaleCondition.None);
		this.RuleFor(request => request.Items).NotEmpty();
		this.RuleForEach(request => request.Items).ChildRules(item =>
		{
			item.RuleFor(line => line.Code).NotEmpty();
			item.RuleFor(line => line.Amount).GreaterThan(0);
		});
	}
}


/// <remarks>Takes the units out of stock and stores the invoice; 409 when any article is unavailable.</remarks>
public class CreateSaleEndpoint(ISaleService saleService, ICustomerServiceProvider customerServiceProvider) : Endpoint<CreateSaleRequest, Results<Created<InvoiceResponse>, Conflict<SaleConflictResponse>>>
{
	public const string CustomerNotFound = "customerNotFound";

	private readonly ISaleService _saleService = saleService;
	private readonly ICustomerServiceProvider _customerServiceProvider = customerServiceProvider;


	public override void Configure()
	{
		this.Post("/sales");
		this.Permissions(Permission.SalesWrite);
	}

	public override async Task<Results<Created<InvoiceResponse>, Conflict<SaleConflictResponse>>> ExecuteAsync(CreateSaleRequest request, CancellationToken cancellationToken)
	{
		var customer = await _customerServiceProvider.GetCustomerAsync(request.CustomerId);
		if (customer is null) this.ThrowError(request => request.CustomerId, CustomerNotFound);

		var items = request.Items.Select(item => new SaleItem(item.Code, item.Amount)).ToList();
		var result = await _saleService.SellAsync(customer, items, request.SaleCondition, DateTime.Now, cancellationToken);
		if (!result.Succeeded || result.Invoice is not Invoice invoice) return TypedResults.Conflict(new SaleConflictResponse(result.UnavailableItems));

		return TypedResults.Created($"/api/invoices/{invoice.Number}", InvoiceResponse.From(invoice));
	}
}
