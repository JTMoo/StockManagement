using FastEndpoints;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using StockManagement.Auth.Core.Contracts;
using StockManagement.Kernel.Database.Interfaces;
using StockManagement.Kernel.Exceptions;
using StockManagement.Kernel.Model;

namespace StockManagement.Api.Features.Suppliers;


public sealed record UpdateSupplierRequest(string Id, string Name, string ContactName = "", string Country = "", string Currency = "", int LeadTimeDays = 0, string Miscellaneous = "");


public class UpdateSupplierValidator : Validator<UpdateSupplierRequest>
{
	public UpdateSupplierValidator()
	{
		this.RuleFor(request => request.Name).NotEmpty();
		this.RuleFor(request => request.LeadTimeDays).GreaterThanOrEqualTo(0);
	}
}


/// <remarks>Matches the stored supplier by <c>Id</c> (docs/decisions.md: update by Id, never by business key).</remarks>
public class UpdateSupplierEndpoint(ISupplierServiceProvider supplierServiceProvider) : Endpoint<UpdateSupplierRequest, Results<Ok<SupplierResponse>, NotFound, Conflict<DuplicateSupplierNameResponse>>>
{
	private readonly ISupplierServiceProvider _supplierServiceProvider = supplierServiceProvider;


	public override void Configure()
	{
		this.Put("/suppliers/{Id}");
		this.Permissions(Permission.SuppliersWrite);
	}

	public override async Task<Results<Ok<SupplierResponse>, NotFound, Conflict<DuplicateSupplierNameResponse>>> ExecuteAsync(UpdateSupplierRequest request, CancellationToken cancellationToken)
	{
		if (await _supplierServiceProvider.GetSupplierByIdAsync(request.Id) is not Supplier supplier) return TypedResults.NotFound();

		supplier.Name = request.Name;
		supplier.ContactName = request.ContactName;
		supplier.Country = request.Country;
		supplier.Currency = request.Currency;
		supplier.LeadTimeDays = request.LeadTimeDays;
		supplier.Miscellaneous = request.Miscellaneous;

		try
		{
			await _supplierServiceProvider.UpdateSupplierAsync(supplier);
		}
		catch (SupplierNameAlreadyExistsException)
		{
			return TypedResults.Conflict(new DuplicateSupplierNameResponse(request.Name));
		}

		return TypedResults.Ok(SupplierResponse.From(supplier));
	}
}
