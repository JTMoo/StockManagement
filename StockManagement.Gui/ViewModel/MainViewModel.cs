using StockManagement.Gui.Commands;
using StockManagement.Gui.ViewModel.Dialogs;
using StockManagement.Gui.ViewModel.Primary;
using StockManagement.Kernel;
using StockManagement.Kernel.Database;
using StockManagement.Kernel.Database.Interfaces;
using StockManagement.Kernel.Model;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;

namespace StockManagement.Gui.ViewModel;


internal class MainViewModel : NotificationBase
{
	private DialogViewModelBase? _dialog;
	private ViewModelBase _currentView;
	private bool isWaitDialogVisible = false;
	private int responsiveDialogBorderThickness;
	private IStockItemServiceProvider _stockItemServiceProvider;
	private ICustomerServiceProvider _customerServiceProvider;
	private IInvoiceServiceProvider _invoiceServiceProvider;


	private MainViewModel(IDatabase database)
	{
		_stockItemServiceProvider = new StockItemServiceProvider(database);
		_customerServiceProvider = new CustomerServiceProvider(database);
		_invoiceServiceProvider = new InvoiceServiceProvider(database);

		QuitCommand = new RelayCommand<string>(_ => Application.Current.Shutdown());
		OpenSettingsCommand = new RelayCommand<string>(_ => this.Dialog = new SettingsDialogViewModel());

		OpenStockItemsViewCommand = new RelayCommand<string>(async _ => this.CurrentView = await StockItemsViewModel.CreateAsync(_stockItemServiceProvider, _customerServiceProvider, _invoiceServiceProvider));
		OpenCustomerViewCommand = new RelayCommand<string>(async _ => this.CurrentView = await CustomerViewModel.CreateAsync(_customerServiceProvider));
		OpenInvoiceViewCommand = new RelayCommand<string>(async _ => this.CurrentView = await InvoiceViewModel.CreateAsync(_invoiceServiceProvider));

		this.ResponsiveDialogBorderThickness = MainManagerFacade.Settings.DialogBorderThickness;
		MainManagerFacade.Settings.PropertyChanged += this.OnSettingsChanged;
	}

	#region Properties
	public RelayCommand<string> QuitCommand { get; }
	public RelayCommand<string> OpenSettingsCommand { get; }
	public RelayCommand<string> OpenStockItemsViewCommand { get; }
	public RelayCommand<string> OpenInvoiceViewCommand { get; }
	public RelayCommand<string> OpenCustomerViewCommand { get; }
	public DialogViewModelBase? Dialog
	{
		get { return _dialog; }
		internal set
		{
			this.SetField(ref _dialog, value);

			if (_dialog != null)
			{
				_dialog.DialogClosing += this.OnDialogClosing;
			}
		}
	}
	public ViewModelBase CurrentView
	{
		get { return _currentView; }
		internal set { this.SetField(ref _currentView, value); }
	}

	public int ResponsiveDialogBorderThickness
	{
		get => this.responsiveDialogBorderThickness;
		set => this.SetField(ref this.responsiveDialogBorderThickness, value);
	}

	public bool IsWaitDialogVisible
	{
		get => this.isWaitDialogVisible;
		set => this.SetField(ref this.isWaitDialogVisible, value);
	}
	#endregion Properties

	public static Task<MainViewModel> CreateAsync(IDatabase database)
	{
		var ret = new MainViewModel(database);
		return ret.InitializeAsync(database);
	}

	private async Task<MainViewModel> InitializeAsync(IDatabase database)
	{
		this.CurrentView = await StockItemsViewModel.CreateAsync(_stockItemServiceProvider, _customerServiceProvider, _invoiceServiceProvider);
		return this;
	}

	private void OnDialogClosing(bool success)
	{
		if (this.Dialog == null) return;

		this.Dialog.DialogClosing -= this.OnDialogClosing;
		this.Dialog = null;
	}

	private void OnSettingsChanged(object? sender, PropertyChangedEventArgs e)
	{
		switch (e.PropertyName)
		{
			case nameof(Settings.DialogBorderThickness):
				Application.Current.Dispatcher.Invoke(() =>
				{
					this.ResponsiveDialogBorderThickness = MainManagerFacade.Settings.DialogBorderThickness;
				});
				break;
		}
	}
}