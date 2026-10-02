using FastEndpoints;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using StockManagement.Auth.Core.Contracts;
using StockManagement.Kernel.Database.Interfaces;
using StockManagement.Kernel.Exceptions;
using StockManagement.Kernel.Model;

namespace StockManagement.Api.Features.Suppliers;


public sealed record CreateSupplierRequest(string Name, string ContactName = "", string Country = "", string Currency = "", int LeadTimeDays = 0, string Miscellaneous = "");


public class CreateSupplierValidator : Validator<CreateSupplierRequest>
{
	public CreateSupplierValidator()
	{
		this.RuleFor(request => request.Name).NotEmpty();
		this.RuleFor(request => request.LeadTimeDays).GreaterThanOrEqualTo(0);
	}
}


public sealed record DuplicateSupplierNameResponse(string Name);


public class CreateSupplierEndpoint(ISupplierServiceProvider supplierServiceProvider) : Endpoint<CreateSupplierRequest, Results<Created<SupplierResponse>, Conflict<DuplicateSupplierNameResponse>>>
{
	private readonly ISupplierServiceProvider _supplierServiceProvider = supplierServiceProvider;


	public override void Configure()
	{
		this.Post("/suppliers");
		this.Permissions(Permission.SuppliersWrite);
	}

	public override async Task<Results<Created<SupplierResponse>, Conflict<DuplicateSupplierNameResponse>>> ExecuteAsync(CreateSupplierRequest request, CancellationToken cancellationToken)
	{
		var supplier = new Supplier(request.Name, request.ContactName, request.Country, request.Currency, request.LeadTimeDays, request.Miscellaneous);

		try
		{
			await _supplierServiceProvider.AddSupplierAsync(supplier, cancellationToken);
		}
		catch (SupplierNameAlreadyExistsException)
		{
			return TypedResults.Conflict(new DuplicateSupplierNameResponse(request.Name));
		}

		return TypedResults.Created($"/api/suppliers/{supplier.Id}", SupplierResponse.From(supplier));
	}
}
