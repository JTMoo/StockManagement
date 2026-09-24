namespace StockManagement.Api.Features.Customers;


public sealed record UpdateCustomerRequest(int CustomerId, string Name, string Lastname = "", string Address = "", string PhoneNumber = "", string IdentificationNumber = "", string PostboxNumber = "", string Email = "", string Miscellaneous = "");
