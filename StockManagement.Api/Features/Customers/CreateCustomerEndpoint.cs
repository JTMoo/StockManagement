using FastEndpoints;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using StockManagement.Customers.Core.Contracts;
using StockManagement.Kernel.Model;

namespace StockManagement.Api.Features.Customers;


public sealed record CreateCustomerRequest(string Name, string Lastname = "", string Address = "", string PhoneNumber = "", string IdentificationNumber = "", string PostboxNumber = "", string Email = "", string Miscellaneous = "");


public class CreateCustomerValidator : Validator<CreateCustomerRequest>
{
	public CreateCustomerValidator()
	{
		this.RuleFor(request => request.Name).NotEmpty();
	}
}


/// <remarks>The customer id is assigned by the server.</remarks>
public class CreateCustomerEndpoint(ICustomerService customerService) : Endpoint<CreateCustomerRequest, Created<CustomerResponse>>
{
	private readonly ICustomerService _customerService = customerService;


	public override void Configure()
	{
		this.Post("/customers");
	}

	public override async Task<Created<CustomerResponse>> ExecuteAsync(CreateCustomerRequest request, CancellationToken cancellationToken)
	{
		var customer = await _customerService.CreateCustomerAsync(new Customer()
		{
			Name = request.Name,
			Lastname = request.Lastname,
			Address = request.Address,
			PhoneNumber = request.PhoneNumber,
			IdentificationNumber = request.IdentificationNumber,
			PostboxNumber = request.PostboxNumber,
			Email = request.Email,
			Miscellaneous = request.Miscellaneous
		}, cancellationToken);

		return TypedResults.Created($"/api/customers/{customer.CustomerId}", CustomerResponse.From(customer));
	}
}
