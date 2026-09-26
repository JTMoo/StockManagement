using FastEndpoints;
using StockManagement.Auth.Core.Contracts;
using StockManagement.Kernel.Database.Interfaces;

namespace StockManagement.Api.Features.Customers;


public class ListCustomersEndpoint(ICustomerServiceProvider customerServiceProvider) : EndpointWithoutRequest<IReadOnlyList<CustomerResponse>>
{
	private readonly ICustomerServiceProvider _customerServiceProvider = customerServiceProvider;


	public override void Configure()
	{
		this.Get("/customers");
		this.Permissions(Permission.CustomersRead);
	}

	public override async Task<IReadOnlyList<CustomerResponse>> ExecuteAsync(CancellationToken cancellationToken)
	{
		var customers = await _customerServiceProvider.GetCustomersAsync() ?? [];
		return customers.Select(CustomerResponse.From).ToList();
	}
}
