using FastEndpoints;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using StockManagement.Auth.Core.Contracts;
using StockManagement.Customers.Core.Contracts;
using StockManagement.Kernel.Exceptions;
using StockManagement.Kernel.Model;
using StockManagement.Kernel.Util;

namespace StockManagement.Api.Features.Customers;


public sealed record CreateCustomerRequest(string Name, string Lastname = "", string Address = "", string PhoneNumber = "", string IdentificationNumber = "", string PostboxNumber = "", string Email = "", string Miscellaneous = "");


public class CreateCustomerValidator : Validator<CreateCustomerRequest>
{
	public CreateCustomerValidator()
	{
		this.RuleFor(request => request.Name).NotEmpty().WithMessage("nameRequired");
	}
}


public sealed record DuplicateCustomerIdentificationNumberResponse(string Code);


/// <remarks>The customer id is assigned by the server.</remarks>
public class CreateCustomerEndpoint(ICustomerService customerService) : Endpoint<CreateCustomerRequest, Results<Created<CustomerResponse>, Conflict<DuplicateCustomerIdentificationNumberResponse>>>
{
	private readonly ICustomerService _customerService = customerService;


	public override void Configure()
	{
		this.Post("/customers");
		this.Permissions(Permission.CustomersWrite);
	}

	public override async Task<Results<Created<CustomerResponse>, Conflict<DuplicateCustomerIdentificationNumberResponse>>> ExecuteAsync(CreateCustomerRequest request, CancellationToken cancellationToken)
	{
		var identificationNumber = RucValidator.TryNormalize(request.IdentificationNumber, out var normalized) ? normalized : request.IdentificationNumber;

		Customer customer;
		try
		{
			customer = await _customerService.CreateCustomerAsync(new Customer()
			{
				Name = request.Name,
				Lastname = request.Lastname,
				Address = request.Address,
				PhoneNumber = request.PhoneNumber,
				IdentificationNumber = identificationNumber,
				PostboxNumber = request.PostboxNumber,
				Email = request.Email,
				Miscellaneous = request.Miscellaneous
			}, cancellationToken);
		}
		catch (CustomerIdentificationNumberAlreadyExistsException)
		{
			return TypedResults.Conflict(new DuplicateCustomerIdentificationNumberResponse(identificationNumber));
		}

		return TypedResults.Created($"/api/customers/{customer.CustomerId}", CustomerResponse.From(customer));
	}
}
