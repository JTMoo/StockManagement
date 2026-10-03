using FastEndpoints;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using StockManagement.Auth.Core.Contracts;
using StockManagement.Kernel.Database.Interfaces;
using StockManagement.Kernel.Model;
using StockManagement.Kernel.Model.Types;
using StockManagement.Sales.Core.Contracts;

namespace StockManagement.Api.Features.RemissionNotes;


public sealed record CreateRemissionNoteRequest(int CustomerId, RemissionReason Reason, string DestinationAddress, IReadOnlyList<RemissionNoteItemRequest> Items);


public sealed record RemissionNoteItemRequest(string Code, int Amount);


public class CreateRemissionNoteValidator : Validator<CreateRemissionNoteRequest>
{
	public CreateRemissionNoteValidator()
	{
		this.RuleFor(request => request.Reason).IsInEnum().WithMessage("reasonInvalid");
		this.RuleFor(request => request.DestinationAddress).NotEmpty().WithMessage("destinationAddressRequired");
		this.RuleFor(request => request.Items).NotEmpty().WithMessage("itemsRequired");
		this.RuleForEach(request => request.Items).ChildRules(item =>
		{
			item.RuleFor(line => line.Code).NotEmpty().WithMessage("codeRequired");
			item.RuleFor(line => line.Amount).GreaterThan(0).WithMessage("amountNotPositive");
		});
	}
}


/// <remarks>
/// Does not touch stock amounts - a remission note documents a movement, it is not a sale (#162). Items that do
/// not exist as a <see cref="StockItem"/> code fail validation the same way an unknown customer id does.
/// </remarks>
public class CreateRemissionNoteEndpoint(IRemissionNoteService remissionNoteService, ICustomerServiceProvider customerServiceProvider, IStockItemServiceProvider stockItemServiceProvider)
	: Endpoint<CreateRemissionNoteRequest, Results<Created<RemissionNoteResponse>, NotFound>>
{
	public const string CustomerNotFound = "customerNotFound";
	public const string StockItemNotFound = "stockItemNotFound";

	private readonly IRemissionNoteService _remissionNoteService = remissionNoteService;
	private readonly ICustomerServiceProvider _customerServiceProvider = customerServiceProvider;
	private readonly IStockItemServiceProvider _stockItemServiceProvider = stockItemServiceProvider;


	public override void Configure()
	{
		this.Post("/remission-notes");
		this.Permissions(Permission.SalesWrite);
	}

	public override async Task<Results<Created<RemissionNoteResponse>, NotFound>> ExecuteAsync(CreateRemissionNoteRequest request, CancellationToken cancellationToken)
	{
		if (await _customerServiceProvider.GetCustomerAsync(request.CustomerId, cancellationToken) is not Customer customer) return TypedResults.NotFound();

		List<(StockItem StockItem, int Amount)> items = [];
		foreach (var line in request.Items)
		{
			if (await _stockItemServiceProvider.GetStockItemAsync(line.Code, cancellationToken) is not StockItem stockItem) return TypedResults.NotFound();
			items.Add((stockItem, line.Amount));
		}

		var remissionNote = await _remissionNoteService.CreateAsync(customer, items, request.Reason, request.DestinationAddress, DateTime.Now, cancellationToken);
		return TypedResults.Created($"/api/remission-notes/{remissionNote.Number}", RemissionNoteResponse.From(remissionNote));
	}
}
