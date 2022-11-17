using StockManagement.Gui.Commands;
using System.Windows;


namespace StockManagement.Gui.ViewModel
{
    internal class MainViewModel
    {
        public MainViewModel()
        {
            this.QuitCommand = new RelayCommand<string>(_ => Application.Current.Shutdown());
        }

        public RelayCommand<string> QuitCommand { get; }
    }
}
