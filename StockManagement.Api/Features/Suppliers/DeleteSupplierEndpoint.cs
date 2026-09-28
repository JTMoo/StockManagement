using FastEndpoints;
using Microsoft.AspNetCore.Http.HttpResults;
using StockManagement.Auth.Core.Contracts;
using StockManagement.Kernel.Database.Interfaces;
using StockManagement.Kernel.Exceptions;
using StockManagement.Kernel.Model;

namespace StockManagement.Api.Features.Suppliers;


public sealed record DeleteSupplierRequest(string Id);

public sealed record SupplierInUseResponse(string Reason);


/// <remarks>Matches the stored supplier by <c>Id</c> (docs/decisions.md: update by Id, never by business key).</remarks>
public class DeleteSupplierEndpoint(ISupplierServiceProvider supplierServiceProvider) : Endpoint<DeleteSupplierRequest, Results<NoContent, NotFound, Conflict<SupplierInUseResponse>>>
{
	private readonly ISupplierServiceProvider _supplierServiceProvider = supplierServiceProvider;


	public override void Configure()
	{
		this.Delete("/suppliers/{Id}");
		this.Permissions(Permission.SuppliersWrite);
	}

	public override async Task<Results<NoContent, NotFound, Conflict<SupplierInUseResponse>>> ExecuteAsync(DeleteSupplierRequest request, CancellationToken cancellationToken)
	{
		if (await _supplierServiceProvider.GetSupplierByIdAsync(request.Id) is not Supplier supplier) return TypedResults.NotFound();

		try
		{
			await _supplierServiceProvider.DeleteSupplierAsync(supplier);
		}
		catch (SupplierInUseException)
		{
			return TypedResults.Conflict(new SupplierInUseResponse(global::StockManagement.Language.Suppliers.supplierInUse));
		}

		return TypedResults.NoContent();
	}
}
