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
using System.Windows.Data;

namespace StockManagement.Gui.ViewModel;


internal class MainViewModel : NotificationBase
{
	private ObservableCollection<StockItem> _stockItems = new ObservableCollection<StockItem>();
	private ObservableCollection<StockItem> _filteredStockItems = new ObservableCollection<StockItem>();

	private DialogViewModelBase? _dialog;
	private StockItem _selectedStockItem;
	private string _searchNames;
	private ManufacturerType _selectedSearchManufacturer;
	private Type _selectedSearchStockItemType;
	private readonly object _stockItemsLock = new object();


	public MainViewModel()
	{
		QuitCommand = new RelayCommand<string>(_ => Application.Current.Shutdown());
		MoreInfoCommand = new RelayCommand<StockItem>(stockItem => this.Dialog = new MoreInfoDialogViewModel(stockItem));
		OpenSettingsCommand = new RelayCommand<string>(_ => this.Dialog = new SettingsDialogViewModel());
		CreateStockItemCommand = new RelayCommand<string>(this.OnCreateStockItemCommand);

		this.StockItems.CollectionChanged += (_, __) => this.OnRefreshSearch();

		BindingOperations.EnableCollectionSynchronization(_stockItems, _stockItemsLock);

		MainManager.Instance.MachineManager.Machines.CollectionChanged += this.OnMachinesChanged;
		MainManager.Instance.SparePartManager.SpareParts.CollectionChanged += this.OnSparepartsChanged;
		MainManager.Instance.TireManager.Tires.CollectionChanged += this.OnTiresChanged;
	}

	#region Properties
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

	public ObservableCollection<StockItem> StockItems
	{
		get => this._stockItems;
		set => this.SetField(ref this._stockItems, value);
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
	private void OnMoreInfoCommand(StockItem param)
	{
		this.Dialog = new MoreInfoDialogViewModel(param);
	}

	private void OnDialogClosing(bool success)
	{
		if (this.Dialog == null) return;

		this.Dialog.DialogClosing -= this.OnDialogClosing;
		this.Dialog = null;
	}

	private void OnRefreshSearch(bool names = false, bool manufacturer = false, bool type = false, bool location = false)
	{
		IEnumerable<StockItem> filteredItems = this.StockItems;
		if (manufacturer)
			filteredItems = this.SelectedSearchManufacturer == ManufacturerType.None ? filteredItems : filteredItems.Where(item => item.Manufacturer == this.SelectedSearchManufacturer);
		else if (type)
			filteredItems = this.SelectedSearchStockItemType == null ? filteredItems : filteredItems.Where(item => item.GetType() == this.SelectedSearchStockItemType);
		else if (names)
			filteredItems = filteredItems.Where(item => Regex.IsMatch(item.Name.ToLower(), this.SearchNames));
		
		this.FilteredStockItems = new ObservableCollection<StockItem>(filteredItems);
	}

	private void OnTiresChanged(object? sender, NotifyCollectionChangedEventArgs e)
	{
		if (e.NewItems == null) return;

		foreach (var tire in e.NewItems)
		{
			if (tire == null || tire is not Tire)
				continue;

			lock (_stockItemsLock)
			{
				this.StockItems.Add((Tire)tire);
			}
		}
	}

	private void OnSparepartsChanged(object? sender, NotifyCollectionChangedEventArgs e)
	{
		if (e.NewItems == null) return;

		foreach (var sparePart in e.NewItems)
		{
			if (sparePart == null || sparePart is not SparePart)
				continue;

			lock (_stockItemsLock)
			{
				this.StockItems.Add((SparePart)sparePart);
			}
		}
	}

	private void OnMachinesChanged(object? sender, NotifyCollectionChangedEventArgs e)
	{
		if (e.NewItems == null) return;

		foreach (var machine in e.NewItems)
		{
			if (machine == null || machine is not Machine)
				continue;

			lock (_stockItemsLock)
			{
				this.StockItems.Add((Machine)machine);
			}
		}
	}
}