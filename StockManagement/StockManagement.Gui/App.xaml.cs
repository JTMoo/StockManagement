using StockManagement.Gui.View;
using StockManagement.Gui.ViewModel;
using StockManagement.Kernel;
using System.Windows;

namespace StockManagement.Gui;


public partial class App
{
    private MainViewModel _mainViewModel;


	protected override void OnStartup(StartupEventArgs e)
    {
        MainManager.Instance.Init();

        _mainViewModel = new MainViewModel();
        GuiManager.Instance.Init(_mainViewModel);

        new MainWindow()
        {
	        DataContext = _mainViewModel
        }.Show();

		base.OnStartup(e);
    }

    protected override void OnExit(ExitEventArgs e)
    {
        MainManager.Instance.Dispose();
        base.OnExit(e);
    }
}