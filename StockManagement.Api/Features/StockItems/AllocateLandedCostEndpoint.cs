using FastEndpoints;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using StockManagement.Auth.Core.Contracts;
using StockManagement.Kernel.Database.Interfaces;
using StockManagement.Kernel.Model;
using StockManagement.Kernel.Model.ExtensionMethods;
using StockManagement.Settings.Core.Contracts;

namespace StockManagement.Api.Features.StockItems;


/// <summary>
/// Freight, customs broker fee, and duties for one shipment - landed cost, split across its stock items (#120)
/// </summary>
public sealed record AllocateLandedCostRequest(IReadOnlyList<string> StockItemIds, decimal FreightCost = 0, decimal BrokerFee = 0, decimal DutiesCost = 0);


public sealed record NoPurchaseValueResponse(string Reason);


public sealed record LandedCostAllocationResponse(string StockItemId, decimal AllocatedCost, decimal AdditionalPurchaseCost, decimal Price);


public class AllocateLandedCostValidator : Validator<AllocateLandedCostRequest>
{
	public AllocateLandedCostValidator()
	{
		this.RuleFor(request => request.StockItemIds).NotEmpty().WithMessage("stockItemIdsRequired");
		this.RuleFor(request => request.FreightCost).GreaterThanOrEqualTo(0).WithMessage("freightCostNegative");
		this.RuleFor(request => request.BrokerFee).GreaterThanOrEqualTo(0).WithMessage("brokerFeeNegative");
		this.RuleFor(request => request.DutiesCost).GreaterThanOrEqualTo(0).WithMessage("dutiesCostNegative");
	}
}


/// <remarks>
/// Splits freight + broker fee + duties across the given stock items proportional to purchase value
/// (<see cref="StockItemExtensions.TryAllocateLandedCost"/>), overwriting each item's <see cref="StockItem.AdditionalPurchaseCost"/>
/// - no per-purchase history, same as the purchase fields themselves (ADR-0020) - and recalculating <see cref="StockItem.Price"/>
/// when <see cref="StockItem.Factor"/> &gt; 0.
/// </remarks>
public class AllocateLandedCostEndpoint(IStockItemServiceProvider stockItemServiceProvider, ISettingsService settingsService)
	: Endpoint<AllocateLandedCostRequest, Results<Ok<IReadOnlyList<LandedCostAllocationResponse>>, NotFound, BadRequest<NoPurchaseValueResponse>>>
{
	private readonly IStockItemServiceProvider _stockItemServiceProvider = stockItemServiceProvider;
	private readonly ISettingsService _settingsService = settingsService;


	public override void Configure()
	{
		this.Post("/stock-items/landed-cost/allocate");
		this.Permissions(Permission.StockItemsWrite);
	}

	public override async Task<Results<Ok<IReadOnlyList<LandedCostAllocationResponse>>, NotFound, BadRequest<NoPurchaseValueResponse>>> ExecuteAsync(AllocateLandedCostRequest request, CancellationToken cancellationToken)
	{
		var stockItems = new List<StockItem>(request.StockItemIds.Count);
		foreach (var id in request.StockItemIds.Distinct())
		{
			if (await _stockItemServiceProvider.GetStockItemByIdAsync(id, cancellationToken) is not StockItem stockItem) return TypedResults.NotFound();
			stockItems.Add(stockItem);
		}

		var companySettings = await _settingsService.GetCompanySettingsAsync(cancellationToken);
		var totalLandedCost = request.FreightCost + request.BrokerFee + request.DutiesCost;

		if (!StockItemExtensions.TryAllocateLandedCost(stockItems, totalLandedCost, companySettings.CurrencyDecimalDigits, out var allocations))
			return TypedResults.BadRequest(new NoPurchaseValueResponse("stockItemsHaveNoPurchaseValue"));

		var responses = new List<LandedCostAllocationResponse>(stockItems.Count);
		foreach (var allocation in allocations)
		{
			var stockItem = stockItems.First(item => item.Id == allocation.StockItemId);
			stockItem.AdditionalPurchaseCost = allocation.AdditionalPurchaseCostPerUnit;
			if (stockItem.Factor > 0)
				stockItem.Price = StockItemExtensions.CalculateSalePrice(stockItem.PurchasePrice, stockItem.PurchaseExchangeRate, stockItem.AdditionalPurchaseCost, stockItem.Factor, companySettings.CurrencyDecimalDigits);

			await _stockItemServiceProvider.UpdateStockItemAsync(stockItem, cancellationToken);
			responses.Add(new LandedCostAllocationResponse(stockItem.Id, allocation.AllocatedCost, stockItem.AdditionalPurchaseCost, stockItem.Price));
		}

		return TypedResults.Ok((IReadOnlyList<LandedCostAllocationResponse>)responses);
	}
}
