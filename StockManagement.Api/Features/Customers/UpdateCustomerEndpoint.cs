using FastEndpoints;
using Microsoft.AspNetCore.Http.HttpResults;
using StockManagement.Auth.Core.Contracts;
using StockManagement.Kernel.Database.Interfaces;
using StockManagement.Kernel.Exceptions;
using StockManagement.Kernel.Model;
using StockManagement.Kernel.Util;

namespace StockManagement.Api.Features.Customers;


public class UpdateCustomerEndpoint(ICustomerServiceProvider customerServiceProvider) : Endpoint<UpdateCustomerRequest, Results<Ok<CustomerResponse>, NotFound, Conflict<DuplicateCustomerIdentificationNumberResponse>>>
{
	private readonly ICustomerServiceProvider _customerServiceProvider = customerServiceProvider;


	public override void Configure()
	{
		this.Put("/customers/{CustomerId}");
		this.Permissions(Permission.CustomersWrite);
	}

	public override async Task<Results<Ok<CustomerResponse>, NotFound, Conflict<DuplicateCustomerIdentificationNumberResponse>>> ExecuteAsync(UpdateCustomerRequest request, CancellationToken cancellationToken)
	{
		if (await _customerServiceProvider.GetCustomerAsync(request.CustomerId) is not Customer customer) return TypedResults.NotFound();

		var identificationNumber = RucValidator.TryNormalize(request.IdentificationNumber, out var normalized) ? normalized : request.IdentificationNumber;

		customer.Name = request.Name;
		customer.Lastname = request.Lastname;
		customer.Address = request.Address;
		customer.PhoneNumber = request.PhoneNumber;
		customer.IdentificationNumber = identificationNumber;
		customer.PostboxNumber = request.PostboxNumber;
		customer.Email = request.Email;
		customer.Miscellaneous = request.Miscellaneous;

		try
		{
			await _customerServiceProvider.UpdateCustomerAsync(customer);
		}
		catch (CustomerIdentificationNumberAlreadyExistsException)
		{
			return TypedResults.Conflict(new DuplicateCustomerIdentificationNumberResponse(identificationNumber));
		}

		return TypedResults.Ok(CustomerResponse.From(customer));
	}
}
