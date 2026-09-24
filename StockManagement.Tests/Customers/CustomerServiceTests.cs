using Moq;
using StockManagement.Customers.Core;
using StockManagement.Kernel.Database.Interfaces;
using StockManagement.Kernel.Model;

namespace StockManagement.Tests.Customers;


[TestClass]
public sealed class CustomerServiceTests
{
	private readonly Mock<ICustomerServiceProvider> _customers = new();


	[TestMethod]
	public async Task GetNextCustomerIdAsync_NoCustomers_Returns1001()
	{
		// Arrange
		_customers.Setup(provider => provider.GetCustomersAsync()).ReturnsAsync([]);

		// Act
		var result = await this.CreateService().GetNextCustomerIdAsync();

		// Assert
		Assert.AreEqual(1001, result);
	}

	[TestMethod]
	public async Task GetNextCustomerIdAsync_ProviderReturnsNull_Returns1001()
	{
		// Arrange
		_customers.Setup(provider => provider.GetCustomersAsync()).ReturnsAsync((IEnumerable<Customer>)null);

		// Act
		var result = await this.CreateService().GetNextCustomerIdAsync();

		// Assert
		Assert.AreEqual(1001, result);
	}

	[TestMethod]
	public async Task GetNextCustomerIdAsync_ExistingCustomers_ReturnsHighestPlusOne()
	{
		// Arrange
		_customers.Setup(provider => provider.GetCustomersAsync()).ReturnsAsync([new Customer() { CustomerId = 1001 }, new Customer() { CustomerId = 1017 }]);

		// Act
		var result = await this.CreateService().GetNextCustomerIdAsync();

		// Assert
		Assert.AreEqual(1018, result);
	}


	private CustomerService CreateService()
	{
		return new CustomerService(_customers.Object);
	}
}
