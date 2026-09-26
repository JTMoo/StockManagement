using Microsoft.Extensions.DependencyInjection;
using Moq;
using StockManagement.Auth.Core;
using StockManagement.Auth.Core.Contracts;
using StockManagement.Customers.Core;
using StockManagement.Customers.Core.Contracts;
using StockManagement.Import.Core;
using StockManagement.Import.Core.Contracts;
using StockManagement.Kernel.Database.Interfaces;
using StockManagement.Sales.Core;
using StockManagement.Sales.Core.Contracts;
using StockManagement.Settings.Core;
using StockManagement.Settings.Core.Contracts;

namespace StockManagement.Tests;


[TestClass]
public sealed class ServiceRegistrationTests
{
	[TestMethod]
	public void AddCores_ResolveAgainstProviderInterfaces_ResolvesEveryContract()
	{
		// Arrange
		var services = new ServiceCollection()
			.AddSingleton(new Mock<IStockItemServiceProvider>().Object)
			.AddSingleton(new Mock<ICustomerServiceProvider>().Object)
			.AddSingleton(new Mock<IInvoiceServiceProvider>().Object)
			.AddSingleton(new Mock<IUserServiceProvider>().Object)
			.AddSingleton(new Mock<ISettingsServiceProvider>().Object)
			.AddSingleton(new Mock<IImportBatchServiceProvider>().Object)
			.AddSalesCore()
			.AddCustomersCore()
			.AddImportCore()
			.AddAuthCore()
			.AddSettingsCore();

		// Act
		using var provider = services.BuildServiceProvider(new ServiceProviderOptions() { ValidateOnBuild = true, ValidateScopes = true });

		// Assert
		Assert.IsNotNull(provider.GetRequiredService<ISaleService>());
		Assert.IsNotNull(provider.GetRequiredService<ICustomerService>());
		Assert.IsNotNull(provider.GetRequiredService<IStockItemImportService>());
		Assert.IsNotNull(provider.GetRequiredService<IImportBatchService>());
		Assert.IsNotNull(provider.GetRequiredService<IAuthService>());
		Assert.IsNotNull(provider.GetRequiredService<ISettingsService>());
	}
}
