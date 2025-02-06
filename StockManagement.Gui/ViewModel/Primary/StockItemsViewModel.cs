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
using System.Text.RegularExpressions;
using System.ComponentModel;
using System.Windows;
using System.Diagnostics;
using StockManagement.Kernel.Database.Interfaces;
using System.Threading.Tasks;

namespace StockManagement.Gui.ViewModel.Primary;


public class StockItemsViewModel : ViewModelBase
{
	private ObservableCollection<StockItem> _filteredStockItems;
	private StockItem _selectedStockItem;
	private string _searchNames;
	private string _searchCodes;
	private string _searchLocations;
	private ManufacturerType _selectedSearchManufacturer;
	private Type _selectedSearchStockItemType;
	private readonly List<Func<StockItem, bool>> _filterFunctions = [];
	private readonly IStockItemServiceProvider _stockItemServiceProvider;
	private readonly ICustomerServiceProvider _customerServiceProvider;
	private readonly IInvoiceServiceProvider _invoiceServiceProvider;


	private StockItemsViewModel(IStockItemServiceProvider stockItemServiceProvider, ICustomerServiceProvider customerServiceProvider, IInvoiceServiceProvider invoiceServiceProvider)
	{
		_stockItemServiceProvider = stockItemServiceProvider;
		_customerServiceProvider = customerServiceProvider;
		_invoiceServiceProvider = invoiceServiceProvider;

		this.MoreInfoCommand = new RelayCommand<StockItem>(this.OnMoreInfoCommand);
		this.CreateStockItemCommand = new RelayCommand<string>(this.OnCreateStockItemCommand);
		this.ExcelImportCommand = new RelayCommand<string>(this.OnExcelImportCommand);
		this.AddToShoppingCartCommand = new RelayCommand<IEnumerable>(this.OnAddToShoppingCartCommand);
		this.DeleteSelectedItemsCommand = new RelayCommand<IEnumerable>(this.OnDeleteSelectedItemsCommand);
		this.ShoppingCartCommand = new RelayCommand<string>(this.OnShoppingCartCommand);

		this.PropertyChanged += this.OnPropertyChangedEvent;

		this.SetupFilterConditions();
	}


	#region Properties
	public RelayCommand<string> ExcelImportCommand { get; }
	public RelayCommand<IEnumerable> DeleteSelectedItemsCommand { get; }
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

	public string SearchNames
	{
		get { return _searchNames; }
		set { this.SetField(ref _searchNames, value); }
	}

	public string SearchCodes
	{
		get { return _searchCodes; }
		set { this.SetField(ref _searchCodes, value); }
	}

	public string SearchLocations
	{
		get { return _searchLocations; }
		set { this.SetField(ref _searchLocations, value); }
	}

	public ManufacturerType SelectedSearchManufacturer
	{
		get { return _selectedSearchManufacturer; }
		set { this.SetField(ref _selectedSearchManufacturer, value); }
	}

	public Type SelectedSearchStockItemType
	{
		get { return _selectedSearchStockItemType; }
		set { this.SetField(ref _selectedSearchStockItemType, value); }
	}
	#endregion Properties

	public static Task<StockItemsViewModel> CreateAsync(IStockItemServiceProvider stockItemServiceProvider, ICustomerServiceProvider customerServiceProvider, IInvoiceServiceProvider invoiceServiceProvider)
	{
		var ret = new StockItemsViewModel(stockItemServiceProvider, customerServiceProvider, invoiceServiceProvider);
		return ret.InitializeAsync();
	}

	private async Task<StockItemsViewModel> InitializeAsync()
	{
		await this.UpdateStockItemsAsync();
		return this;
	}

	private async void OnPropertyChangedEvent(object? sender, PropertyChangedEventArgs e)
	{
		switch(e.PropertyName)
		{
			case nameof(this.SelectedSearchStockItemType):
			case nameof(this.SelectedSearchManufacturer):
			case nameof(this.SearchNames):
			case nameof(this.SearchCodes):
			case nameof(this.SearchLocations):
				await this.UpdateStockItemsAsync();
				break;
		}
	}

	private async Task UpdateStockItemsAsync()
	{
		var filteredStockItems = await _stockItemServiceProvider.GetAllStockItemsAsync().ContinueWith(task => task.Result.Where(_filterFunctions));
		this.FilteredStockItems = new(filteredStockItems);
	}

