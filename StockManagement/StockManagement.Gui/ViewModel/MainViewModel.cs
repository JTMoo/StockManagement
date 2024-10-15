using Microsoft.Win32;
using StockManagement.Gui.Commands;
using StockManagement.Gui.ViewModel.Dialogs;
using StockManagement.Kernel;
using StockManagement.Kernel.Model;
using StockManagement.Kernel.Model.Types;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;

namespace StockManagement.Gui.ViewModel;


internal class MainViewModel : NotificationBase
{
	private ObservableCollection<StockItem> _filteredStockItems = [];

	private DialogViewModelBase? _dialog;
	private StockItem _selectedStockItem;
	private string _searchNames;
	private string _searchCodes;
	private ManufacturerType _selectedSearchManufacturer;
	private Type _selectedSearchStockItemType;
	private bool isWaitDialogVisible = false;


	public MainViewModel()
	{
		QuitCommand = new RelayCommand<string>(_ => Application.Current.Shutdown());
		MoreInfoCommand = new RelayCommand<StockItem>(stockItem => this.Dialog = new MoreInfoDialogViewModel(stockItem));
		OpenSettingsCommand = new RelayCommand<string>(_ => this.Dialog = new SettingsDialogViewModel());
		CreateStockItemCommand = new RelayCommand<string>(this.OnCreateStockItemCommand);
		ExcelImportCommand = new RelayCommand<string>(this.OnExcelImportCommand);

		((INotifyCollectionChanged)MainManagerFacade.Machines).CollectionChanged += (_, __) => this.OnRefreshSearch();
		((INotifyCollectionChanged)MainManagerFacade.SpareParts).CollectionChanged += (_, __) => this.OnRefreshSearch();
		((INotifyCollectionChanged)MainManagerFacade.Tires).CollectionChanged += (_, __) => this.OnRefreshSearch();

		this.OnRefreshSearch();
	}

	#region Properties
	public RelayCommand<string> ExcelImportCommand { get; }

	public RelayCommand<string> QuitCommand { get; }
	public RelayCommand<StockItem> MoreInfoCommand { get; }
	public RelayCommand<string> CreateStockItemCommand { get; }
	public RelayCommand<string> OpenSettingsCommand { get; }
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
	public List<Type> StockItemTypes { get; internal set; }

	public bool IsWaitDialogVisible
	{
		get => this.isWaitDialogVisible;
		set => this.SetField(ref this.isWaitDialogVisible, value);
	}

	public ObservableCollection<StockItem> FilteredStockItems
	{
		get => this._filteredStockItems;
		set => this.SetField(ref this._filteredStockItems, value);
	}

	public StockItem SelectedStockItem
	{
		get { return _selectedStockItem; }
		set { this.SetField(ref _selectedStockItem, value); }
	}

	public string SearchNames
	{
		get { return _searchNames; }
		set
		{
			this.SetField(ref _searchNames, value);
			this.OnRefreshSearch(names: true);
		}
	}

	public string SearchCodes
	{
		get { return _searchCodes; }
		set
		{
			this.SetField(ref _searchCodes, value);
			this.OnRefreshSearch(codes: true);
		}
	}

	public ManufacturerType SelectedSearchManufacturer
	{
		get { return _selectedSearchManufacturer; }
		set
		{
			this.SetField(ref _selectedSearchManufacturer, value);
			this.OnRefreshSearch(manufacturer: true);
		}
	}

	public Type SelectedSearchStockItemType
	{
		get { return _selectedSearchStockItemType; }
		set
		{
			this.SetField(ref _selectedSearchStockItemType, value);
			this.OnRefreshSearch(type: true);
		}
	}
	#endregion Properties

	private void OnCreateStockItemCommand(string param)
	{
		if (this.StockItemTypes == null)
			return;

		this.Dialog = new StockItemTypeSelectionDialogViewModel(this.StockItemTypes);
	}

	private async void OnExcelImportCommand(string obj)
	{
		var dialog = new OpenFileDialog
		{
			Filter = "Excel files (*.xlsx)|*.xlsx"
		};
		if (dialog.ShowDialog() is bool isTrue && isTrue)
		{
			GuiManager.Instance.ShowWaitDialog();
			this.Dialog = await ExcelImportDialogViewModel.CreateAsync(dialog.FileName);
			GuiManager.Instance.HideWaitDialog();
		}
	}

	private void OnDialogClosing(bool success)
	{
		if (this.Dialog == null) return;

		this.Dialog.DialogClosing -= this.OnDialogClosing;
		this.Dialog = null;
	}

	private void OnRefreshSearch(bool names = false, bool manufacturer = false, bool type = false, bool codes = false)
	{
		var filteredItems = this.GetStockItems();
		if (manufacturer)
			filteredItems = this.SelectedSearchManufacturer == ManufacturerType.None ? filteredItems : filteredItems.Where(item => item.Manufacturer == this.SelectedSearchManufacturer);
		else if (type)
			filteredItems = this.SelectedSearchStockItemType == null ? filteredItems : filteredItems.Where(item => item.GetType() == this.SelectedSearchStockItemType);
		else if (names)
			filteredItems = filteredItems.Where(item => Regex.IsMatch(item.Name.ToLower(), this.SearchNames.ToLower()));
		else if (codes)
			filteredItems = filteredItems.Where(item => Regex.IsMatch(item.Code.ToLower(), this.SearchCodes.ToLower()));
		
		this.FilteredStockItems = new ObservableCollection<StockItem>(filteredItems);
	}

	private IEnumerable<StockItem> GetStockItems()
	{
		var machines = MainManagerFacade.Machines.Cast<StockItem>();
		var spareParts = MainManagerFacade.SpareParts.Cast<StockItem>();
		var tires = MainManagerFacade.Tires.Cast<StockItem>();
		return machines.Concat(spareParts).Concat(tires);
	}
}