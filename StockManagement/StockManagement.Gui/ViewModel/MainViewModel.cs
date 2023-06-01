using StockManagement.Gui.Commands;
using StockManagement.Kernel;
using StockManagement.Kernel.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Windows;
using System.Windows.Data;

namespace StockManagement.Gui.ViewModel;


internal class MainViewModel : NotificationBase
{
	private Type _selectedStockItemType;
	private DialogViewModelBase? _dialog;


	public MainViewModel()
    {
        QuitCommand = new RelayCommand<string>(_ => Application.Current.Shutdown());
		CreateStockItemCommand = new RelayCommand<string>(this.OnCreateStockItemCommand);

		// TODO: Do events have to be detached? Right now this dies with application so now..
		MainManager.Instance.MachineManager.Machines.CollectionChanged += this.OnMachinesChanged;
		MainManager.Instance.SparePartManager.SpareParts.CollectionChanged += this.OnSparepartsChanged;
		MainManager.Instance.TireManager.Tires.CollectionChanged += this.OnTiresChanged;
	}

	#region Properties
	public RelayCommand<string> QuitCommand { get; }
    public RelayCommand<string> CreateStockItemCommand { get; }
	public DialogViewModelBase? Dialog
	{
		get { return _dialog; }
		private set { this.SetField(ref _dialog, value); }
	}
	public List<Type> StockItemTypes { get; internal set; } = new List<Type>();

	public ObservableCollection<StockItem> StockItems { get; internal set; } = new ObservableCollection<StockItem>();

	public Type SelectedStockItemType
	{
		get { return _selectedStockItemType; }
		set { this.SetField(ref _selectedStockItemType, value); }
	}
	#endregion Properties

	private void OnCreateStockItemCommand(string param)
	{
		this.Dialog = GuiManager.Instance.StockItemToViewModel[this.SelectedStockItemType];
		this.Dialog.DialogClosing += this.OnDialogClosing;
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
			if (tire != null && tire is Tire)
				this.StockItems.Add((Tire)tire);
		}
	}

	private void OnSparepartsChanged(object? sender, NotifyCollectionChangedEventArgs e)
	{
		if (e.NewItems == null) return;

		foreach (var sparePart in e.NewItems)
		{
			if (sparePart != null && sparePart is SparePart)
				this.StockItems.Add((SparePart)sparePart);
		}
	}

	private void OnMachinesChanged(object? sender, NotifyCollectionChangedEventArgs e)
	{
		if (e.NewItems == null) return;

		foreach (var machine in e.NewItems)
		{
			if (machine != null && machine is Machine)
				this.StockItems.Add((Machine)machine);
		}
	}
}