	private async void UpdateStockItemsOnSuccess(bool success)
	{
		if (!success) return;
		await this.UpdateStockItemsAsync();
	}

	private void OnCreateStockItemCommand(string param)
	{
		if (GuiManager.Instance.StockItemTypes == null)
			return;

		GuiManager.Instance.MainViewModel.Dialog = new StockItemCreationDialogViewModel(_stockItemServiceProvider, stockItem: new());
		GuiManager.Instance.MainViewModel.Dialog.DialogClosing += this.UpdateStockItemsOnSuccess;
	}

	private void OnMoreInfoCommand(StockItem stockItem)
	{
		if (stockItem == null) return;

		GuiManager.Instance.MainViewModel.Dialog = new StockItemCreationDialogViewModel(_stockItemServiceProvider, stockItem);
		GuiManager.Instance.MainViewModel.Dialog.DialogClosing += this.UpdateStockItemsOnSuccess;
	}

	private void OnAddToShoppingCartCommand(IEnumerable selectedItems)
	{
		var items = selectedItems.OfType<StockItem>().ConvertToShoppingCartList();
		
		if (items.RemoveUnavailableItems())
		{
			MessageBox.Show(Language.Resources.stockItemsRemoved, Language.Resources.information, MessageBoxButton.OK, MessageBoxImage.Information);
		}

		this.ShoppingCartItems.EqualizeTo(items);
	}

	private async void OnDeleteSelectedItemsCommand(IEnumerable selectedItems)
	{
		var items = selectedItems.OfType<StockItem>().ToList();
		var result = MessageBox.Show(string.Format(Language.Resources.selectedItemsDeletionPrompt, items.Count), Language.Resources.deleteSelectedItems, MessageBoxButton.YesNo);
		if (result != MessageBoxResult.Yes) return;

		GuiManager.Instance.ShowWaitDialog();
		foreach (var item in items)
		{
			await _stockItemServiceProvider.DeleteStockItemAsync(item);
		}
		await this.UpdateStockItemsAsync();
		GuiManager.Instance.HideWaitDialog();
	}

	private async void OnShoppingCartCommand(string obj)
	{
		var cartDialog = await ShoppingCartDialogViewModel.CreateAsync(this.ShoppingCartItems, _customerServiceProvider, _invoiceServiceProvider, _stockItemServiceProvider);
		GuiManager.Instance.MainViewModel.Dialog = cartDialog;
		GuiManager.Instance.MainViewModel.Dialog.DialogClosing += this.UpdateStockItemsOnSuccess;
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
			try
			{
				GuiManager.Instance.MainViewModel.Dialog = await ExcelImportDialogViewModel.CreateAsync(dialog.FileName, _stockItemServiceProvider);
				GuiManager.Instance.MainViewModel.Dialog.DialogClosing += this.UpdateStockItemsOnSuccess;
			}
			catch(Exception ex)
			{
				MessageBox.Show(Language.Resources.selectedFileInUse);
				Trace.WriteLine(ex.Message);
			}
			GuiManager.Instance.HideWaitDialog();
		}
	}

	private void SetupFilterConditions()
	{
		_filterFunctions.Add(item =>
		{
			return this.SelectedSearchManufacturer == ManufacturerType.None || item.Manufacturer == this.SelectedSearchManufacturer;
		});
		_filterFunctions.Add(item =>
		{
			return this.SelectedSearchStockItemType == null || item.GetType() == this.SelectedSearchStockItemType;
		});
		_filterFunctions.Add(item =>
		{
			return string.IsNullOrEmpty(this.SearchNames) || Regex.IsMatch(item.Name.ToLower(), this.SearchNames.ToLower());
		});
		_filterFunctions.Add(item =>
		{
			return string.IsNullOrEmpty(this.SearchCodes) || Regex.IsMatch(item.Code.ToLower(), this.SearchCodes.ToLower());
		});
		_filterFunctions.Add(item =>
		{
			return string.IsNullOrEmpty(this.SearchLocations) || Regex.IsMatch(item.Location.ToLower(), this.SearchLocations.ToLower());
		});
	}
}
