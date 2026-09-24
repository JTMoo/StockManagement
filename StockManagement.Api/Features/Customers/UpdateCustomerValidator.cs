using FastEndpoints;
using FluentValidation;

namespace StockManagement.Api.Features.Customers;


public class UpdateCustomerValidator : Validator<UpdateCustomerRequest>
{
	public UpdateCustomerValidator()
	{
		this.RuleFor(request => request.Name).NotEmpty();
	}
}
