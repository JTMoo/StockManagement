using StockManagement.Gui.Commands;
using StockManagement.Kernel;

namespace StockManagement.Gui.ViewModel;


public delegate void DialogClosingEventHandler(bool success);

public class DialogViewModelBase : NotificationBase
{
	public event DialogClosingEventHandler DialogClosing;

	private bool _visible;

	public DialogViewModelBase()
	{
		ConfirmDialogCommand = new RelayCommand<string>(this.Confirm);
		CancelDialogCommand = new RelayCommand<string>(this.Cancel);
	}

	public bool Visible
	{
		get { return _visible; }
		private set { this.SetField(ref _visible, value); }
	}

	public RelayCommand<string> ConfirmDialogCommand { get; }
	public RelayCommand<string> CancelDialogCommand { get; }

	public virtual void Cancel(string param)
	{
		this.Close(false);
	}

	public void Close(bool success)
	{
		if (DialogClosing != null)
			DialogClosing(success);
		
		this.Visible = false;
	}

	public virtual void Confirm(string param)
	{
		this.Close(true);
	}

	public void Open()
	{
		this.Visible = true;
	}
}
