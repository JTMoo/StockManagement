using FastEndpoints;
using Microsoft.AspNetCore.Http.HttpResults;
using StockManagement.Auth.Core.Contracts;
using StockManagement.Kernel.Database.Interfaces;
using StockManagement.Kernel.Model;

namespace StockManagement.Api.Features.Customers;


public class UpdateCustomerEndpoint(ICustomerServiceProvider customerServiceProvider) : Endpoint<UpdateCustomerRequest, Results<Ok<CustomerResponse>, NotFound>>
{
	private readonly ICustomerServiceProvider _customerServiceProvider = customerServiceProvider;


	public override void Configure()
	{
		this.Put("/customers/{CustomerId}");
		this.Permissions(Permission.CustomersWrite);
	}

	public override async Task<Results<Ok<CustomerResponse>, NotFound>> ExecuteAsync(UpdateCustomerRequest request, CancellationToken cancellationToken)
	{
		if (await _customerServiceProvider.GetCustomerAsync(request.CustomerId) is not Customer customer) return TypedResults.NotFound();

		customer.Name = request.Name;
		customer.Lastname = request.Lastname;
		customer.Address = request.Address;
		customer.PhoneNumber = request.PhoneNumber;
		customer.IdentificationNumber = request.IdentificationNumber;
		customer.PostboxNumber = request.PostboxNumber;
		customer.Email = request.Email;
		customer.Miscellaneous = request.Miscellaneous;

		await _customerServiceProvider.UpdateCustomerAsync(customer);
		return TypedResults.Ok(CustomerResponse.From(customer));
	}
}
