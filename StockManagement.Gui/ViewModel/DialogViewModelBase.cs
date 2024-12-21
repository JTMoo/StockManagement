using StockManagement.Gui.Commands;

namespace StockManagement.Gui.ViewModel;


public delegate void DialogClosingEventHandler(bool success);

public class DialogViewModelBase : ViewModelBase
{
	public event DialogClosingEventHandler DialogClosing;

	public DialogViewModelBase()
	{
		ConfirmDialogCommand = new RelayCommand<string>(_ => this.Confirm());
		CancelDialogCommand = new RelayCommand<string>(_ => this.Cancel());
	}

	public RelayCommand<string> ConfirmDialogCommand { get; }
	public RelayCommand<string> CancelDialogCommand { get; }

	public virtual void Cancel()
	{
		this.Close(false);
	}

	public void Close(bool success)
	{
		DialogClosing?.Invoke(success);
	}

	public virtual void Confirm()
	{
		this.Close(true);
	}
}