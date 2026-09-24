using System;
using System.Windows;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StockManagement.Customers.Core;
using StockManagement.Gui.View;
using StockManagement.Gui.ViewModel;
using StockManagement.Import.Core;
using StockManagement.Kernel;
using StockManagement.Kernel.Database;
using StockManagement.Sales.Core;

namespace StockManagement.Gui;


public partial class App
{
	private ServiceProvider _services;


	protected override async void OnStartup(StartupEventArgs e)
	{
		var configuration = new ConfigurationBuilder()
			.SetBasePath(AppContext.BaseDirectory)
			.AddJsonFile("appsettings.json")
			.AddJsonFile("appsettings.local.json", optional: true)
			.Build();
		var databaseAccess = await MainManager.Initialize(configuration.GetMongoDatabase());

		_services = new ServiceCollection()
			.AddKernel(databaseAccess)
			.AddSalesCore()
			.AddCustomersCore()
			.AddImportCore()
			.AddGui()
			.BuildServiceProvider(new ServiceProviderOptions() { ValidateOnBuild = true, ValidateScopes = true });

		var mainViewModel = await _services.GetRequiredService<MainViewModel>().InitializeAsync();
		GuiManager.Instance.Init(mainViewModel);

		new MainWindow()
		{
			DataContext = mainViewModel
		}.Show();

		base.OnStartup(e);
	}

	protected override void OnExit(ExitEventArgs e)
	{
		_services?.Dispose();
		MainManager.Dispose(true);
		base.OnExit(e);
	}
}
