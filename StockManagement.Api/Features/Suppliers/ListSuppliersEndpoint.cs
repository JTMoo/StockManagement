using FastEndpoints;
using StockManagement.Auth.Core.Contracts;
using StockManagement.Kernel.Database.Interfaces;

namespace StockManagement.Api.Features.Suppliers;


public class ListSuppliersEndpoint(ISupplierServiceProvider supplierServiceProvider) : EndpointWithoutRequest<IReadOnlyList<SupplierResponse>>
{
	private readonly ISupplierServiceProvider _supplierServiceProvider = supplierServiceProvider;


	public override void Configure()
	{
		this.Get("/suppliers");
		this.Permissions(Permission.SuppliersRead);
	}

	public override async Task<IReadOnlyList<SupplierResponse>> ExecuteAsync(CancellationToken cancellationToken)
	{
		var suppliers = await _supplierServiceProvider.GetAllSuppliersAsync(cancellationToken) ?? [];
		return suppliers.Select(SupplierResponse.From).ToList();
	}
}
