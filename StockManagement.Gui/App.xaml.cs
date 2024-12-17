using StockManagement.Gui.View;
using StockManagement.Gui.ViewModel;
using StockManagement.Kernel;
using System.Windows;

namespace StockManagement.Gui;


public partial class App
{
    private MainViewModel _mainViewModel;


	protected override async void OnStartup(StartupEventArgs e)
    {
        var databaseAccess = await MainManager.Initialize();

        _mainViewModel = await MainViewModel.CreateAsync(databaseAccess);
        GuiManager.Instance.Init(_mainViewModel);

        new MainWindow()
        {
	        DataContext = _mainViewModel
        }.Show();

		base.OnStartup(e);
    }

    protected override void OnExit(ExitEventArgs e)
    {
        MainManager.Dispose(true);
        base.OnExit(e);
    }
}