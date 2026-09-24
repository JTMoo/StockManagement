using FastEndpoints;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using StockManagement.Kernel.Database.Interfaces;
using StockManagement.Kernel.Model;

namespace StockManagement.Api.Features.Customers;


public sealed record UpdateCustomerRequest(int CustomerId, string Name, string Lastname = "", string Address = "", string PhoneNumber = "", string IdentificationNumber = "", string PostboxNumber = "", string Email = "", string Miscellaneous = "");


public class UpdateCustomerValidator : Validator<UpdateCustomerRequest>
{
	public UpdateCustomerValidator()
	{
		this.RuleFor(request => request.Name).NotEmpty();
	}
}


public class UpdateCustomerEndpoint(ICustomerServiceProvider customerServiceProvider) : Endpoint<UpdateCustomerRequest, Results<Ok<CustomerResponse>, NotFound>>
{
	private readonly ICustomerServiceProvider _customerServiceProvider = customerServiceProvider;


	public override void Configure()
	{
		this.Put("/customers/{CustomerId}");
		this.AllowAnonymous();
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
