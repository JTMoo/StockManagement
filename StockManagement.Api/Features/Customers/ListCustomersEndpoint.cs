using FastEndpoints;
using StockManagement.Auth.Core.Contracts;
using StockManagement.Kernel.Database.Interfaces;

namespace StockManagement.Api.Features.Customers;


public sealed record ListCustomersRequest(string? Cursor, int? PageSize);


/// <param name="NextCursor">Opaque cursor for the next page; <see langword="null"/> on the last page</param>
public sealed record CustomerListResponse(IReadOnlyList<CustomerResponse> Items, string? NextCursor);


public class ListCustomersEndpoint(ICustomerServiceProvider customerServiceProvider) : Endpoint<ListCustomersRequest, CustomerListResponse>
{
	private readonly ICustomerServiceProvider _customerServiceProvider = customerServiceProvider;


	public override void Configure()
	{
		this.Get("/customers");
		this.Permissions(Permission.CustomersRead);
	}

	public override async Task<CustomerListResponse> ExecuteAsync(ListCustomersRequest request, CancellationToken cancellationToken)
	{
		var pageSize = Math.Clamp(request.PageSize ?? 20, 1, 100);
		var result = await _customerServiceProvider.GetCustomersAsync(request.Cursor, pageSize, cancellationToken);
		return new(result.Items.Select(CustomerResponse.From).ToList(), result.NextCursor);
	}
}
