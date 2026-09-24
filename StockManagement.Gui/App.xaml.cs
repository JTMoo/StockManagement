using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using StockManagement.Gui.View;
using StockManagement.Gui.ViewModel;
using StockManagement.Kernel;

namespace StockManagement.Gui;


public partial class App
{
	private ServiceProvider _services;


	protected override async void OnStartup(StartupEventArgs e)
	{
		var databaseAccess = await MainManager.Initialize();

		_services = new ServiceCollection()
			.AddKernel(databaseAccess)
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
