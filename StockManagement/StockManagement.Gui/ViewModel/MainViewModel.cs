using StockManagement.Gui.Commands;
using StockManagement.Gui.ViewModel.Dialogs;
using StockManagement.Kernel;
using StockManagement.Kernel.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Data;

namespace StockManagement.Gui.ViewModel;


internal class MainViewModel : NotificationBase
{
	private ObservableCollection<StockItem> _stockItems = new ObservableCollection<StockItem>();
	private DialogViewModelBase? _dialog;
	private StockItem _selectedStockItem;
	private readonly object _stockItemsLock = new object();


	public MainViewModel()
    {
        QuitCommand = new RelayCommand<string>(_ => Application.Current.Shutdown());
		MoreInfoCommand = new RelayCommand<StockItem>(stockItem => this.Dialog = new MoreInfoDialogViewModel(stockItem));
		OpenSettingsCommand = new RelayCommand<string>(_ => this.Dialog = new SettingsDialogViewModel());
		CreateStockItemCommand = new RelayCommand<string>(this.OnCreateStockItemCommand);

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
		get { return _stockItems; }
		internal set { this.SetField(ref _stockItems, value); }
	}

	public StockItem SelectedStockItem
	{
		get { return _selectedStockItem; }
		set { this.SetField(ref _selectedStockItem, value); }
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