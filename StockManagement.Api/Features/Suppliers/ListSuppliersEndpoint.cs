using FastEndpoints;
using StockManagement.Auth.Core.Contracts;
using StockManagement.Kernel.Database.Interfaces;

namespace StockManagement.Api.Features.Suppliers;


public sealed record ListSuppliersRequest(string? Cursor, int? PageSize);


/// <param name="NextCursor">Opaque cursor for the next page; <see langword="null"/> on the last page</param>
public sealed record SupplierListResponse(IReadOnlyList<SupplierResponse> Items, string? NextCursor);


public class ListSuppliersEndpoint(ISupplierServiceProvider supplierServiceProvider) : Endpoint<ListSuppliersRequest, SupplierListResponse>
{
	private readonly ISupplierServiceProvider _supplierServiceProvider = supplierServiceProvider;


	public override void Configure()
	{
		this.Get("/suppliers");
		this.Permissions(Permission.SuppliersRead);
	}

	public override async Task<SupplierListResponse> ExecuteAsync(ListSuppliersRequest request, CancellationToken cancellationToken)
	{
		var pageSize = Math.Clamp(request.PageSize ?? 20, 1, 100);
		var result = await _supplierServiceProvider.GetSuppliersAsync(request.Cursor, pageSize, cancellationToken);
		return new(result.Items.Select(SupplierResponse.From).ToList(), result.NextCursor);
	}
}
