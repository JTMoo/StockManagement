using FastEndpoints;
using Microsoft.AspNetCore.Http.HttpResults;
using StockManagement.Kernel.Database.Interfaces;
using StockManagement.Kernel.Model;

namespace StockManagement.Api.Features.Customers;


public sealed record GetCustomerRequest(int CustomerId);


public class GetCustomerEndpoint(ICustomerServiceProvider customerServiceProvider) : Endpoint<GetCustomerRequest, Results<Ok<CustomerResponse>, NotFound>>
{
	private readonly ICustomerServiceProvider _customerServiceProvider = customerServiceProvider;


	public override void Configure()
	{
		this.Get("/customers/{CustomerId}");
		this.AllowAnonymous();
	}

	public override async Task<Results<Ok<CustomerResponse>, NotFound>> ExecuteAsync(GetCustomerRequest request, CancellationToken cancellationToken)
	{
		if (await _customerServiceProvider.GetCustomerAsync(request.CustomerId) is not Customer customer) return TypedResults.NotFound();

		return TypedResults.Ok(CustomerResponse.From(customer));
	}
}
