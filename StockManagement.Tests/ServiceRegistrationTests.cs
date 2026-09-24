using Microsoft.Extensions.DependencyInjection;
using Moq;
using StockManagement.Customers.Core;
using StockManagement.Customers.Core.Contracts;
using StockManagement.Import.Core;
using StockManagement.Import.Core.Contracts;
using StockManagement.Kernel;
using StockManagement.Kernel.Database.Interfaces;
using StockManagement.Sales.Core;
using StockManagement.Sales.Core.Contracts;

namespace StockManagement.Tests;


[TestClass]
public sealed class ServiceRegistrationTests
{
	[TestMethod]
	public void AddKernelAndCores_BuildWithValidation_ResolvesEveryContract()
	{
		// Arrange
		var services = new ServiceCollection()
			.AddKernel(new Mock<IDatabase>().Object)
			.AddSalesCore()
			.AddCustomersCore()
			.AddImportCore();

		// Act
		using var provider = services.BuildServiceProvider(new ServiceProviderOptions() { ValidateOnBuild = true, ValidateScopes = true });

		// Assert
		Assert.IsNotNull(provider.GetRequiredService<IStockItemServiceProvider>());
		Assert.IsNotNull(provider.GetRequiredService<ICustomerServiceProvider>());
		Assert.IsNotNull(provider.GetRequiredService<IInvoiceServiceProvider>());
		Assert.IsNotNull(provider.GetRequiredService<IUserServiceProvider>());
		Assert.IsNotNull(provider.GetRequiredService<ISaleService>());
		Assert.IsNotNull(provider.GetRequiredService<ICustomerService>());
		Assert.IsNotNull(provider.GetRequiredService<IStockItemImportService>());
	}

	[TestMethod]
	public void AddKernel_NullDatabase_Throws()
	{
		// Act + Assert
		Assert.ThrowsException<ArgumentNullException>(() => new ServiceCollection().AddKernel(null));
	}
}
