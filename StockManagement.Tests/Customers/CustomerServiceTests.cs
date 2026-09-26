using Moq;
using StockManagement.Customers.Core;
using StockManagement.Kernel.Database.Interfaces;
using StockManagement.Kernel.Model;
using StockManagement.Settings.Core.Contracts;

namespace StockManagement.Tests.Customers;


[TestClass]
public sealed class CustomerServiceTests
{
	private readonly Mock<ICustomerServiceProvider> _customers = new();
	private readonly Mock<ISettingsService> _settings = new();


	[TestInitialize]
	public void Initialize()
	{
		_settings.Setup(service => service.GetCompanySettingsAsync(It.IsAny<CancellationToken>()))
			.ReturnsAsync(new CompanySettings("", "", "", 10m, 30, 1, 1001, 0));
	}

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

	[TestMethod]
	public async Task GetNextCustomerIdAsync_NoCustomers_UsesConfiguredFirstCustomerId()
	{
		// Arrange
		_customers.Setup(provider => provider.GetCustomersAsync()).ReturnsAsync([]);
		_settings.Setup(service => service.GetCompanySettingsAsync(It.IsAny<CancellationToken>()))
			.ReturnsAsync(new CompanySettings("", "", "", 10m, 30, 1, 2000, 0));

		// Act
		var result = await this.CreateService().GetNextCustomerIdAsync();

		// Assert
		Assert.AreEqual(2000, result);
	}


	[TestMethod]
	public async Task CreateCustomerAsync_ExistingCustomers_StoresWithNextId()
	{
		// Arrange
		_customers.Setup(provider => provider.GetCustomersAsync()).ReturnsAsync([new Customer() { CustomerId = 1001 }]);
		var customer = new Customer() { Name = "Ana", CustomerId = 5 };

		// Act
		var result = await this.CreateService().CreateCustomerAsync(customer);

		// Assert
		Assert.AreSame(customer, result);
		Assert.AreEqual(1002, result.CustomerId);
		_customers.Verify(provider => provider.AddCustomerAsync(customer), Times.Once);
	}

	private CustomerService CreateService()
	{
		return new CustomerService(_customers.Object, _settings.Object);
	}
}
