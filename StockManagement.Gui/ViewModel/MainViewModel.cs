using StockManagement.Gui.Commands;
using StockManagement.Gui.ViewModel.Dialogs;
using StockManagement.Gui.ViewModel.Primary;
using StockManagement.Kernel;
using StockManagement.Kernel.Model;
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;

namespace StockManagement.Gui.ViewModel;


internal class MainViewModel : NotificationBase
{
	private DialogViewModelBase? _dialog;
	private ViewModelBase _currentView;
	private bool isWaitDialogVisible = false;
	private bool _isMenuExtended = false;
	private int responsiveDialogBorderThickness;
	private readonly Func<StockItemsViewModel> _createStockItemsViewModel;


	public MainViewModel(Func<StockItemsViewModel> createStockItemsViewModel, Func<CustomerViewModel> createCustomerViewModel, Func<InvoiceViewModel> createInvoiceViewModel, Func<LoginViewModel> createLoginViewModel)
	{
		_createStockItemsViewModel = createStockItemsViewModel;

		ToggleMenuCommand = new RelayCommand<string>(_ => this.IsMenuExtended = !this.IsMenuExtended);
		QuitCommand = new RelayCommand<string>(_ => Application.Current.Shutdown());
		OpenSettingsCommand = new RelayCommand<string>(_ => this.Dialog = new SettingsDialogViewModel());

		OpenStockItemsViewCommand = new RelayCommand<string>(async _ => this.CurrentView = await createStockItemsViewModel().InitializeAsync());
		OpenCustomerViewCommand = new RelayCommand<string>(async _ => this.CurrentView = await createCustomerViewModel().InitializeAsync());
		OpenInvoiceViewCommand = new RelayCommand<string>(async _ => this.CurrentView = await createInvoiceViewModel().InitializeAsync());
		OpenLoginCommand = new RelayCommand<string>(async _ => this.CurrentView = await createLoginViewModel().InitializeAsync());

		this.ResponsiveDialogBorderThickness = MainManagerFacade.Settings.DialogBorderThickness;
		MainManagerFacade.Settings.PropertyChanged += this.OnSettingsChanged;
	}

	#region Properties
	public RelayCommand<string> QuitCommand { get; }
	public RelayCommand<string> ToggleMenuCommand { get; }
	public RelayCommand<string> OpenSettingsCommand { get; }
	public RelayCommand<string> OpenStockItemsViewCommand { get; }
	public RelayCommand<string> OpenInvoiceViewCommand { get; }
	public RelayCommand<string> OpenLoginCommand { get; }
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

	public bool IsMenuExtended
	{
		get => _isMenuExtended;
		set => this.SetField(ref _isMenuExtended, value);
	}
	#endregion Properties

	/// <summary>
	/// Opens the stock view. Call once after the container created the view model.
	/// </summary>
	public async Task<MainViewModel> InitializeAsync()
	{
		this.CurrentView = await _createStockItemsViewModel().InitializeAsync();
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