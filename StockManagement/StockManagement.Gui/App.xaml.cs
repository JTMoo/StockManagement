using StockManagement.Kernel;
using System.Windows;


namespace StockManagement.Gui
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            MainManager.Instance.Init();
            base.OnStartup(e);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            MainManager.Instance.Dispose();
            base.OnExit(e);
        }
    }
}
