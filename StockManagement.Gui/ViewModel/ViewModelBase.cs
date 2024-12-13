using StockManagement.Gui.Commands;
using StockManagement.Kernel;

namespace StockManagement.Gui.ViewModel;


public abstract class ViewModelBase : NotificationBase
{
	private bool _isSearchBarVisible;


	public ViewModelBase()
	{
		this.ToggleSearchBarCommand = new RelayCommand<string>(_ => this.IsSearchBarVisible = !this.IsSearchBarVisible);
	}


	public RelayCommand<string> ToggleSearchBarCommand { get; }

	public bool IsSearchBarVisible
	{
		get { return _isSearchBarVisible; }
		set { this.SetField(ref _isSearchBarVisible, value); }
	}
}
