namespace StockManagement.Customers.Core.Contracts;


public interface ICustomerService
{
	/// <summary>
	/// Id for the next new customer
	/// </summary>
	public Task<int> GetNextCustomerIdAsync(CancellationToken cancellationToken = default);
}
