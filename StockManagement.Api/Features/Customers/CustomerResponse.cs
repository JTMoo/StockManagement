using StockManagement.Kernel.Model;

namespace StockManagement.Api.Features.Customers;


/// <summary>
/// Customer with its business id
/// </summary>
public sealed record CustomerResponse(int CustomerId, string Name, string Lastname, string Address, string PhoneNumber, string IdentificationNumber, string PostboxNumber, string Email, string Miscellaneous)
{
	public static CustomerResponse From(Customer customer)
	{
		return new(customer.CustomerId, customer.Name, customer.Lastname, customer.Address, customer.PhoneNumber, customer.IdentificationNumber, customer.PostboxNumber, customer.Email, customer.Miscellaneous);
	}
}
