using System.Collections;
using Microsoft.Win32;
using System.Linq;
using StockManagement.Gui.Commands;
using StockManagement.Gui.ViewModel.Dialogs;
using StockManagement.Kernel.Model;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System;
using StockManagement.Kernel.Model.ExtensionMethods;
using StockManagement.Kernel.Model.Types;
using StockManagement.Kernel;
using System.Text.RegularExpressions;
using System.Collections.Specialized;

namespace StockManagement.Gui.ViewModel.Primary;


public class StockItemsViewModel : ViewModelBase
{
	private ObservableCollection<StockItem> _filteredStockItems;
	private StockItem _selectedStockItem;
	private string _searchNames;
	private string _searchCodes;
	private ManufacturerType _selectedSearchManufacturer;
	private Type _selectedSearchStockItemType;
	private bool _isSearchBarVisible = false;

	public StockItemsViewModel()
	{
		this.ToggleSearchBarCommand = new RelayCommand<string>(_ => this.IsSearchBarVisible = !this.IsSearchBarVisible);
		this.MoreInfoCommand = new RelayCommand<StockItem>(stockItem => GuiManager.Instance.MainViewModel.Dialog = new MoreInfoDialogViewModel(stockItem));
		this.CreateStockItemCommand = new RelayCommand<string>(this.OnCreateStockItemCommand);
		this.ExcelImportCommand = new RelayCommand<string>(this.OnExcelImportCommand);
		this.AddToShoppingCartCommand = new RelayCommand<IEnumerable>(this.OnAddToShoppingCartCommand);
		this.ShoppingCartCommand = new RelayCommand<string>(this.OnShoppingCartCommand);

		((INotifyCollectionChanged)MainManagerFacade.Machines).CollectionChanged += (_, __) => this.OnRefreshSearch();
		((INotifyCollectionChanged)MainManagerFacade.SpareParts).CollectionChanged += (_, __) => this.OnRefreshSearch();
		((INotifyCollectionChanged)MainManagerFacade.Tires).CollectionChanged += (_, __) => this.OnRefreshSearch();

		this.OnRefreshSearch();
	}


	#region Properties
	public RelayCommand<string> ToggleSearchBarCommand { get; }
	public RelayCommand<string> ExcelImportCommand { get; }
	public RelayCommand<IEnumerable> AddToShoppingCartCommand { get; }
	public RelayCommand<string> ShoppingCartCommand { get; }
	public RelayCommand<StockItem> MoreInfoCommand { get; }
	public RelayCommand<string> CreateStockItemCommand { get; }

	public ObservableCollection<StockItem> FilteredStockItems
	{
		get => this._filteredStockItems;
		set => this.SetField(ref this._filteredStockItems, value);
	}

	public ObservableCollection<ShoppingCartItem> ShoppingCartItems { get; } = [];

	public StockItem SelectedStockItem
	{
		get { return _selectedStockItem; }
		set { this.SetField(ref _selectedStockItem, value); }
	}

	public bool IsSearchBarVisible
	{
		get { return _isSearchBarVisible; }
		set { this.SetField(ref _isSearchBarVisible, value); }
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

	private void OnRefreshSearch(bool names = false, bool manufacturer = false, bool type = false, bool codes = false)
	{
		var filteredItems = GetStockItems();
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

	private static IEnumerable<StockItem> GetStockItems()
	{
		var machines = MainManagerFacade.Machines.Cast<StockItem>();
		var spareParts = MainManagerFacade.SpareParts.Cast<StockItem>();
		var tires = MainManagerFacade.Tires.Cast<StockItem>();
		return machines.Concat(spareParts).Concat(tires);
	}

	private void OnCreateStockItemCommand(string param)
	{
		if (GuiManager.Instance.StockItemTypes == null)
			return;

		GuiManager.Instance.MainViewModel.Dialog = new StockItemTypeSelectionDialogViewModel(GuiManager.Instance.StockItemTypes);
	}

	private void OnAddToShoppingCartCommand(IEnumerable selectedItems)
	{
		var items = selectedItems.OfType<StockItem>().ConvertToShoppingCartList();

		this.ShoppingCartItems.EqualizeTo(items);
	}

	private void OnShoppingCartCommand(string obj)
	{
		var cartDialog = new ShoppingCartDialogViewModel(this.ShoppingCartItems);
		GuiManager.Instance.MainViewModel.Dialog = cartDialog;
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
			GuiManager.Instance.MainViewModel.Dialog = await ExcelImportDialogViewModel.CreateAsync(dialog.FileName);
			GuiManager.Instance.HideWaitDialog();
		}
	}
}
