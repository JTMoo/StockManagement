using StockManagement.Gui.View;
using StockManagement.Gui.ViewModel;
using StockManagement.Kernel;
using System.Windows;

namespace StockManagement.Gui;


public partial class App : Application
{
    private MainViewModel _mainViewModel;


    protected override void OnStartup(StartupEventArgs e)
    {
        MainManager.Instance.Init();

        _mainViewModel = new MainViewModel();
        GuiManager.Instance.Init(_mainViewModel);

        Window main = new MainWindow();
        main.DataContext = _mainViewModel;
        main.Show();

        base.OnStartup(e);
    }

    protected override void OnExit(ExitEventArgs e)
    {
        MainManager.Instance.Dispose();
        base.OnExit(e);
    }
}
