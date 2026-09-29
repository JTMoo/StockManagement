using StockManagement.Kernel.Model;

namespace StockManagement.Api.Features.Suppliers;


/// <summary>
/// A vendor stock items can be reordered from
/// </summary>
public sealed record SupplierResponse(string Id, string Name, string ContactName, string Country, string Currency, int LeadTimeDays, string Miscellaneous)
{
	public static SupplierResponse From(Supplier supplier)
	{
		return new(supplier.Id, supplier.Name, supplier.ContactName, supplier.Country, supplier.Currency, supplier.LeadTimeDays, supplier.Miscellaneous);
	}
}
